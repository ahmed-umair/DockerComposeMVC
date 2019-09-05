using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using DockerComposeMVC.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DockerComposeMVC
{
    public class ComposeFileOperationsNew
    {
        public static List<CompositeModel> LoadCompositesFromFiles(string directory, bool ready)
        {
            var config = Configuration.GetConfig();
            var list = new List<CompositeModel>();
            var FolderPath = directory;
            string[] fileNames = Directory.GetFiles(FolderPath);

            foreach (var FileName in fileNames)
            { 
                var composite = LoadCompositeFromSingleFile(FileName, ready);
                list.Add(composite);
            }
            return list;
        }

        public static CompositeModel LoadCompositeFromSingleFile(string FileName, bool ready) {

            var yaml = new StreamReader(FileName);
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize(yaml);

            var serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();

            var json = serializer.Serialize(yamlObject);
            var name = FileName.Substring(FileName.LastIndexOf('\\') + 1);
            var composite = new CompositeModel
            {
                Name = name,
                FilePath = FileName,
                ContainersFromFile = JSONtoContainers(json),
                ReadyForExecution = ready
            };

            if (ready)
            {
                composite.Service = new Builder()
                                  .UseContainer()
                                  .FromComposeFile(composite.FilePath)
                                  .RemoveOrphans()
                                  .KeepVolumes()
                                  .ForceRecreate()
                                  .ServiceName(composite.Name)
                                  .Build();
            }
            return composite;
        }


        private static List<ContainerModel> JSONtoContainers(string json)
        {
            var list = new List<ContainerModel>();
            JObject root = JsonConvert.DeserializeObject<JObject>(json);
            JObject services = root["services"].Value<JObject>();
            foreach (var service in services)
            {
                var container = new ContainerModel { Name = service.Key };
                var jsonContainer = (JObject)service.Value;

                //add image to ContainerModel
                if (!(jsonContainer["image"] is null))
                {
                    container.Image = jsonContainer["image"].Value<string>();
                }
                else if (jsonContainer["image"] is null && !(jsonContainer["build"] is null))
                {
                    container.Image = "Image will be built from directory";
                }
                //add platform
                if (!(jsonContainer["platform"] is null))
                {
                    container.Platform = jsonContainer["platform"].Value<string>();
                }
                else
                {
                    container.Platform = "Linux";
                }

                //add environment variables
                if (!(jsonContainer["environment"] is null))
                {
                    if (jsonContainer["environment"].Type is JTokenType.Array)
                    {
                        container.EnvironmentVariables = ((JArray)jsonContainer["environment"]).ToObject<List<string>>();
                    }
                    else if (jsonContainer["environment"].Type is JTokenType.Object)
                    {
                        container.EnvironmentVariables = jsonContainer["environment"].ToObject<Dictionary<string, string>>();
                    }
                }

                //add port mappings
                if (!(jsonContainer["ports"] is null))
                {
                    container.PortMappings = jsonContainer["ports"].ToObject<List<string>>();
                }

                //add volume bindings
                if (!(jsonContainer["volumes"] is null))
                {
                    if (jsonContainer["volumes"][0].Type is JTokenType.Object)
                    {
                        container.Volumes = jsonContainer["volumes"].ToObject<List<Dictionary<string, string>>>();
                    }

                    if (jsonContainer["volumes"][0].Type is JTokenType.String)
                    {
                        container.Volumes = jsonContainer["volumes"].ToObject<List<string>>();
                    }
                }

                list.Add(container);
            };
            return list;
        }


        public static bool AddNewComposeTemplate(string TemplateContent, string FileName)
        {
            try
            {
                var config = Configuration.GetConfig();
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), config.GetValueOrDefault("ComposeTemplate", "data/templates")), TemplateContent);
                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return false;
            }
        }


        public static bool RemoveComposeTemplate(string FileName)
        { return true; }
        
        public static bool AddComposeReadyFile(string content, string FileName) { return true; }
        public static bool RemoveComposeReadyFile(string FileName) { return true; }
        public static Dictionary<string, string> GetParamsListFromFile(string FileName) { return new Dictionary<string, string>(); }
    }
}
