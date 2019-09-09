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
        public bool IsTemplate { get; set; }
        public ICompositeService Service { get; set; }

        public void UpdateContainersStatus()
        {
            foreach (var container in Service.Containers)
            {
                try
                {
                    var searchResult = ContainersFromFile.Single(ContModel => ContModel.Name == container.Name);
                    searchResult.UpdateStatusOnRunning(container);
                }
                catch
                {
                    return;
                }
            }
        }

        public CompositeModel GetUpdatedModel()
        {
            UpdateContainersStatus();
            return this;
        }

    }
}
