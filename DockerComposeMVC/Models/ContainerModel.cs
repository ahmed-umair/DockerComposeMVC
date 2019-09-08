using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ductus.FluentDocker;
using Ductus.FluentDocker.Services;

namespace DockerComposeMVC.Models
{
    public class ContainerModel
    {
        public ContainerModel() { }
        public ContainerModel(IContainerService container)
        {
            var config = container.GetConfiguration();
            this.Id = container.Id;
            this.Name = config.Name;
            this.Image = container.Image.Name;
            this.IsWindows = container.IsWindowsContainer;
            this.State = container.State.ToString();

        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string State { get; set; }
        public bool IsWindows { get; set; }
        
        public List<string> PortMappings { get; set; }
        ///Check for type: could be List<string> or Dictionary<string,string> depending on how it's defined in the YAML file
        public Object EnvironmentVariables { get; set; }
        public string Platform { get; set; }
        ///Check for type: could be List<Dictionary<string,string>> or List<string> depending on  how it's defined in the YAML file
        public Object Volumes { get; set; }
    }
}
