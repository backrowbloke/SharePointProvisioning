using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core;
using OfficeDevPnP.Core.Framework.Provisioning.Extensibility;
using OfficeDevPnP.Core.Framework.Provisioning.Connectors;
using OfficeDevPnP.Core.Framework.Provisioning.Model;
using OfficeDevPnP.Core.Framework.Provisioning.ObjectHandlers;
using OfficeDevPnP.Core.Diagnostics;
using OfficeDevPnP.Core.Framework.Provisioning.ObjectHandlers.TokenDefinitions;
using System.IO;
using System.Xml.Serialization;



namespace SPR.Provisioning
{
    [AttributeUsage(AttributeTargets.Property,
                Inherited = false,
                AllowMultiple = false)]
    internal sealed class OptionalAttribute : Attribute
    {
    }

    public class WorkflowHandler
    {
        public enum WorkflowType
        {
            list,
            globallyreusable,
            site,
            reusable,
            userdefinedaction
        }

        private const string serviceUrl = "/_vti_bin/NintexWorkflow/Workflow.asmx";

        public bool publishWorkflowFromXml(string webUrl, string workflowXML, string workflowName, string listName)
        {
            NintexWorkflowService.NintexWorkflowWS nintexWS = new NintexWorkflowService.NintexWorkflowWS();
            nintexWS.Url = webUrl + serviceUrl;
            nintexWS.UseDefaultCredentials = true;
            bool isPublished = nintexWS.PublishFromNWFXml(workflowXML, listName, workflowName, false);
            return isPublished;
        }

        public string getWorkflowXML(string webUrl, string workflowName, string listName, WorkflowType workflowType)
        {
            string workflowXML = string.Empty;
            NintexWorkflowService.NintexWorkflowWS nintexWS = new NintexWorkflowService.NintexWorkflowWS();
            nintexWS.Url = webUrl + serviceUrl;
            Console.WriteLine("Getting workflow from " + webUrl + serviceUrl);
            nintexWS.UseDefaultCredentials = true;
            workflowXML = nintexWS.ExportWorkflow(workflowName, listName, workflowType.ToString());
            return workflowXML;
        }


        public Stream getWorkflowXMStream(string webUrl, string workflowName, string listName, WorkflowType workflowType)
        {
            string workflowXML = string.Empty;
            NintexWorkflowService.NintexWorkflowWS nintexWS = new NintexWorkflowService.NintexWorkflowWS();
            nintexWS.Url = webUrl + serviceUrl;
            nintexWS.UseDefaultCredentials = true;
            workflowXML = nintexWS.ExportWorkflow(workflowName, listName, workflowType.ToString());
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(workflowXML);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public void ScheduleWorkflow(string webUrl, string workflowName, string startData, int maximumRepeats, bool workdaysOnly, string endOn, DateTime startTime, DateTime endTime, int countBetweenIntervals, string repeatType)
        {
            NintexWorkflowService.NintexWorkflowWS nintexWS = new NintexWorkflowService.NintexWorkflowWS();
            nintexWS.Url = webUrl + serviceUrl;
            nintexWS.UseDefaultCredentials = true;
            NintexWorkflowService.Schedule schedule = new NintexWorkflowService.Schedule();
            NintexWorkflowService.RepeatIntervalType nintexRepeatType = new NintexWorkflowService.RepeatIntervalType();
            NintexWorkflowService.EndScheduleOn nintexEndOn = new NintexWorkflowService.EndScheduleOn();
            NintexWorkflowService.RepeatInterval repeatInterval = new NintexWorkflowService.RepeatInterval();

            Enum.TryParse(repeatType, out nintexRepeatType);
            Enum.TryParse(endOn, out nintexEndOn);

            repeatInterval.CountBetweenIntervals = countBetweenIntervals;

            repeatInterval.Type = nintexRepeatType;
            schedule.EndOn = nintexEndOn;
            schedule.RepeatInterval = repeatInterval;
            schedule.StartTime = startTime;
            schedule.EndTime = endTime;
            schedule.WorkdaysOnly = workdaysOnly;
            schedule.MaximumRepeats = maximumRepeats;


            nintexWS.AddWorkflowSchedule(null, workflowName, startData, schedule, true);
        }
    }

