using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPR.Provisioning
{
    [AttributeUsage(AttributeTargets.Property,
                Inherited = false,
                AllowMultiple = false)]
    internal sealed class OptionalAttribute : Attribute
    {
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
}
