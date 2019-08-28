using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ductus.FluentDocker;
using Ductus.FluentDocker.Services;

namespace DockerComposeMVC.Models
{
    public class ContainerModel
    {
        public ContainerModel(IContainerService container)
        {
            this.Id = container.Id;
            this.Name = container.Name;
            this.Image = container.Image.Name;
            this.IsWindows = container.IsWindowsContainer;
            this.State = container.State.ToString();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public bool IsWindows { get; set; }
        public string State { get; set; }
    }
}
