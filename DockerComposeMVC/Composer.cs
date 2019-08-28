using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Builders;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using DockerComposeMVC.Models;

namespace DockerComposeMVC
{
    public class Composer
    {
        //This function starts a commpose file given its path
        private static readonly string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "config/compose-destination.yml");
        private static ICompositeService svc = new Builder()
                        .UseContainer()
                        .FromComposeFile(FilePath)
                        .RemoveOrphans()
                        .KeepVolumes()
                        .ForceRecreate()
                        .KeepOnDispose()
                        .ServiceName("testing_compose")
                        .Build();

        public static string Run(string svc_name)
        {

            if (!System.IO.File.Exists(FilePath))
            {
                throw new FileNotFoundException("Compose file not found in the given location");
            }
            else if (svc != null && svc.State.ToString() is "Running")
            {
                throw new InvalidOperationException("SERVICE_ALREADY_RUNNING");
            }
            else
            {
                Task compose = new Task(() => svc.Start());
                compose.Start();
                Thread.Sleep(500);
            }

            return svc.State.ToString();
        }

        public static List<ContainerModel> GetDetailedStatus() {
            var containerEnum = svc.Containers.GetEnumerator();
            var containerList = new List<ContainerModel>();
            for (int count = 0; count < svc.Containers.Count; count++) {
                containerEnum.MoveNext();
                containerList.Add(new ContainerModel(containerEnum.Current));
            }
            return containerList;
        }

        public static string GetStatus() {
            return svc.State.ToString();
        }

        public static string Stop()
        {

            if (!System.IO.File.Exists(FilePath))
            {
                throw new FileNotFoundException("Compose file not found");
            }
            else if (svc == null)
            {
                throw new NullReferenceException("NO_SERVICE_DEFINED");
            }
            else if (svc.State.ToString() != "Running")
            {
                throw new InvalidOperationException("NO_SERVICE_RUNNING");
            }
            else
            {
                svc.Stop();
            }
            return svc.State.ToString();
        }
    }
}
