using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogParserService.Model
{
    class MasterPageView
    {
        public string Title { get; set; }
        public string LastVisit { get; set; }
        public string UserName { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}
