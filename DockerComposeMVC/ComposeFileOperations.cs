using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DockerComposeMVC
{
    public static class ComposeFileOperations
    {
        public static string ReadFile(string filePath = "/config/compose-source-debug.yml")
        {
            string contents = File.ReadAllText(Path.GetFullPath(Directory.GetCurrentDirectory() + filePath));
            return contents;

        }

        public static string ReplaceParams(string content, Dictionary<string, string> paramsList)
        {
            var keys = paramsList.Keys;

            foreach (string key in keys)
            {
                content = content.Replace("${{"+key+"}}", paramsList.GetValueOrDefault(key));
            }

            return content;
        }

        public static bool WriteToFile(string contents, string filePath = "/config/compose-destination.yml")
        {
            try
            {
                File.WriteAllText(Path.GetFullPath(Directory.GetCurrentDirectory() + filePath), contents);
                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return false;
        }

    }
}
