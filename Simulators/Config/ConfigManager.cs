using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Simulators.Config
{
    public class SimulatorConfig
    {
        public string VendorName { get; set; } = "Unknown Vendor";
        public bool SecureConnections { get; set; } = false;
        public List<DeviceConfig> Devices { get; set; } = new();

        public static SimulatorConfig Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Simulator configuration not found: {path}");

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<SimulatorConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new SimulatorConfig();
        }
    }

    public class DeviceConfig
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public int Port { get; set; } = 8080;
    }
}
