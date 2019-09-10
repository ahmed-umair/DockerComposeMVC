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
            StringValues filename, cFileName;
            bool result = false;
            string contents = System.IO.File.ReadAllText(filePath);
            form.TryGetValue("destFileName", out filename);
            form.TryGetValue("file", out cFileName);
            String[] parameters = ComposerNew.ExtractParameters(contents, out result);

            if (result == false)
            {
                return View("AddParameters");
            }

            try
            {
                System.IO.File.WriteAllText(Path.Combine(Program.ComposeTemporaryDir, filename + ".yml"), contents);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            ViewData["fileString"] = contents;
            ViewData["FileName"] = filename;
            //ViewData["templateName"] = cFileName;
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
            String basePath = Path.Combine(Program.ComposeTemplateDir, cName);
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

        public IActionResult RunningReadyFiles()
        {
            var RunningList = ComposerNew.GetRunningServices();
            return View(RunningList);
        }

        public IActionResult StopRunningFile([FromQuery] String FileName)
        {
            try
            {
                string result = ComposerNew.StopService(FileName);
                var RunningList = ComposerNew.GetRunningServices();
                return View("RunningReadyFiles", RunningList);
            }
            catch (Exception e)
            {
                return NotFound("ERR_NO_SUCH_RUNNING_COMPOSE_FILE_FOUND: " + FileName);
            }

        }

        public IActionResult StatusDebug()
        {
            return Ok(Composer.GetStatus() + " " + Composer.GetStatus().Length);
        }

        public IActionResult UploadCompose()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateCustom([FromForm] Dictionary<string, string> dict, [FromForm] String templateName, [FromForm] String instanceName)
        {

            String tempPath = Path.Combine(Directory.GetCurrentDirectory(), @"data\templates\" + templateName);
            String fileString = System.IO.File.ReadAllText(tempPath);

            string finalComposeString = ComposerNew.ReplaceParams(fileString, dict);
            var filename = ComposeFileOperationsNew.WriteFileToReadyFolder(finalComposeString, templateName, instanceName);
            ComposerNew.StartService(filename);


            return View("RunComposeFile");
        }

        public IActionResult VerifyUploadedTemplate([FromForm] Dictionary<string, string> dict, [FromForm] String templateName)
        {
            templateName = templateName + ".yml";

            String tempPath = Path.Combine(Program.ComposeTemporaryDir, templateName);
            String fileString = System.IO.File.ReadAllText(tempPath);
            String writeStatus = "";
            string finalComposeString = ComposerNew.ReplaceParams(fileString, dict);
            var filename = ComposeFileOperationsNew.WriteFileToReadyFolder(finalComposeString, templateName, "test");

            bool verificationResult = false;
            if (filename != "ERR_UNABLE_TO_WRITE_TO_FILE")
            {
                verificationResult = ComposerNew.VerifyContainer(filename);
            }

            if (!verificationResult)
            {
                ComposeFileOperationsNew.RemoveFileFromReadyFolder(filename);
                ComposeFileOperationsNew.RemoveFromReadyList(filename);
                return View("ErrorPage");
            }
            else
            {
                ComposeFileOperationsNew.AddToTemplatesFromFile(Path.Combine(Program.ComposeTemporaryDir, templateName), templateName, out writeStatus);
                return View("SuccessPage");
            }

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

        public IActionResult RunComposeFile([FromQuery] string cName)
        {
            try
            {
                string result = ComposerNew.StartService(cName);
                ViewData["status"] = result;
                return View();
            }
            catch (Exception e)
            {
                return NotFound("ERR_NO_SUCH_COMPOSE_FILE_FOUND: " + cName);
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

        public IActionResult RemoveReadyFile([FromQuery] string FileName)
        {
            if (ComposeFileOperationsNew.RemoveFromReadyList(FileName))
            {
                return View("ViewReadyList", ComposerNew.ReadyList);
            }
            else
            {
                return View("GeneralError");
            }
        }

        public IActionResult RemoveTemplateFile([FromQuery] string FileName)
        {
            Debug.WriteLine("------------------");
            Debug.WriteLine(FileName);
            if (ComposeFileOperationsNew.RemoveComposeTemplateFromList(FileName))
            {
                return View("ViewTemplateList", ComposerNew.TemplatesList);
            }
            else
            {
                return View("GeneralError");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
