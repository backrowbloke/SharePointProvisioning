using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core.Framework.Provisioning.Extensibility;
using OfficeDevPnP.Core.Framework.Provisioning.Model;
using OfficeDevPnP.Core.Framework.Provisioning.ObjectHandlers;
using OfficeDevPnP.Core.Framework.Provisioning.Connectors;
using System.IO;

namespace SPR.Provisioning
{
    class Helpers
    {
        private const string workflowFolderName = "NintexWorkflows";

        public void DownloadWorkflowFile(string workflowID, FileConnectorBase writer, Stream fileStream, WorkflowLevel workflowLevel)
        {
            writer.SaveFileStream(workflowID + ".xml", workflowFolderName, fileStream);
        }
        public string ReadWorkflowFile(string fileName, FileConnectorBase fileReader)
        {

            //FileSystemConnector connector = new FileSystemConnector(null,workflowFolderName);

            //string fileContents = connector.GetFile(fileName);
            string fileContents = fileReader.GetFile(fileName);
            return fileContents;
        }
    }
}
