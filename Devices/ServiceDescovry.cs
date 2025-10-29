using GlobalShared;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Websocket.Client;

namespace Devices
{
    public class ServiceDiscovery
    {
        public string StartupUrl { get; set; } = string.Empty;
        public string[] AvailableServices => availableServices.ToArray();
        public Dictionary<string, Device> Devices { get; set; }  = new();

        private Utils utils = new Utils("ServiceDiscovery");
        private List<string> availableServices = new List<string>();
        private List<WebsocketClient> publishers = new List<WebsocketClient>();
        //private Utils utils = new Utils("ServiceDiscovery");

        public ServiceDiscovery()
        {
            //get base url from configuration.
            StartupUrl = "ws://localhost:@/xfs4iot/v1.0/";
        }

        public ServiceDiscovery(string url)
        {
            StartupUrl = url;
        }

        public void GetPublishers()
        {
            foreach (var port in XFS4IoTConstants.PortRanges)
            //Parallel.ForEach(XFS4IoTConstants.PortRanges, async (port) =>
            {
                try
                {
                    var url = StartupUrl.Replace("@", port.ToString());
                    utils.LogInfo($"opening port: {port} - url: {url}");
                    WebsocketClient wc = new WebsocketClient(new Uri(url));
                    wc.Start().GetAwaiter().GetResult();
                    if (wc.IsRunning)
                    {
                        publishers.Add(wc);
                        utils.LogInfo($">>>>>> publisher connected on Port: {port} >>>>>>>>>>>>");
                        wc.MessageReceived.Subscribe(msg => Wc_OnMessage(msg.Text));
                    }
                    else
                    {
                        utils.LogInfo($">>>>>> publisher NOT connected on Port: {port}  >>>>>>>>>>>>");
                        wc.Dispose();
                    }
                }
                catch (Exception e)
                {
                    utils.LogWarning($">>>>> Exception oppening port={port}: {e.Message}");
                }
            };
            //);

            if (publishers.Count < 1)
            {
                utils.LogWarning("###No publisher diteceted####");
                throw new NoServicesFoundEx("No publisher was detected.");
                //maybe pause for few mins and try again
                //Thread.Sleep(3 * 60 * 1000);
                //GetPublishers();
            }
        }

        public async void GetServices(int timeout = 60000)
        {
            //var acknoleged = Task.WaitAll(new Task[] { TGetServices() }, timeout);
            //if (!acknoleged)
            //{

            //}
            await TGetServicesAsync(timeout).ConfigureAwait(false);
        }

        private Task TGetServicesAsync(int timeout)
        {
            utils.LogInfo($">>>>>> getting services >>>>>>>>>>>>");
            return Task.Run(() =>
            {
                try
                {
                    foreach (var publisher in publishers)
                    {
                        utils.LogInfo($">>>>>> Sending  GetServices CMD >>>>>>>>>>>>");
                        var GetServicesCmd = new Command("ServicePublisher.GetServices", 120000);
                        if (publisher.IsRunning)
                            publisher.Send(GetServicesCmd.ToJson());
                        else
                            utils.LogWarning($"###this publisher is not connected anymore ####");
                        utils.LogInfo($"MESSEGE SENT TO PUBLISHER");
                    }
                }
                catch (Exception e)
                {
                    utils.LogWarning($"###Exceptin in sending GetService command #### {e.Message}");
                    throw;
                }
            });
        }

        private void Wc_OnMessage(string msg)
        {
            utils.LogInfo($"A service response received from Publisher: {msg}");
            Message response = Command.FromJson(msg);// JsonConvert.DeserializeObject<CommandMessage>(e.Data);

            if (response.Header.Type == MessageType.Acknowledge)
            {
                utils.LogInfo($"A acknowleog response received from Publisher: {response.Header.RequestId}");
                var status = response.Header.Status;
                if (string.IsNullOrEmpty(status))
                    return;
                else
                {
                    utils.LogWarning($"Bad Acknowledge received {status}");
                    throw new InternalException(InternalException.ExceptionType.InvalidAcknowlege, $"Invalid AKC {status} received for cmd id {response.Header.RequestId}");
                }
            }
            else if (response.Header.Type == MessageType.Event)
            {
                utils.LogInfo($"An event response received from Publisher for command id: {response.Header.RequestId}");
                try
                {
                    var services = response.GetPayloadValue<object[]>("services");
                    utils.LogInfo($"Found {services?.Length ?? 0} services.");
                    for (int i = 0; i < services?.Length; i++)
                    {
                        availableServices.Add(response.GetPayloadValue<string>($"services[{i}].serviceURI") ?? string.Empty);
                    }
                }
                catch (RuntimeBinderException ex)
                {
                    throw new InternalException(InternalException.ExceptionType.InvalidEvent, $"Invalid event data received for cmd id: {response.Header.RequestId}");
                }
            }
            else if (response.Header.Type == MessageType.Completion)
            {
                if (availableServices.Count == 0)
                {
                    utils.LogInfo($"No events received, only comlition event!");
                    var services = response.GetPayloadValue<object[]>("services");
                    utils.LogInfo($"Found {services?.Length ?? 0} services.");
                    for (int i = 0; i < services?.Length; i++)
                    {
                        var srviceUri = response.GetPayloadValue<string>($"services[{i}].serviceURI") ?? string.Empty;
                        availableServices.Add(srviceUri);
                    }
                }
            }
            else
            {
                throw new InternalException(InternalException.ExceptionType.WrongCommandTypeEx, $"Wrong command type received {response.Header.Type}");
            }
        }

        public void InitializeDevices()
        {
            Command getStatus = new Command("Common.Status", 60000);
            foreach (var url in AvailableServices)
            {
                try
                {
                    utils.LogInfo($"===>opning connection to {url}");
                    var client = new WebsocketClient(new Uri(url));
                    client.Start().GetAwaiter().GetResult();
                    if(client.IsRunning)
                    {
                        client.Send(getStatus.ToJson());
                        client.MessageReceived.Subscribe(msg => 
                        {
                            utils.LogInfo($"Message received from service point {url}: {msg.Text}");
                            var response = Command.FromJson(msg.Text);
                            if(response.Header.Type == MessageType.Completion)
                            {
                                InitDevice(response);
                            }
                        });
                    }
                    else
                    {
                        utils.LogWarning($">>>>>> Device NOT connected at service piont: {url}  >>>>>>>>>>>>");
                        client.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    utils.LogWarning($"###Exception initializing device at {url}: {ex.Message}");
                }
            }
        }

        private void InitDevice(Message res)
        {
            var missing = res.GetPayloadObject<object>("CardReader");
            if (missing != null)
            {

            }
        }

        //var root = JObject.Parse(jsonString);
        //var guestValues = root["Results"][0]["GuestValues"].ToObject<GuestValue[]>();
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
