using Ductus.FluentDocker.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DockerComposeMVC
{
    public class ComposeFileOperationsNew
    {
        private static readonly string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "config/config.txt");

        public static List<ICompositeService> GetComposeTemplatesFiles() { return new List<ICompositeService>(); }
        public static bool AddNewComposeTemplate() { return true; }
        public static bool RemoveComposeTemplate() { return true; }
        public static List<ICompositeService> GetComposeReadyFiles() { return new List<ICompositeService>(); }
        public static bool RemoveComposeReadyFile(string FileName) { return true; }
        public static bool BuildFromTemplate() { return true; }
        public static Dictionary<string, string> GetParamsListFromFile(string FileName) { return new Dictionary<string, string>(); }
    }
}
