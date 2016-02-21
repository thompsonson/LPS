using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using System.Text.RegularExpressions;

namespace LogParserService.Model
{
    [PetaPoco.TableName("Monitor")]
    [PetaPoco.PrimaryKey("MonitorID")]
    [PetaPoco.ExplicitColumns]
    class Monitor
    {
        //^Select\W+.*\W+INTO\W+(.*)\W+FROM\W+.*
        //^Select .* INTO (.*) FROM .*
        private static Regex _r = new Regex(@"^Select\W+.*\W+INTO\W+(.*)\W+FROM\W+.*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        [PetaPoco.Column]
        public int MonitorID { get; set; }
        [PetaPoco.Column]
        public string Name { get; set; }
        [PetaPoco.Column]
        public string Description { get; set; }
        [PetaPoco.Column]
        public int RunInterval { get; set; }
        [PetaPoco.Column]
        public string SQL { get; set; }
        [PetaPoco.Column]
        public string Type { get; set; }
        [PetaPoco.Column]
        public string State { get; set; }
        [PetaPoco.Column]
        public int RunMultiple { get; set; }
        [PetaPoco.Column]
        public int Active { get; set; }
        [PetaPoco.Column]
        public string Checkpoint { get; set; }
        [PetaPoco.Column]
        public int Alert { get; set; }
        [PetaPoco.Column]
        public string EmailAddresses { get; set; }
        [PetaPoco.Column]
        public string created_user { get; set; }
        // these shouldn't be setable externally (or internally really) - set via defaults and triggers
        [PetaPoco.Column]
        public DateTime created_ts { get; private set; }
        [PetaPoco.Column]
        public DateTime update_ts { get; private set; }
        [PetaPoco.Column]
        public string update_user { get; set; }

        public bool isBathStatement()
        {
            if (_r.IsMatch(SQL))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string intoFileName()
        {
            Match match = _r.Match(SQL);

            if (match.Success)
                return match.Groups[1].Value;
            else
                return null;
        }
 
        // filtering on update is inclusion only so keeping a list of columns to update from web here.
        public string[] ColumnsToUpdateFromWeb {
            get {
                return new string[] { "MonitorID", "Name", "Description", "RunInterval", "SQL", "Type", "RunMultiple", "Active", "CheckPoint", "Alert", "EmailAddresses" };
            }
        }


    }

    enum MonitorType
    {
        EVENTVWR,
        IIS,
        W3C
    }

    enum MonitorState
    {
        Idle,
        Running,
        Alert,
        Complete
    }

}
