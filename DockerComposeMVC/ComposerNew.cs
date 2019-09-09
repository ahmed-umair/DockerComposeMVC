using DockerComposeMVC.Models;
using Ductus.FluentDocker.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DockerComposeMVC
{
    public class ComposerNew
    {
        public static List<CompositeModel> TemplatesList = new List<CompositeModel>();
        public static List<CompositeModel> ReadyList = new List<CompositeModel>();
        public static readonly string BasePath = Path.Combine(Directory.GetCurrentDirectory(), "data");

        public static void InitializeLists()
        {
            TemplatesList = ComposeFileOperationsNew.LoadCompositesFromFiles(Path.Combine(Directory.GetCurrentDirectory(), @"data\templates"), true);
            ReadyList = ComposeFileOperationsNew.LoadCompositesFromFiles(Path.Combine(Directory.GetCurrentDirectory(), @"data\ready"), false);
        }
        public static string StartService(string ServiceName)
        {
            try
            {
                var searchResult = ReadyList.Single(service => service.Name == ServiceName);
                if (!(searchResult.Service.State is ServiceRunningState.Running && searchResult.Service.State is ServiceRunningState.Starting))
                {
                    try
                    {
                        Task compose = new Task(() => searchResult.Service.Start());
                        compose.Start();
                        Thread.Sleep(500);
                        return searchResult.Service.State.ToString();
                    }
                    catch (Exception e)
                    {
                        //Log error and stack trace here
                        return "ERR_FAILED_TO_START";
                    }
                }
                //Log error here
                return "ERR_ALREADY_STARTED";
            }
            catch (Exception e)
            {
                return "ERR_COMPOSE_FILE_NOT_FOUND";
            }
        }
        public static string GetServiceStatus(string ServiceName)
        {
            try
            {
                var searchResult = ReadyList.Single(service => service.Name == ServiceName);
                if (searchResult.IsTemplate)
                {
                    return "ERR_TEMPLATE_NOT_EXECUTABLE";
                }
                else
                {
                    return searchResult.Service.State.ToString();
                }
            }
            catch
            {
                return "ERR_COMPOSE_FILE_NOT_FOUND";
            }
        }

        public static List<CompositeModel> GetRunningServices()
        {
            try
            {
                var searchResult = ReadyList.FindAll(service => (service.Service.State == ServiceRunningState.Running || service.Service.State == ServiceRunningState.Starting));
                foreach(var composite in searchResult)
                {
                    composite.UpdateContainersStatus();
                }
                return searchResult;
            }
            catch
            {
                return new List<CompositeModel>();
            }
        }

        public static string StopService(string ServiceName)
        {
            try
            {
                var searchResult = ReadyList.Single(service => service.Name == ServiceName);
                searchResult.Service.Stop();
                //ResetContainerStatus(searchResult.Service);
            }
            catch
            {
                return "ERR_COMPOSE_FILE_NOT_FOUND";
            }
            return "stopped";
        }

        ///CATCH NOT FOUND EXCEPTION WHEREVER THIS IS CALLED
        public static CompositeModel GetSingleCompositeDetail(string ServiceName, bool IsTemplate)
        {
            CompositeModel searchResult;
            if (!IsTemplate)
            {
                searchResult = ReadyList.Single(service => service.Name == ServiceName);
            }
            else
            {
                searchResult = TemplatesList.Single(service => service.Name == ServiceName);
            }

            return searchResult;
        }

        public static string ReplaceParams(string content, Dictionary<string, string> paramsList)
        {
            var keys = paramsList.Keys;

            foreach (string key in keys)
            {
                content = content.Replace("${{" + key + "}}", paramsList.GetValueOrDefault(key));
            }

            return content;
        }

        public static async Task<string> FilePathAsync (IEnumerable<IFormFile>  file)
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
            return filePath;
        }

        public static String[] ExtractParameters(String contents, out bool result)
        {
            result = false;
            String output = String.Join(";", Regex.Matches(contents, @"\${{(.+?)}}")
                                                .Cast<Match>()
                                                .Select(m => m.Groups[1].Value));
            if (!string.IsNullOrEmpty(output))
            {
                result = true;
            }

            String[] parameters = output.Split(';');
            return (parameters);
        }

        public static String[] ExtractParameters(String contents)
        {
            String output = String.Join(";", Regex.Matches(contents, @"\${{(.+?)}}")
                                                .Cast<Match>()
                                                .Select(m => m.Groups[1].Value));
            
            String[] parameters = output.Split(';');
            return (parameters);
        }
    }
}
