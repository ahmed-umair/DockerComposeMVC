using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DockerComposeMVC
{

    public class Program
    {
        public static readonly string compose_source_path = Path.GetFullPath("compose-source/docker-compose.yml");
        public static readonly string compose_destination_path = Path.GetFullPath("compose-destination/docker-compose.yml");
        public const string paramsListFilePath = "config/paramsList.json";
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
