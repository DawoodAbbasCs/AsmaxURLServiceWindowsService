using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using WindowsService1.BuildDocWSDLService;
using Newtonsoft.Json;
using System.Data.Entity;
using System.Runtime.Remoting.Contexts;
using System.Timers;
using System.IO;
using Serilog;
using Serilog.Core;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        private static ILogger logger;
        public Service1()
        {
            InitializeComponent();
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            string logFile = Path.Combine(logDirectory, $"log_{DateTime.Now.ToString("yyyyMMdd")}.txt");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFile)
                .CreateLogger();

            logger = Log.Logger;
        }

        protected override void OnStart(string[] args)
        {

        }

        protected override void OnStop()
        {
        }

        public async Task GetTemplates()
        {
            try
            {
                EmailSender.SendEmail("dawood.abbas507@gmail.com","Starting edoc service", $"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} : Hourly task started");
                logger.Information($"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} : Hourly task started");

                var objClient = new buildDocSoapClient();
                objClient.Open();
                var keys = ConfigurationManager.AppSettings.Keys;

                Instances objInstances = new Instances();
                List<Templates> objTemplates = new List<Templates>();

                foreach (var key in keys)
                {
                    int i = 0;
                    string email = key.ToString();
                    string password = ConfigurationManager.AppSettings[email];
                    var response1 = await objClient.getLibraryTemplatesAsync(email, password);

                    if (response1.getLibraryTemplatesResult != null)
                    {
                        var distinctCategories = response1.getLibraryTemplatesResult.Select(t => t.docCategory).Distinct();

                        var response2List = new List<libraryTemplate>();

                        foreach (var category in distinctCategories)
                        {
                            var response2 = await objClient.getLibraryTemplatesByCategoryAsync(email, password, category);

                            if (response2 != null)
                            {
                                response2List.AddRange(response2.getLibraryTemplatesByCategoryResult);
                            }
                        }

                        if (response2List != null)
                        {
                            foreach (var response2 in response2List)
                            {

                                Templates objTemplate = new Templates();
                                objTemplate.InstanceId = objInstances.InstanceId[i];
                                objTemplate.Template = response2;

                                objTemplates.Add(objTemplate);
                            }
                        }
                    }
                    else
                    {
                        logger.Information($"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} : Missing templates by Instance " + email);
                    }
                    i++;
                }

                await ValidateWithExistingTemplates(objTemplates);

                logger.Information($"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} : Hourly task executed successfully");
            }
            catch (Exception ex)
            {
                logger.Information($"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} : " + ex.ToString());
            }
        }

        public async Task ValidateWithExistingTemplates(List<Templates> objTemplates)
        {
            if (objTemplates is null)
            {
                logger.Information($"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} : ArgumentNullException : Templates shouldn't null or empty");
                return;
            }

            FulfillmentTestEntities objFulfillmentTestEntities = new FulfillmentTestEntities();

            List<EDocLibraryTemplate> destinationData = await objFulfillmentTestEntities.EDocLibraryTemplates.ToListAsync();
            List<Task> tasks = new List<Task>();

            foreach (var sourceObject in objTemplates)
            {
                var matchingDestinationObject = destinationData.FirstOrDefault(d => d.docID == sourceObject.Template.docID);
                if (matchingDestinationObject != null)
                {
                    try
                    {
                        if (matchingDestinationObject.isActive != false && !AreValuesMatched(matchingDestinationObject, sourceObject.Template))
                        {
                            tasks.Add(Task.Run(() => SetTemplateProperies(matchingDestinationObject, sourceObject)));
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Information($"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} : Something went wrong with bulk() UPDATE: {ex.Message}");
                    }
                }
                else
                {
                    if (sourceObject.Template.isActive != false)
                    {
                        try
                        {
                            EDocLibraryTemplate objSingleEDocLibraryTemplate = new EDocLibraryTemplate();
                            tasks.Add(Task.Run(() => SetTemplateProperies(objSingleEDocLibraryTemplate, sourceObject)));
                            objFulfillmentTestEntities.EDocLibraryTemplates.Add(objSingleEDocLibraryTemplate);
                        }
                        catch (Exception ex)
                        {
                            logger.Information($"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} : Something wrong with bulk() INSERT: {ex.Message}");
                        }
                    }
                }
            }

            await Task.WhenAll(tasks);
            int rowsAffected = await objFulfillmentTestEntities.SaveChangesAsync();

            logger.Information($"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} : SUCCESSFULLY MERGED {rowsAffected} TEMPLATES!");
        }

        //public void GetTemplates()
        //{
        //    try
        //    {
        //        var objClient = new buildDocSoapClient();
        //        objClient.Open();
        //        var keys = ConfigurationManager.AppSettings.Keys;

        //        Instances objInstances = new Instances();
        //        List<Templates> objTemplates = new List<Templates>();

        //        foreach (var key in keys)
        //        {
        //            int i = 0;
        //            string email = key.ToString();
        //            string password = ConfigurationManager.AppSettings[email];
        //            var response1 = objClient.getLibraryTemplates(email, password);

        //            if (response1 != null)
        //            {
        //                Templates objTemplate = new Templates();
        //                objTemplate.InstanceId = objInstances.InstanceId[i];

        //                foreach (var template in response1)
        //                {
        //                    var response2 = objClient.getLibraryTemplatesByCategory(email, password, template.docCategory);

        //                    if (response2 != null)
        //                    {

        //                        foreach (var template2 in response2)
        //                        {
        //                            objTemplate.Template = template2;
        //                        }
        //                    }
        //                }
        //                objTemplates.Add(objTemplate);
        //            }

        //            i++;
        //        }

        //        ValidateWithExistingTemplates(objTemplates);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }
        //}

        //public void ValidateWithExistingTemplates(List<Templates> objTemplates)
        //{
        //    if (objTemplates is null)
        //    {
        //        Console.WriteLine("ArgumentNullException : Templates shouldn't null or empty");
        //        return;
        //    }

        //    FulfillmentTestEntities objFulfillmentTestEntities = new FulfillmentTestEntities();

        //    List<EDocLibraryTemplate> destinationData = objFulfillmentTestEntities.EDocLibraryTemplates.ToList();
        //    List<EDocLibraryTemplate> objNewEDocLibraryTemplates = new List<EDocLibraryTemplate>();

        //    foreach (var sourceObject in objTemplates)
        //    {
        //        var matchingDestinationObject = destinationData.FirstOrDefault(d => d.docID == sourceObject.Template.docID);
        //        if (matchingDestinationObject != null)
        //        {
        //            if (matchingDestinationObject.isActive != false)
        //            {
        //                SetTemplateProperies(matchingDestinationObject,sourceObject);
        //            }
        //        }
        //        else
        //        {
        //            if (sourceObject.Template.isActive != false)
        //            {
        //                EDocLibraryTemplate objSingleEDocLibraryTemplate = new EDocLibraryTemplate();

        //                SetTemplateProperies(objSingleEDocLibraryTemplate, sourceObject);

        //                objNewEDocLibraryTemplates.Add(objSingleEDocLibraryTemplate);
        //            }
        //        }
        //    }

        //    objFulfillmentTestEntities.EDocLibraryTemplates.AddRange(objNewEDocLibraryTemplates);
        //    objFulfillmentTestEntities.SaveChanges();
        //}

        public void SetTemplateProperies(EDocLibraryTemplate destinationTemplate, Templates sourceTemplate)
        {
            destinationTemplate.instanceId = Convert.ToByte(sourceTemplate.InstanceId);
            destinationTemplate.docID = sourceTemplate.Template.docID;
            destinationTemplate.docPassword = sourceTemplate.Template.docPassword;
            destinationTemplate.docName = sourceTemplate.Template.docName;
            destinationTemplate.docDescription = sourceTemplate.Template.docDescription;
            destinationTemplate.docCategory = sourceTemplate.Template.docCategory;
            destinationTemplate.isActive = sourceTemplate.Template.isActive;
            destinationTemplate.docPageCount = Convert.ToInt16(sourceTemplate.Template.docPageCount);
            destinationTemplate.docWidth = sourceTemplate.Template.docWidth;
            destinationTemplate.docHeight = sourceTemplate.Template.docHeight;
            destinationTemplate.docBleed = sourceTemplate.Template.docBleed;
            destinationTemplate.clientType = sourceTemplate.Template.clientType;
            destinationTemplate.userID = Convert.ToInt16(sourceTemplate.Template.userID);
            destinationTemplate.fullScreenWindow = Convert.ToByte(FullscreenwindowEnum.GetStatusFromString(sourceTemplate.Template.fullScreenWindow.ToString()));
        }

        public bool AreValuesMatched(EDocLibraryTemplate destinationTemplate, libraryTemplate sourceTemplate)
        {
            if (destinationTemplate.docID == sourceTemplate.docID
                && destinationTemplate.docPassword == sourceTemplate.docPassword
                && destinationTemplate.docName == sourceTemplate.docName
                && destinationTemplate.docDescription == sourceTemplate.docDescription
                && destinationTemplate.docCategory == sourceTemplate.docCategory
                && destinationTemplate.isActive == sourceTemplate.isActive
                && destinationTemplate.docPageCount == Convert.ToInt16(sourceTemplate.docPageCount)
                && destinationTemplate.docWidth == sourceTemplate.docWidth
                && destinationTemplate.docHeight == sourceTemplate.docHeight
                && destinationTemplate.docBleed == sourceTemplate.docBleed
                && destinationTemplate.clientType == sourceTemplate.clientType
                && destinationTemplate.userID == Convert.ToInt16(sourceTemplate.userID)
                && destinationTemplate.fullScreenWindow == Convert.ToByte(FullscreenwindowEnum.GetStatusFromString(sourceTemplate.fullScreenWindow.ToString())))
                return true;

            return false;
        }


        public class Instances
        {
            public int[] instanceId;
            public int[] InstanceId
            {
                get
                {
                    return instanceId;
                }
            }
            public Instances()
            {
                var keys = ConfigurationManager.AppSettings.Keys;
                instanceId = new int[keys.Count];
                for (int i = 0; i <= keys.Count - 1; i++)
                {
                    instanceId[i] = i + 1;
                }
            }
        }

        public class Templates
        {
            public libraryTemplate Template;
            public int InstanceId;
        }
    }
}
