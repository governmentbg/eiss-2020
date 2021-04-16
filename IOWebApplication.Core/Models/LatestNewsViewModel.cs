using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Core.Models
{
    public class LatestNewsViewModel
    {
        public NewsViewModel News { get; set; }

        public int UnreadNews { get; set; }
        public bool IsUnreadNews { get; set; }
    }
}
