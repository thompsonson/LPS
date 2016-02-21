using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogParserService.Model
{
    [PetaPoco.TableName("Job")]
    [PetaPoco.PrimaryKey("JobID")]
    [PetaPoco.ExplicitColumns]
    class Job
    {
        [PetaPoco.Column]
        public int JobID { get; set; }
        [PetaPoco.Column]
        public int MonitorID { get; set; }
        [PetaPoco.Column]
        public string StartTime { get; set; }
        [PetaPoco.Column]
        public string EndTime { get; set; }
        [PetaPoco.Column]
        public string FinalState { get; set; }
        [PetaPoco.Column]
        public string Info { get; set; }
        [PetaPoco.Column]
        public int Batch { get; set; }
    }

    class JobView
    {
        public int JobID { get; set; }
        public int MonitorID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string FinalState { get; set; }
        public int Batch { get; set; }
        public string Info { get; set; }
        //public List<T> returnedData { get; set; } 
    }
}
