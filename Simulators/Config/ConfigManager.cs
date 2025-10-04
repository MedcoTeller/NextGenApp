using System.IO;
using System.Text.Json;

namespace Simulators.Config
{
    public class SimulatorConfig
    {
        public List<DeviceConfig> Devices { get; set; } = new();
    }

    public class DeviceConfig
    {
        public string DeviceType { get; set; } = "";
        public int Port { get; set; }
    }

    public static class ConfigManager
    {
        private static readonly string ConfigPath = "simulators.json";

        public static SimulatorConfig Load()
        {
            if (!File.Exists(ConfigPath))
                return new SimulatorConfig();

            string json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<SimulatorConfig>(json) ?? new SimulatorConfig();
        }

        public static void Save(SimulatorConfig config)
        {
            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
    }
}
