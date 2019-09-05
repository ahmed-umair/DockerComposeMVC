using DockerComposeMVC.Models;
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
        private static List<CompositeModel> TemplatesList = new List<CompositeModel>();
        private static List<CompositeModel> ReadyList = new List<CompositeModel>();
        private static readonly string BasePath = Path.Combine(Directory.GetCurrentDirectory(), "config");

        public static void InitializeLists() {
            TemplatesList = ComposeFileOperationsNew.LoadCompositesFromFiles(Path.Combine(Directory.GetCurrentDirectory(), @"data\templates"), false);
            ReadyList = ComposeFileOperationsNew.LoadCompositesFromFiles(Path.Combine(Directory.GetCurrentDirectory(), @"data\ready"), true);
        }
        public static string StartFromTemplate(string ServiceName, Dictionary<string, string> dict) { return "started"; }
        public static string StartFromReady(string ServiceName) { return "started"; }
        public static string GetServiceStatus(string ServiceName) { return "status"; }
        public static List<IContainerService> GetServiceDetails(string ServiceName) { return null; }
        public static string StopService(string ServiceName) { return "stopped"; }
        
    }
}
