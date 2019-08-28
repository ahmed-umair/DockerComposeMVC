using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace DockerComposeMVC.Models
{
    [DataContract]
    public static class Params
    {
        private const string paramsListFilePath = "config/paramsList.json";

        public static List<string> ReadParamsList(string path = paramsListFilePath)
        {
            string json = File.ReadAllText(Path.GetFullPath(paramsListFilePath));
            List<string> paramsList = JsonConvert.DeserializeObject<List<string>>(json);

            return paramsList;
        }

        public static bool CheckParams(Dictionary<string, string> dict)
        {
            List<string> requiredParams = ReadParamsList();

            if (dict.Count >= requiredParams.Count)
            {
                foreach (string item in requiredParams)
                {
                    if (!dict.ContainsKey(item))
                    {
                        return false;
                    }
                }
                if (dict.Count > requiredParams.Count) {
                    System.Diagnostics.Debug.WriteLine("Additional parameters were sent. Please check the request");
                }

                return true;
            }
            return false;
        }
    }
}
