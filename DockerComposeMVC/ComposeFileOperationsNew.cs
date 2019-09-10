using DockerComposeMVC.Models;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace DockerComposeMVC
{
    public class ComposeFileOperationsNew
    {
        /// <summary>
        /// This method returns a list of CompositeModels from a given folder. It also loads a DockerFluent Service object 
        /// into the composite model if the IsTemplate parameter is sent as false;
        /// DEPENDENT ON: LoadSingleCompositeModel()
        /// </summary>
        /// <param name="directory">The directory from which to load files from</param>
        /// <param name="IsTemplate">Indicates if the directory contains template files (with variables that need to be populated) or ready-to-run compose files</param>
        /// <returns>A list of CompositeModels</returns>
        public static List<CompositeModel> LoadCompositesFromFiles(string directory, bool IsTemplate)
        {
            var config = Configuration.GetConfig();
            var list = new List<CompositeModel>();
            var FolderPath = directory;
            DirectoryInfo dirInfo = new DirectoryInfo(directory);
            FileInfo[] fileNames = dirInfo.GetFiles().OrderByDescending(p => p.CreationTime).ToArray();

            foreach (var FileName in fileNames)
            {
                var composite = LoadCompositeFromSingleFile(FileName.FullName, IsTemplate);
                list.Add(composite);
            }
            return list;
        }

        /// <summary>
        /// This method parses a given YAML file and returns a CompositeModel based on it. 
        /// It can also create a service according to the params specified in the file
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="IsTemplate"></param>
        /// <returns>A single CompositeModels</returns>
        public static CompositeModel LoadCompositeFromSingleFile(string FilePath, bool IsTemplate)
        {

            var yaml = new StreamReader(FilePath);
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize(yaml);

            var serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();

            var json = serializer.Serialize(yamlObject);
            var name = FilePath.Substring(FilePath.LastIndexOf('\\') + 1);
            var composite = new CompositeModel
            {
                Name = name,
                FilePath = FilePath,
                ContainersFromFile = JSONtoContainers(json),
                IsTemplate = IsTemplate,
                dateTimeAdded = DateTime.UtcNow
            };

            if (!IsTemplate)
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
        public static bool AddToTemplatesFromFile(string SourcePath, string TemplateName, out string status)
        {
            try
            {
                var fileString = File.ReadAllText(SourcePath);
                if (File.Exists(Path.Combine(Program.ComposeTemplateDir, TemplateName)))
                {
                    status = "ERR_TEMPLATE_EXISTS";
                    return false;
                }

                try
                {
                    File.WriteAllText(Path.Combine(Program.ComposeTemplateDir, TemplateName), fileString);
                    AddComposeTemplateToList(TemplateName);
                    status = "SUCCESS";
                    return true;
                }
                catch
                {
                    status = "ERR_WRITE_FAILED";
                    return false;
                }
            }
            catch
            {
                status = "ERR_READ_FAILED_FROM_TEMPORARY";
                return false;
            }
        }

        public static bool RemoveFileFromTemplatesFolder(string FileName)
        {
            try
            {
                File.Delete(Path.Combine(Program.ComposeTemplateDir, FileName));
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool AddComposeTemplateToList(string FileName)
        {
            try
            {
                var newComposite = LoadCompositeFromSingleFile(Path.Combine(Program.ComposeTemplateDir, FileName), true);
                if (!ComposerNew.TemplatesList.Contains(newComposite))
                {
                    ComposerNew.TemplatesList.Add(newComposite);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine(e.Message);
                return false;
            }
        }

        public static bool RemoveComposeTemplateFromList(string FileName)
        {
            try
            {
                var searchResult = ComposerNew.TemplatesList.Single(service => service.Name == FileName);
                RemoveFileFromTemplatesFolder(searchResult.Name);
                ComposerNew.TemplatesList.Remove(searchResult);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool AddToReadyList(string FileName)
        {

            var newComposite = LoadCompositeFromSingleFile(Path.Combine(Program.ComposeReadyDir, FileName), false);
            if (!ComposerNew.ReadyList.Contains(newComposite))
            {
                ComposerNew.ReadyList.Add(newComposite);
                return true;
            }
            return false;


        }
        public static bool RemoveFromReadyList(string FileName)
        {
            try
            {
                var searchResult = ComposerNew.ReadyList.Single(service => service.Name == FileName);
                if ((searchResult.Service.State is ServiceRunningState.Running || searchResult.Service.State is ServiceRunningState.Starting || searchResult.Service.State is ServiceRunningState.Stopping))
                {
                    throw new Exception("ERR_CANNOT_DELETE_RUNNING_COMPOSE_FILE");
                }
                var filename = searchResult.Name;
                ComposerNew.ReadyList.Remove(searchResult);
                RemoveFileFromReadyFolder(filename);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static string WriteFileToReadyFolder(string contents, string templateName, string instanceName)
        {
            try
            {
                string filename = templateName.Substring(0, templateName.Length - 4) + "_" + instanceName + "_" + System.DateTime.Now.ToShortDateString() + ".yml";
                File.WriteAllText(Path.Combine(Program.ComposeReadyDir, filename), contents);
                System.Diagnostics.Debug.WriteLine("-------------------------");
                System.Diagnostics.Debug.WriteLine(filename);
                AddToReadyList(filename);
                return filename;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return "ERR_UNABLE_TO_WRITE_TO_FILE";
            }
        }

        public static bool RemoveFileFromReadyFolder(string FileName)
        {
            var RunningList = ComposerNew.GetRunningServices();
            try
            {
                var searchResult = RunningList.Where(service => service.Name == FileName);
                if (searchResult.Count<CompositeModel>() == 0)
                {
                    File.Delete(Path.Combine(Program.ComposeReadyDir, FileName));
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
