namespace Simulators.Services
{
    public interface IDeviceService
    {
        string DeviceType { get; }
        int Port { get; }

        void Start();
        void Stop();
    }
}
