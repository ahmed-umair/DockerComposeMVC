using DockerComposeMVC.Models;
using Ductus.FluentDocker.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DockerComposeMVC
{
    public class ComposerNew
    {
        public static List<CompositeModel> TemplatesList = new List<CompositeModel>();
        public static List<CompositeModel> ReadyList = new List<CompositeModel>();
        public static readonly string BasePath = Path.Combine(Directory.GetCurrentDirectory(), "config");

        public static void InitializeLists()
        {
            TemplatesList = ComposeFileOperationsNew.LoadCompositesFromFiles(Path.Combine(Directory.GetCurrentDirectory(), @"data\templates"), false);
            ReadyList = ComposeFileOperationsNew.LoadCompositesFromFiles(Path.Combine(Directory.GetCurrentDirectory(), @"data\ready"), true);
        }
        public static string StartFromTemplate(string ServiceName, Dictionary<string, string> dict) { return "started"; }
        public static string StartFromReady(string ServiceName)
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
                if (!searchResult.IsTemplate)
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
    }
}