    public enum WorkflowType
    {
        SharePoint,
        Nintex,
        Other
    }

    public class WorkflowInfo
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string InstantiationUrl { get; set; }
        public string AssociationData { get; set; }
        public string BaseID { get; set; }
        public string ListID { get; set; }
        public string ListName { get; set; }
        public string Scope { get; set; }
        public WorkflowType Type { get; set; }
        public string FilePath { get; set; }
    }

    public class ListWorkflows
    {
        public List<WorkflowInfo> Workflows { get; set; }
    }

    public class SiteWorkflows
    {
        public List<WorkflowInfo> Workflows { get; set; }
    }

    public class ContentTypeWorkflows
    {
        public List<WorkflowInfo> Workflows { get; set; }
    }

    public class WorkflowCollection
    {
        [Optional]
        public ListWorkflows ListWorkflows { get; set; }
        [Optional]
        public SiteWorkflows SiteWorkflows { get; set; }
        [Optional]
        public ContentTypeWorkflows ContentTypeWorkflows { get; set; }
        [Optional]
        public WorkflowScheduleCollection WorkflowSchedules { get; set; }
    }
    public class RepeatInterval
    {
        public string Type { get; set; }
        public int CountBetweenIntervals { get; set; }
    }

    public class WorkflowSchedule
    {
        public string WorkflowName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string EndOn { get; set; }
        public int MaximumRepeats { get; set; }
        public bool WorkDaysOnly { get; set; }
        public RepeatInterval RepeatInterval { get; set; }
        [Optional]
        public string StartData { get; set; }
    }


    public class WorkflowScheduleCollection
    {
        public List<WorkflowSchedule> WorkflowSchedules { get; set; }
    }

    public class PnpNintex : IProvisioningExtensibilityHandler
    {
        private const string workflowProviderName = "Nintex";
        private const string workflowFolderName = "NintexWorkflows";
        private const string providerNameSpace = "http://babcockinternational.com";

        //private void DownloadWorkflowFile(string workflowID, FileConnectorBase writer, Stream fileStream)
        //{
        //    writer.SaveFileStream(workflowID, workflowFolderName, fileStream);
        //}

