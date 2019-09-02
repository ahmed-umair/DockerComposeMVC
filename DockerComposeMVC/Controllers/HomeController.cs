using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DockerComposeMVC.Models;
using System.Reflection;
using System.Threading;

namespace DockerComposeMVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Status()
        {

            if (Composer.GetStatus() == "Running")
            {
                ViewData["Status"] = true;
                ViewData["Title"] = "Running";
                ViewData["Message"] = "Your multi-container application is running";
            }
            else
            {
                ViewData["Status"] = false;
                ViewData["Title"] = "Stopped";
                ViewData["Message"] = "You do not have a multi-container application running";
            }
            var containers = Composer.GetDetailedStatus();

            return View(containers);
        }

        public IActionResult StatusDebug()
        {
            return Ok(Composer.GetStatus() + " " + Composer.GetStatus().Length);
        }

        public IActionResult GetConfig() {
            Configuration.ReadConfigFile();
            return Ok(Configuration.GetConfig());
        }

        public IActionResult Stop()
        {
            ViewData["success"] = false;
            ViewData["message"] = "Either there was no application running or the running application was not stopped successfully";
            if (Composer.GetStatus() == "Running" && Composer.Stop() == "Stopped")
            {
                ViewData["success"] = true;
                ViewData["message"] = "The application was stopped successfully";
            }
            return View();
        }

        public IActionResult StartNew()
        {
            ViewData["Message"] = "Please fill out the configuration details below and click Start!";

            return View(Params.ReadParamsList());
        }

        public IActionResult UploadCompose()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitNew([FromForm] Dictionary<string, string> dict)
        {
            if (Composer.GetStatus() == "Running")
            {
                ViewData["Title"] = "Failure";
                ViewData["success"] = false;
                ViewData["message"] = "A multi-container application is already running. Please stop it before attempting to start another one!";
                return View();
            }


            string finalComposeString = ComposeFileOperations.ReplaceParams(ComposeFileOperations.ReadFile(), dict);
            if (ComposeFileOperations.WriteToFile(finalComposeString))
            {
                Composer.Run("Compose_Application");
                Thread.Sleep(1000);

                ViewData["Title"] = "Success";
                ViewData["success"] = true;
                ViewData["message"] = "Your application has been started. You will be redirected to the status page in a few seconds.";
                ViewData["status"] = Composer.GetStatus();
            }


            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
