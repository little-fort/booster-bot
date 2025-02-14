using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BoosterBot
{
    internal static class SettingsHelper
    {
        public static void Save(string key, object value)
        {
            // Parse configuration file
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

            // Read contents of current appsettings.json
            var json = File.ReadAllText(configPath);

            // Update the value of the specified key
            dynamic jsonObj = JsonConvert.DeserializeObject(json);
            jsonObj[key] = JToken.FromObject(value);

            // Write the updated settings to appsettings.json
            string output = JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(configPath, output);
        }
    }
}
