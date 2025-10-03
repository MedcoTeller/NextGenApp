using Devices.Commands;
using Devices.Events;
using GlobalShared;
using Websocket.Client;

namespace Devices
{
    public class Device
    {
        private readonly WebsocketClient _client;
        private readonly Queue<DeviceEvent> _events = new();
        private readonly object _lock = new();
        private readonly AutoResetEvent _newEvent = new(false);
        internal WaitHandle WaitHandle => _newEvent;
        public string Name { get; }
        public string Uri { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class with the specified name and URI.
        /// Sets up the websocket client and starts listening for messages.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <param name="uri">The URI endpoint for the device connection.</param>
        protected Device(string name, string uri)
        {
            Name = name;
            Uri = uri;

            _client = new WebsocketClient(new Uri(uri));
            _client.MessageReceived.Subscribe(msg =>
            {
                if (!string.IsNullOrEmpty(msg.Text))
                    OnMessage(msg.Text);
            });

            _client.Start();
        }

        /// <summary>
        /// Sends a JSON command to the device endpoint via the websocket client.
        /// </summary>
        /// <param name="cmd">The command to send.</param>
        protected void SendCommand(Command cmd)
        {
            _client.Send(cmd.JsonString);
        }

        /// <summary>
        /// Handles incoming JSON messages from the websocket, converts them into <see cref="DeviceEvent"/> objects, and enqueues them for processing.
        /// </summary>
        /// <param name="json">The incoming JSON message.</param>
        protected virtual void OnMessage(string json)
        {
            DeviceEvent evt = Xfs4DynamicParser.Parse(json) as DeviceEvent;

            lock (_lock)
            {
                _events.Enqueue(evt);
            }
            _newEvent.Set();
        }

        /// <summary>
        /// Blocks until the next device event is available or the specified timeout elapses.
        /// </summary>
        /// <param name="timeoutMs">The timeout in milliseconds to wait for an event.</param>
        /// <returns>The next <see cref="DeviceEvent"/> if available; otherwise, null.</returns>
        public DeviceEvent? GetEvent(int timeoutMs)
        {
            if (_newEvent.WaitOne(timeoutMs))
            {
                lock (_lock)
                {
                    if (_events.Count > 0)
                        return _events.Dequeue();
                }
            }
            return null;
        }

        /// <summary>
        /// Waits for any of the specified devices to signal an event, or until the timeout elapses.
        /// </summary>
        /// <param name="devices">An array of devices to monitor for events.</param>
        /// <param name="timeoutMs">The timeout in milliseconds to wait for any event.</param>
        /// <returns>The index of the first device with an event, or -1 if the timeout occurred.</returns>
        public static int WaitAny(Device[] devices, int timeoutMs)
        {
            var handles = new WaitHandle[devices.Length];
            for (int i = 0; i < devices.Length; i++)
                handles[i] = devices[i].WaitHandle;

            // Returns index of the first signaled device, or WaitHandle.WaitTimeout
            var result = WaitHandle.WaitAny(handles, timeoutMs);
            if ( result == WaitHandle.WaitTimeout)
                return -1; // timeout

            return result;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
