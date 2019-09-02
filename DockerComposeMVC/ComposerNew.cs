using Ductus.FluentDocker.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DockerComposeMVC
{
    public class ComposerNew
    {
        private static List<ICompositeService> AllServicesList = new List<ICompositeService>();
        private static List<ICompositeService> RunningServicesList = new List<ICompositeService>();
        private static readonly string BasePath = Path.Combine(Directory.GetCurrentDirectory(), "config");

        public static void PopulateAllServicesList() { }
        public static string StartService(string ServiceName) { return "started"; }
        public static string GetServiceStatus(string ServiceName) { return "status"; }
        public static List<IContainerService> GetServiceDetails(string ServiceName) { return null; }
        public static string StopService(string ServiceName) { return "stopped"; }
        
    }
}
