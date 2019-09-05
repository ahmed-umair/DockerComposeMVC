using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DockerComposeMVC
{
    public static class Configuration
    {
        public static string ConfigFilePath = Path.Combine(Directory.GetCurrentDirectory(), "config\\config.json");
        public static bool ConfigLoaded = false;
        public static Dictionary<string, string> CurrentConfig = null;

        public static bool ReadConfigFile()
        {
            try
            {
                string json = File.ReadAllText(ConfigFilePath);
                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                CurrentConfig = values;
                ConfigLoaded = true;
                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("CONFIG_FILE_READ_ERROR: " + e.Message);
                return false;
            }
        }

        public static bool StoreConfigFile()
        {
            if (!IsConfigLoaded())
            {
                return false;
            }

            else
            {
                try
                {
                    string json = JsonConvert.SerializeObject(CurrentConfig);
                    File.WriteAllText(ConfigFilePath, json);
                    return true;
                }

                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    return false;
                }
            }

        }

        public static bool IsConfigLoaded()
        {
            return ConfigLoaded;
        }

        public static Dictionary<string, string> GetConfig()
        {
            if (!IsConfigLoaded())
            {
                throw new Exception("CONFIG_NOT_SET");
            }
            else
            {
                return CurrentConfig;
            }
        }
        public static bool SetConfig(Dictionary<string, string> NewConfig)
        {
            if (!IsConfigLoaded())
            {
                return false;
            }
            else
            {
                bool keysValid = true;
                foreach (var key in CurrentConfig.Keys)
                {
                    if (!NewConfig.ContainsKey(key))
                    {
                        keysValid = false;
                        return keysValid;
                    }
                }
                CurrentConfig = NewConfig;
                return true;
            }
        }
    }
}
