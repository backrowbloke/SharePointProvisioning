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

    public enum WorkflowType
    {
        SharePoint,
        Nintex,
        Other
    }

    public enum WorkflowLevel
    {
        Site,
        ContentType,
        List
    }
    

    public class PnpNintex : IProvisioningExtensibilityHandler
    {
        private const string workflowProviderName = "Nintex";
        


        


        public ProvisioningTemplate Extract(ClientContext ctx, ProvisioningTemplate template,
   ProvisioningTemplateCreationInformation creationInformation,
   PnPMonitoredScope scope, string configurationData)
        {
            var currentTemplate = creationInformation.BaseTemplate;
            ExtensibilityHandler sprHandler = creationInformation.ExtensibilityHandlers[0];

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
                    //Helpers.DownloadWorkflowFile(wfInfo.FilePath, creationInformation.FileConnector);
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
                wfInfo.FilePath = wfAss.BaseId.ToString() + ".xml";
                //Helpers.DownloadWorkflowFile(wfInfo.FilePath, creationInformation.FileConnector);

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
                    wfInfo.FilePath = wfAss.BaseId.ToString() + ".xml";
                    //Helpers.DownloadWorkflowFile(wfInfo.FilePath, creationInformation.FileConnector);
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
