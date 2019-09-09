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
using Microsoft.AspNetCore.Routing;

namespace DockerComposeMVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult StartComposeReady()
        {

            //ComposerNew.InitializeLists();
            var composite = ComposerNew.GetSingleCompositeDetail("compose-destination.yml", false);
            return Ok(composite);
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
            //var filePath = Path.GetTempFileName();

            //foreach (var formFile in file)
            //{
            //    if (formFile.Length > 0)
            //    {
            //        using (var stream = new FileStream(filePath, FileMode.Create))
            //        {
            //            await formFile.CopyToAsync(stream);
            //        }
            //    }
            //}
            String filePath = await ComposerNew.FilePathAsync(file);
            StringValues filename;
            bool result = false;
            string contents = System.IO.File.ReadAllText(filePath);
            form.TryGetValue("destFileName", out filename);
            String[] parameters = ComposerNew.ExtractParameters(contents, out result);

            if (result == false)
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
        }


        public IActionResult ViewTemplateList()
        {
            return View(ComposerNew.TemplatesList);
        }

        public IActionResult ViewReadyList()
        {
            return View(ComposerNew.ReadyList);
        }

        //[Route("{cName}")]
        public IActionResult TemplateDetails([FromQuery] String cName)
        {
            ViewData["cFileName"] = cName;
            CompositeModel composeFileDetails = ComposerNew.GetSingleCompositeDetail(cName, true);
            String basePath = Path.Combine(Directory.GetCurrentDirectory(), "data/templates/" + cName);
            String contents = System.IO.File.ReadAllText(basePath);
            String[] parameters = ComposerNew.ExtractParameters(contents);
            
            ViewData["params"] = parameters;
            return View(composeFileDetails);
        }

        public IActionResult ReadyFileDetails([FromQuery] String cName)
        {
            ViewData["cFileName"] = cName;
            CompositeModel composeFileDetails = ComposerNew.GetSingleCompositeDetail(cName, false);
            //String basePath = Path.Combine(Directory.GetCurrentDirectory(), "data/templates/" + cName);
            //String contents = System.IO.File.ReadAllText(basePath);
            return View(composeFileDetails);
        }

        public IActionResult AddParamsInTemplate([FromQuery] String cName)
        {
            return Ok();
        }

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

        public IActionResult DebugListReady()
        {
            return Ok(ComposerNew.ReadyList);
        }

        public IActionResult DebugStart([FromQuery] string FileName)
        {
            try
            {
                string result = ComposerNew.StartService(FileName);
                return Ok(result);
            }
            catch (Exception e)
            {
                return NotFound("ERR_NO_SUCH_COMPOSE_FILE_FOUND: " + FileName);
            }

        }

        public IActionResult DebugStatus([FromQuery] string FileName)
        {
            try
            {
                string result = ComposerNew.GetServiceStatus(FileName);
                return Ok(result);
            }
            catch (Exception e)
            {
                return NotFound("ERR_NO_SUCH_RUNNING_COMPOSE_FILE_FOUND: " + FileName);
            }

        }

        public IActionResult DebugListRunning([FromQuery] string FileName)
        {
            return Ok(ComposerNew.GetRunningServices());
        }

        public IActionResult DebugStop([FromQuery] string FileName)
        {
            try
            {
                string result = ComposerNew.StopService(FileName);
                return Ok(result);
            }
            catch (Exception e)
            {
                return NotFound("ERR_NO_SUCH_RUNNING_COMPOSE_FILE_FOUND: " + FileName);
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
