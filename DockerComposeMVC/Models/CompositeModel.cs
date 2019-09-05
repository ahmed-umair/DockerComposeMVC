using Ductus.FluentDocker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DockerComposeMVC.Models
{
    public class CompositeModel
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public List<ContainerModel> ContainersFromFile { get; set; }
        public bool ReadyForExecution { get; set; }
        public ICompositeService Service { get; set; }

    }
}
