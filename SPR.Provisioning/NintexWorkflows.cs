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
    }

    public class PnpNintex : IProvisioningExtensibilityHandler
    {
        private const string workflowProviderName = "Nintex";
        private const string workflowFolderName = "NintexWorkflows";

        private void DownloadWorkflowFile(string workflowID, FileConnectorBase writer, Stream fileStream)
        {
            writer.SaveFileStream(workflowID, workflowFolderName, fileStream);
        }

        public ProvisioningTemplate Extract(ClientContext ctx, ProvisioningTemplate template,
   ProvisioningTemplateCreationInformation creationInformation,
   PnPMonitoredScope scope, string configurationData)
        {
            var currentTemplate = creationInformation.BaseTemplate;
            ExtensibilityHandler sprHandler = creationInformation.ExtensibilityHandlers[0];

            StreamReader test = new StreamReader("d:\\babcockgraph.txt");
            DownloadWorkflowFile("gpj.txt", creationInformation.FileConnector, test.BaseStream);

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
                ctx.Load(list.WorkflowAssociations);
                ctx.ExecuteQuery();

                foreach (var wfAss in list.WorkflowAssociations)
                {
                    WorkflowInfo wfInfo = new WorkflowInfo();

                    wfInfo.ID = wfAss.Id.ToString();
                    wfInfo.Name = wfAss.Name;
                    wfInfo.InstantiationUrl = wfAss.InstantiationUrl;
                    wfInfo.BaseID = wfAss.BaseId.ToString();
                    wfInfo.Scope = "List";
                    wfInfo.ListID = wfAss.ListId.ToString();
                    wfInfo.AssociationData = wfAss.AssociationData;
                    if (wfAss.InstantiationUrl.IndexOf(workflowProviderName) > -1)
                    {
                        wfInfo.Type = WorkflowType.Nintex;
                    }
                    else
                    {
                        wfInfo.Type = WorkflowType.SharePoint;
                    }
                    wfInfo.FilePath = wfAss.BaseId.ToString() + ".xml";
                    //DownloadWorkflowFile(wfInfo.FilePath, creationInformation.FileConnector);
                    wfInfoList.Add(wfInfo);



                }


            }
            listWF.Workflows = wfInfoList;

            var siteWorkflows = web.WorkflowAssociations;
            ctx.Load(siteWorkflows);
            ctx.ExecuteQuery();
            foreach (var wfAss in siteWorkflows)
            {
                WorkflowInfo wfInfo = new WorkflowInfo();

                wfInfo.ID = wfAss.Id.ToString();
                wfInfo.Name = wfAss.Name;
                wfInfo.InstantiationUrl = wfAss.InstantiationUrl;
                wfInfo.BaseID = wfAss.BaseId.ToString();
                wfInfo.Scope = "List";
                wfInfo.ListID = wfAss.ListId.ToString();
                wfInfo.AssociationData = wfAss.AssociationData;
                if (wfAss.InstantiationUrl.IndexOf(workflowProviderName) > -1)
                {
                    wfInfo.Type = WorkflowType.Nintex;
                }
                else
                {
                    wfInfo.Type = WorkflowType.SharePoint;
                }
                wfInfo.FilePath = System.IO.Directory.GetCurrentDirectory() + "\\" + wfAss.BaseId.ToString() + ".xml";

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
                    WorkflowInfo wfInfo = new WorkflowInfo();

                    wfInfo.ID = wfAss.Id.ToString();
                    wfInfo.Name = wfAss.Name;
                    wfInfo.InstantiationUrl = wfAss.InstantiationUrl;
                    wfInfo.BaseID = wfAss.BaseId.ToString();
                    wfInfo.Scope = "List";
                    wfInfo.ListID = wfAss.ListId.ToString();
                    wfInfo.AssociationData = wfAss.AssociationData;
                    if (wfAss.InstantiationUrl.IndexOf(workflowProviderName) > -1)
                    {
                        wfInfo.Type = WorkflowType.Nintex;
                    }
                    else
                    {
                        wfInfo.Type = WorkflowType.SharePoint;
                    }
                    wfInfo.FilePath = System.IO.Directory.GetCurrentDirectory() + "\\" + wfAss.BaseId.ToString() + ".xml";
                    wfInfoContentType.Add(wfInfo);

                }
            }


            contentTypeWF.Workflows = wfInfoContentType;

            wfColl.ListWorkflows = listWF;
            wfColl.SiteWorkflows = siteWF;
            wfColl.ContentTypeWorkflows = contentTypeWF;

            XmlSerializer sprHandlerXML = new XmlSerializer(typeof(WorkflowCollection));
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
            var reader = new StringReader(configurationData);
            var serializer = new XmlSerializer(typeof(WorkflowInfo));
        }
    }
}
