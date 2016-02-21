using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LogParserService.Model
{
    [PetaPoco.TableName("IIS")]
    [PetaPoco.PrimaryKey("IISID")]
    [PetaPoco.ExplicitColumns]
    class IIS
    {
        [PetaPoco.Column]
        public int IISID { get; set; }
        [PetaPoco.Column]
        public int JobID { get; set; }
        [PetaPoco.Column]
        public string LogFilename { get; set; }
        [PetaPoco.Column]
        public int RowNumber { get; set; }
        [PetaPoco.Column]
        public string date { get; set; }
        [PetaPoco.Column]
        public string time { get; set; }
        [PetaPoco.Column]
        public string s_ip  { get; set; }
        [PetaPoco.Column]
        public string cs_method { get; set; }
        [PetaPoco.Column]
        public string cs_uri_stem { get; set; }
        [PetaPoco.Column]
        public string cs_uri_query { get; set; }
        [PetaPoco.Column]
        public int s_port { get; set; }
        [PetaPoco.Column]
        public string cs_username { get; set; }
        [PetaPoco.Column]
        public string c_ip { get; set; }
        [PetaPoco.Column]
        public string cs_User_Agent { get; set; }
        [PetaPoco.Column]
        public int sc_status { get; set; }
        [PetaPoco.Column]
        public int sc_substatus { get; set; }
        [PetaPoco.Column]
        public int sc_win32_status { get; set; }
        [PetaPoco.Column]
        public int time_taken { get; set; }

        /*
LogFilename
RowNumber 
date       
time     
s_ip         
cs_method 
cs_uri_stem      
cs_uri_query 
s_port 
cs_username 
c_ip        
cs_User_Agent
sc_status 
sc_substatus 
sc_win32_status 
time_taken
        */
        public IIS() { }
        // not so POCO?
        public IIS(DataRow row, int Job)
        {
            JobID = Job;
            LogFilename = row["LogFilename"].ToString();
            RowNumber = (int)row["LogRow"];
            date = row["date"].ToString();
            time = row["time"].ToString();
            s_ip = row["s-ip"].ToString();
            cs_method = row["cs-method"].ToString();
            cs_uri_stem = row["cs-uri-stem"].ToString();
            cs_uri_query = row["cs-uri-query"].ToString();
            s_port = (int)row["s-port"];
            cs_username = row["cs-username"].ToString();
            c_ip = row["c-ip"].ToString();
            cs_User_Agent = row["cs(User-Agent)"].ToString();
            sc_status = (int)row["sc-status"];
            sc_substatus = (int)row["sc-substatus"];
            sc_win32_status = (int)row["sc-win32-status"];
            time_taken = (int)row["time-taken"];
        }
    }
}
