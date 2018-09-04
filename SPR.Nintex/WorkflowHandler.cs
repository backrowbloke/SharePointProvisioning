using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SPR.Nintex
{
    public enum WorkflowType
    {
        list,
        globallyresuable,
        site,
        reusable,
        userdefinedaction
    }
    public class WorkflowHandler
    {
        private const string serviceUrl = "/_vti_bin/NintexWorkflow/Workflows.asmx";
        public string workflowXML(string webUrl, string workflowName, string listName, WorkflowType workflowType)
        {
            string workflowXML = string.Empty;
            NintexWorkflows.NintexWorkflowWS nintexWS = new NintexWorkflows.NintexWorkflowWS();
            nintexWS.Url = webUrl + serviceUrl;
            nintexWS.UseDefaultCredentials = true;
            nintexWS.ExportWorkflow(workflowName, listName, workflowType.ToString());
            return workflowXML;
        }
    }
}
