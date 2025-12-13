using Devices.Common;
using GlobalShared;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using WatsonWebsocket;
using Websocket.Client;

namespace Devices
{
    /// <summary>
    /// Discovers publisher services and their device endpoints using XFS4IoT ServicePublisher.
    /// </summary>
    public class ServiceDiscovery
    {
        public string StartupUrl { get; set; } = string.Empty;
        public string[] AvailableServices => availableServices.ToArray();
        public Dictionary<string, Device> Devices { get; set; } = new();

        private readonly Utils utils = new("ServiceDiscovery");
        private readonly List<string> availableServices = new();
        private readonly List<WatsonWsClient> publishers = new();

        /// <summary>
        /// Initializes new instance using default configuration.
        /// </summary>
        public ServiceDiscovery()
        {
            StartupUrl = "ws://localhost:@/xfs4iot/v1.0/";
        }

        /// <summary>
        /// Initializes new instance using custom publisher URL pattern.
        /// </summary>
        public ServiceDiscovery(string url)
        {
            StartupUrl = url;
        }

        /// <summary>
        /// Scans all ports and connects to available publishers.
        /// </summary>
        public async Task GetPublishers()
        {
            var tasks = new List<Task>();

            foreach (var port in XFS4IoTConstants.PortRanges)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var url = StartupUrl.Replace("@", port.ToString());
                        utils.LogInfo($"Opening port: {port} - url: {url}");

                        var client = new WatsonWsClient(new Uri(url));
                        client.MessageReceived += (s, msg) => Wc_OnMessage(Encoding.UTF8.GetString(msg.Data));

                        await client.StartAsync().ConfigureAwait(false);

                        if (client.Connected)
                        {
                            publishers.Add(client);
                            utils.LogInfo($">>>>>>> Publisher connected on Port: {port} >>>>>>>>>>");
                        }
                        else
                        {
                            utils.LogInfo($">>>>>>> Publisher NOT connected on Port: {port} >>>>>>>>>>");
                            client.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        utils.LogWarning($">>>>> Exception opening port={port}: {e.Message}");
                    }
                }));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            if (publishers.Count < 1)
            {
                utils.LogWarning("### No publisher detected ####");
                //throw new NoServicesFoundEx("No publisher was detected.");
            }
        }

        /// <summary>
        /// Sends GetServices command to all publishers.
        /// </summary>
        public async Task GetServices(int timeout = 60000)
        {
            await SendGetServicesCommands(timeout).ConfigureAwait(false);
        }

        private Task SendGetServicesCommands(int timeout)
        {
            utils.LogInfo(">>>>>> Sending GetServices to all publishers >>>>>>>>");
            return Task.Run(() =>
            {
                try
                {
                    foreach (var publisher in publishers)
                    {
                        var getServicesCmd = new Command("ServicePublisher.GetServices", timeout);

                        if (publisher.Connected)
                        {
                            publisher.SendAsync(getServicesCmd.ToJson());
                        }
                        else
                        {
                            utils.LogWarning("### Publisher is no longer connected ####");
                        }
                    }
                }
                catch (Exception e)
                {
                    utils.LogWarning($"### Exception sending GetServices #### {e.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Handler for messages received from publisher services.
        /// </summary>
        private void Wc_OnMessage(string msg)
        {
            utils.LogInfo($"A service response received from Publisher: {msg}");
            var response = Command.FromJson(msg);

            if (response.Header.Type == MessageType.Acknowledge)
            {
                utils.LogInfo($"Acknowledge received for RequestId {response.Header.RequestId}");
                if (!string.IsNullOrEmpty(response.Header.Status))
                {
                    throw new InternalException(
                        InternalException.ExceptionType.InvalidAcknowlege,
                        $"Invalid ACK {response.Header.Status} for RequestId {response.Header.RequestId}"
                    );
                }
            }
            else if (response.Header.Type == MessageType.Event)
            {
                try
                {
                    var services = response.GetPayloadValue<object[]>("services");
                    utils.LogInfo($"Found {services?.Length ?? 0} services.");

                    for (var i = 0; i < services?.Length; i++)
                    {
                        availableServices.Add(
                            response.GetPayloadValue<string>($"services[{i}].serviceURI") ?? string.Empty
                        );
                    }
                }
                catch
                {
                    throw new InternalException(
                        InternalException.ExceptionType.InvalidEvent,
                        $"Invalid event payload for cmd id {response.Header.RequestId}"
                    );
                }
            }
            else if (response.Header.Type == MessageType.Completion)
            {
                if (availableServices.Count == 0)
                {
                    var services = response.GetPayloadValue<object[]>("services");
                    utils.LogInfo($"No prior Event. Services = {services?.Length ?? 0}");

                    for (var i = 0; i < services?.Length; i++)
                    {
                        availableServices.Add(response.GetPayloadValue<string>($"services[{i}].serviceURI") ?? string.Empty);
                    }
                }
            }
            else
            {
                throw new InternalException(
                    InternalException.ExceptionType.WrongCommandTypeEx,
                    $"Invalid command type: {response.Header.Type}"
                );
            }
        }

        /// <summary>
        /// Connects to each discovered Service URI and initializes devices.
        /// </summary>
        public void InitializeDevices()
        {
            var getStatus = new Command("Common.Status", 60000);
            var tasks = new List<Task>();
            foreach (var url in AvailableServices)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {

                        utils.LogInfo($"===> Opening connection to {url}");
                        var client = new WatsonWsClient(new Uri(url));
                        client.StartAsync().GetAwaiter().GetResult();

                        if (client.Connected)
                        {
                            client.SendAsync(getStatus.ToJson());
                            client.MessageReceived += (s, msg) =>
                            {
                                var message = Encoding.UTF8.GetString(msg.Data);
                                utils.LogInfo($"Message received from service point {url}: {message}");
                                var response = Command.FromJson(message);

                                if (response.Header.Type == MessageType.Completion)
                                {
                                    InitDevice(response);
                                }
                            };
                        }
                        else
                        {
                            utils.LogWarning($">>>>>>> Device NOT connected at service point: {url} >>>>>>>>>>");
                            client.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        utils.LogWarning($"### Exception initializing device at {url}: {ex.Message}");
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
        }

        private void InitDevice(Message res)
        {
            var obj = res.GetPayloadObject<object>("CardReader");
            if (obj != null)
            {
                // Future initialization logic here
            }
        }
    }

    static class XFS4IoTConstants
    {
        /// <summary>
        /// XFS4IoT mandated port options. 
        /// </summary>
        public static readonly int[] PortRanges = new int[]
        {
            80,  // Only for HTTP
            443, // Only for HTTPS
            5846,
            5847,
            5848,
            5849,
            5850,
            5851,
            5852,
            5853,
            5854,
            5855,
            5856
        };

        /// <summary>
        /// Service classes 
        /// </summary>
        /// <remarks>
        /// Use .ToString() to get service class name strings. 
        /// </remarks>
        public enum ServiceClass { CardReader, Publisher, Printer, TextTerminal, PinPad };


    }
}
