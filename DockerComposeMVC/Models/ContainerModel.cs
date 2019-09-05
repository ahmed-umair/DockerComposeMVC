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
            this.Image = config.Image;
            this.IsWindows = container.IsWindowsContainer;
            this.State = container.State.ToString();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string State { get; set; }
        public bool IsWindows { get; set; }
        public List<string> PortMappings { get; set; }
        public Object EnvironmentVariables { get; set; }
        public string Platform { get; set; }
        public Object Volumes { get; set; }
    }
}