        public ProvisioningTemplate Extract(ClientContext ctx, ProvisioningTemplate template,
   ProvisioningTemplateCreationInformation creationInformation,
   PnPMonitoredScope scope, string configurationData)
        {
            Helpers helpers = new Helpers();
            WorkflowHandler wfHandler = new WorkflowHandler();
            Console.WriteLine("Nintex");
            var currentTemplate = creationInformation.BaseTemplate;
            ExtensibilityHandler sprHandler = creationInformation.ExtensibilityHandlers[0];

            //StreamReader test = new StreamReader("d:\\babcockgraph.txt");
            //DownloadWorkflowFile("gpj.txt", creationInformation.FileConnector, test.BaseStream);

            Web web = ctx.Web;
            ctx.Load(web);
            ctx.ExecuteQuery();
            ListCollection listColl = web.Lists;
            ctx.Load(listColl);
            ctx.ExecuteQuery();
            string configText = "";

            WorkflowCollection wfColl = new WorkflowCollection();

            ListWorkflows listWF = new ListWorkflows();

            SiteWorkflows siteWF = new SiteWorkflows();

            ContentTypeWorkflows contentTypeWF = new ContentTypeWorkflows();

            List<WorkflowInfo> wfInfoList = new List<WorkflowInfo>();
            List<WorkflowInfo> wfInfoSite = new List<WorkflowInfo>();
            List<WorkflowInfo> wfInfoContentType = new List<WorkflowInfo>();

            foreach (List list in listColl)
            {
                ctx.Load(list);


                ctx.Load(list.WorkflowAssociations);

                ctx.ExecuteQuery();
                string listName = list.Title;

                foreach (var wfAss in list.WorkflowAssociations)
                {
                    if (wfAss.Name.IndexOf("Previous Version") > -1)
                    {
                        break;
                    }
                    WorkflowInfo wfInfo = new WorkflowInfo();

                    wfInfo.ID = wfAss.Id.ToString();
                    wfInfo.Name = wfAss.Name;
                    if (wfInfo.Name.IndexOf("Previous Version") > -1)
                    {
                        break;
                    }
                    wfInfo.InstantiationUrl = wfAss.InstantiationUrl;
                    wfInfo.BaseID = wfAss.BaseId.ToString();
                    wfInfo.Scope = WorkflowLevel.List.ToString();
                    wfInfo.ListID = wfAss.ListId.ToString();
                    wfInfo.ListName = listName;
                    wfInfo.AssociationData = wfAss.AssociationData;
                    if (wfAss.InstantiationUrl.IndexOf(workflowProviderName) > -1)
                    {
                        wfInfo.Type = WorkflowType.Nintex;
                    }
                    else
                    {
                        wfInfo.Type = WorkflowType.SharePoint;
                    }
                    wfInfo.FilePath = workflowFolderName + "\\" + wfAss.BaseId.ToString() + ".xml";
                    try
                    {
                        Stream wfStream = wfHandler.getWorkflowXMStream(ctx.Url, wfInfo.Name, listName, WorkflowHandler.WorkflowType.list);
                        helpers.DownloadWorkflowFile(wfAss.BaseId.ToString(), creationInformation.FileConnector, wfStream, WorkflowLevel.List);
                    }
                    catch
                    {
                        Console.WriteLine("ERROR!!! Could not export workflow Name " + wfAss.Name);
                    }
                    wfInfoList.Add(wfInfo);



                }


            }
            listWF.Workflows = wfInfoList;

            var siteWorkflows = web.WorkflowAssociations;
            ctx.Load(siteWorkflows);
            ctx.ExecuteQuery();
            foreach (var wfAss in siteWorkflows)
            {
                if (wfAss.Name.IndexOf("Previous Version") > -1)
                {
                    break;
                }
                WorkflowInfo wfInfo = new WorkflowInfo();

                wfInfo.ID = wfAss.Id.ToString();
                wfInfo.Name = wfAss.Name;
                wfInfo.InstantiationUrl = wfAss.InstantiationUrl;
                wfInfo.BaseID = wfAss.BaseId.ToString();
                wfInfo.Scope = WorkflowLevel.ContentType.ToString();
                wfInfo.AssociationData = wfAss.AssociationData;
                if (wfAss.InstantiationUrl.IndexOf(workflowProviderName) > -1)
                {
                    wfInfo.Type = WorkflowType.Nintex;
                }
                else
                {
                    wfInfo.Type = WorkflowType.SharePoint;
                }
                wfInfo.FilePath = workflowFolderName + "\\" + wfAss.BaseId.ToString() + ".xml";
                try
                {
                    Stream wfStream = wfHandler.getWorkflowXMStream(ctx.Url, wfInfo.Name, null, WorkflowHandler.WorkflowType.site);
                    helpers.DownloadWorkflowFile(wfAss.BaseId.ToString(), creationInformation.FileConnector, wfStream, WorkflowLevel.Site);
                }
                catch
                {
                    Console.WriteLine("ERROR!!! Could not export workflow Name " + wfAss.Name);
                }

                wfInfoSite.Add(wfInfo);
            }

            siteWF.Workflows = wfInfoSite;

            Microsoft.SharePoint.Client.ContentTypeCollection contentTypes = web.ContentTypes;
            ctx.Load(contentTypes);
            ctx.ExecuteQuery();
            foreach (Microsoft.SharePoint.Client.ContentType cType in contentTypes)
            {
                ctx.Load(cType.WorkflowAssociations);
                ctx.ExecuteQuery();
                foreach (var wfAss in cType.WorkflowAssociations)
                {
                    if (wfAss.Name.IndexOf("Previous Version") > -1)
                    {
                        break;
                    }

                    WorkflowInfo wfInfo = new WorkflowInfo();

                    wfInfo.ID = wfAss.Id.ToString();
                    wfInfo.Name = wfAss.Name;
                    wfInfo.InstantiationUrl = wfAss.InstantiationUrl;
                    wfInfo.BaseID = wfAss.BaseId.ToString();
                    wfInfo.Scope = WorkflowLevel.ContentType.ToString(); ;
                    wfInfo.AssociationData = wfAss.AssociationData;
                    if (wfAss.InstantiationUrl.IndexOf(workflowProviderName) > -1)
                    {
                        wfInfo.Type = WorkflowType.Nintex;
                    }
                    else
                    {
                        wfInfo.Type = WorkflowType.SharePoint;
                    }
                    wfInfo.FilePath = workflowFolderName + "\\" + wfAss.BaseId.ToString() + ".xml";
                    try
                    {
                        Stream wfStream = wfHandler.getWorkflowXMStream(ctx.Url, wfInfo.Name, null, WorkflowHandler.WorkflowType.globallyreusable);
                        helpers.DownloadWorkflowFile(wfAss.BaseId.ToString(), creationInformation.FileConnector, wfStream, WorkflowLevel.ContentType);
                    }
                    catch
                    {
                        Console.WriteLine("ERROR!!! Could not export workflow Name " + wfAss.Name);
                    }
                    wfInfoContentType.Add(wfInfo);

                }
            }


            contentTypeWF.Workflows = wfInfoContentType;

            wfColl.ListWorkflows = listWF;
            wfColl.SiteWorkflows = siteWF;
            wfColl.ContentTypeWorkflows = contentTypeWF;

            #region schedules
            //List<WorkflowSchedule> schedulesList = new List<WorkflowSchedule>();
            //WorkflowScheduleCollection schedules = new WorkflowScheduleCollection();
            //WorkflowSchedule schedule = new WorkflowSchedule();
            //schedule.WorkflowName = "SITE Workflow";
            //schedule.MaximumRepeats = 0;
            //schedule.WorkDaysOnly = false;
            //schedule.EndOn = "NoLimit";
            //schedule.EndTime = DateTime.Now.AddYears(1).ToUniversalTime();
            //schedule.StartTime = DateTime.Now.ToUniversalTime();
            //RepeatInterval interval = new RepeatInterval();
            //interval.CountBetweenIntervals = 7;
            //interval.Type = "Daily";
            //schedule.RepeatInterval = interval;
            //schedulesList.Add(schedule);
            //schedules.WorkflowSchedules = schedulesList;
            //wfColl.WorkflowSchedules = schedules;
            #endregion

            XmlSerializer sprHandlerXML = new XmlSerializer(typeof(WorkflowCollection), providerNameSpace);
            StringWriter xmlWriter = new StringWriter();
            sprHandlerXML.Serialize(xmlWriter, wfColl);
            configText = xmlWriter.ToString();

            sprHandler.Configuration = configText;

            ProvisioningTemplate newtemplate = template;
            newtemplate.ExtensibilityHandlers.Add(creationInformation.ExtensibilityHandlers[0]);
            return newtemplate;
        }

