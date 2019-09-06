using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DockerComposeMVC.Models;
using System.Reflection;
using System.Threading;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Text.RegularExpressions;

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

        public IActionResult Upload()
        {
            return View();
        }

        //[HttpPost]
        [HttpPost]
        public async Task<IActionResult> UploadFiles(IEnumerable<IFormFile> file, IFormCollection form)
        {
            //full path to file in temp location
            var filePath = Path.GetTempFileName();
            
            foreach (var formFile in file)
            {
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            StringValues filename;
            string contents = System.IO.File.ReadAllText(filePath);
            form.TryGetValue("destFileName", out filename);

            String output = String.Join(";", Regex.Matches(contents, @"\${{(.+?)}}")
                                                .Cast<Match>()
                                                .Select(m => m.Groups[1].Value));
            String[] parameters = output.Split(';');
          
            if (string.IsNullOrEmpty(output))
            {
                return View("AddParameters");
            }

            try
            {
                System.IO.File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "temp/" + filename + ".yaml"), contents);  
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            ViewData["fileString"] = contents;
            ViewData["fileName"] = filename;
            return View("AddParameters", parameters);
            //return Ok(new { size, filePath })
        }

        //public IActionResult AddParameters()
        //{
        //    var paramsArray = ViewBag.prams;
        //    return View(paramsArray);
        //}

        public IActionResult StatusDebug()
        {
            return Ok(Composer.GetStatus() + " " + Composer.GetStatus().Length);
        }

        public IActionResult GetConfig()
        {
            Configuration.ReadConfigFile();
            var config = Configuration.GetConfig();

            var list = ComposeFileOperationsNew.LoadCompositesFromFiles(Path.Combine(Directory.GetCurrentDirectory(), @"data\ready"), true);
            return Ok(list);
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

        [HttpPost]
        public IActionResult CreateCustom([FromForm] Dictionary<string, string> dict, String fileString)
        {
            if (Composer.GetStatus() == "Running")
            {
                ViewData["Title"] = "Failure";
                ViewData["success"] = false;
                ViewData["message"] = "A multi-container application is already running. Please stop it before attempting to start another one!";
                return View();
            }
           
            string finalComposeString = ComposeFileOperations.ReplaceParams(fileString, dict);
            if (ComposeFileOperations.WriteToFile(finalComposeString))
            {
                Composer.Run("Compose_Application");
                Thread.Sleep(1000);

                ViewData["Title"] = "Success";
                ViewData["success"] = true;
                ViewData["message"] = "Your application has been started. You will be redirected to the status page in a few seconds.";
                ViewData["status"] = Composer.GetStatus();
            }
            
            return View("SubmitNew");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
