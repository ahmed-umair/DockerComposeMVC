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
        public static readonly string ComposeTemplateDir = Path.GetFullPath("data/templates");
        public static readonly string ComposeReadyDir = Path.GetFullPath("data/ready");
        public static readonly string ComposeTemporaryDir = Path.GetFullPath("temp");
        
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
