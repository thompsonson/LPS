using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LogParserService.Model
{
    [PetaPoco.TableName("Eventvwr")]
    [PetaPoco.PrimaryKey("EventvwrID")]
    [PetaPoco.ExplicitColumns]
    class Eventvwr
    {
        [PetaPoco.Column]
        public int EventvwrID { get; set; }
        [PetaPoco.Column]
        public int JobID { get; set; }
        [PetaPoco.Column]
        public string EventLog { get; set; }
        [PetaPoco.Column]
        public int RecordNumber { get; set; }
        [PetaPoco.Column]
        public string TimeGenerated { get; set; }
        [PetaPoco.Column]
        public string TimeWritten { get; set; }
        [PetaPoco.Column]
        public int EventID { get; set; }
        [PetaPoco.Column]
        public int EventType { get; set; }
        [PetaPoco.Column]
        public string EventTypeName { get; set; }
        [PetaPoco.Column]
        public int EventCategory { get; set; }
        [PetaPoco.Column]
        public string EventCategoryName { get; set; }
        [PetaPoco.Column]
        public string SourceName { get; set; }
        [PetaPoco.Column]
        public string Strings { get; set; }
        [PetaPoco.Column]
        public string ComputerName { get; set; }
        [PetaPoco.Column]
        public string SID { get; set; }
        [PetaPoco.Column]
        public string Message { get; set; }
        [PetaPoco.Column]
        public string Data  { get; set; }

        public Eventvwr() { }
        // not so POCO?
        public Eventvwr(DataRow row, int Job) {
            JobID = Job;
            EventLog = row["EventLog"].ToString();
            RecordNumber = (int)row["RecordNumber"];
            TimeGenerated = row["TimeGenerated"].ToString();
            TimeWritten = row["TimeWritten"].ToString();
            EventID = (int)row["EventID"];
            EventType = (int)row["EventType"];
            EventTypeName = row["EventTypeName"].ToString();
            EventCategory = (int) row["EventCategory"];
            EventCategoryName = row["EventCategoryName"].ToString();
            SourceName = row["SourceName"].ToString();
            Strings = row["Strings"].ToString();
            ComputerName = row["ComputerName"].ToString();
            SID = row["SID"].ToString();
            Message = row["Message"].ToString();
            Data = row["Data"].ToString();
            
        }
    }
}
