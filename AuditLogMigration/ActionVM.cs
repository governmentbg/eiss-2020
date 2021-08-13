using System;
using System.Collections.Generic;
using System.Text;

namespace AuditLogMigration
{
    public class ActionVM
    {
        public string UserName { get; set; }
        public string HttpMethod { get; set; }
        public string RequestUrl { get; set; }

        public ActionRequestVM ResponseBody { get; set; }
    }

    public class ActionRequestVM
    {
        public string Type { get; set; }
    }
}