        public IEnumerable<TokenDefinition> GetTokens(ClientContext ctx,
         ProvisioningTemplate template, string configurationData)
        {
            return null;
        }

        public void Provision(ClientContext ctx, ProvisioningTemplate template,
         ProvisioningTemplateApplyingInformation applyingInformation,
         TokenParser tokenParser, PnPMonitoredScope scope, string configurationData)
        {
            if (!string.IsNullOrEmpty(configurationData))
            {
                Helpers helper = new Helpers();
                WorkflowHandler workflowHandler = new WorkflowHandler();
                var reader = new StringReader(configurationData);



                var serializer = new XmlSerializer(typeof(WorkflowCollection), providerNameSpace);
                var workflowColl = (WorkflowCollection)serializer.Deserialize(reader);
                ListWorkflows listWorkflows = workflowColl.ListWorkflows;
                Console.WriteLine("Processing List Workflows");
                //applyingInformation.ProgressDelegate = delegate (String message, Int32 progress, Int32 total)
                //{
                //    Console.Write("List Workflows", progress, total, "List Workflows");
                //};

                foreach (WorkflowInfo listWorkflow in listWorkflows.Workflows)
                {
                    string listName = listWorkflow.ListName;
                    string wfFile = listWorkflow.FilePath;
                    string workflowName = listWorkflow.Name;
                    string workflowXml = helper.ReadWorkflowFile(wfFile, template.Connector);
                    try
                    {
                        Console.WriteLine("Processing List Workflow '" + workflowName + "'");
                        workflowHandler.publishWorkflowFromXml(ctx.Url, workflowXml, workflowName, listName);
                    }
                    catch
                    {
                        Console.WriteLine("ERROR! Could not publish workflow " + workflowName);
                    }

                }

                ContentTypeWorkflows contentTypeWorkflows = workflowColl.ContentTypeWorkflows;
                Console.WriteLine("Processing Reusable Workflows");
                foreach (WorkflowInfo contentTypeWorkflow in contentTypeWorkflows.Workflows)
                {
                    string wfFile = contentTypeWorkflow.FilePath;
                    string workflowName = contentTypeWorkflow.Name;
                    string workflowXml = helper.ReadWorkflowFile(wfFile, template.Connector);
                    try
                    {
                        Console.WriteLine("Processing Reusable Workflow '" + workflowName + "'");
                        workflowHandler.publishWorkflowFromXml(ctx.Url, workflowXml, workflowName, null);
                    }
                    catch
                    {
                        Console.WriteLine("ERROR! Could not publish workflow " + workflowName);
                    }

                }

                SiteWorkflows siteWorkflows = workflowColl.SiteWorkflows;
                Console.WriteLine("Processing Site Workflows");
                foreach (WorkflowInfo siteWorkflow in siteWorkflows.Workflows)
                {
                    string wfFile = siteWorkflow.FilePath;
                    string workflowName = siteWorkflow.Name;
                    string workflowXml = helper.ReadWorkflowFile(wfFile, template.Connector);
                    try
                    {
                        Console.WriteLine("Processing Site Workflow '" + workflowName + "'");
                        workflowHandler.publishWorkflowFromXml(ctx.Url, workflowXml, workflowName, null);
                    }
                    catch
                    {
                        Console.WriteLine("ERROR! Could not publish workflow " + workflowName);
                    }

                }

                WorkflowScheduleCollection workflowSchedules = workflowColl.WorkflowSchedules;
                Console.WriteLine("Processing Workflow Schedules");
                foreach (WorkflowSchedule schedule in workflowSchedules.WorkflowSchedules)
                {
                    try
                    {
                        workflowHandler.ScheduleWorkflow(ctx.Url, schedule.WorkflowName, schedule.StartData, schedule.MaximumRepeats, schedule.WorkDaysOnly, schedule.EndOn, schedule.StartTime, schedule.EndTime, schedule.RepeatInterval.CountBetweenIntervals, schedule.RepeatInterval.Type);
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine("ERROR! Could not schedule workflow " + schedule.WorkflowName);
                        Console.WriteLine(err.InnerException);
                    }
                }
            }
        }
    }
}
