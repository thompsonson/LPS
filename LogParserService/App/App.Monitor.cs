using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using log4net;
using Nancy.Hosting.Self;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;

namespace LogParserService.App
{
    class Monitor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Monitor));
        
        private static List<MonitorClass> _monitorRunPool = new List<MonitorClass>();

        public static void HandleActiveChangeEvent(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Not used... 
            // TODO: check the value of the Active flag. Start\Stop\Do nothing with the monitor as appropriate
            //log.Info("Event fired");
        }

        public static void Run(Model.Monitor monitor)
        {

            var db = new PetaPoco.Database("LocalSQLite");

            // check the monitor should run
            if (monitor.State == Model.MonitorState.Running.ToString() && monitor.RunMultiple == 0)
            {
                log.InfoFormat("Monitor [{0}] is in a running state - not running a multiple version", monitor.MonitorID);
                return;
            }

            monitor.State = Model.MonitorState.Running.ToString();
            db.Update(monitor);

            log.InfoFormat("Monitor (ID: {2}) running on Thread Id {0} with the sql {1}", Thread.CurrentThread.ManagedThreadId.ToString(), monitor.SQL, monitor.MonitorID);

            // Sleeping to test the web interface update
            //Thread.Sleep(10000);

            // start the job
            var job = new Model.Job();
            job.MonitorID = monitor.MonitorID;
            job.StartTime = DateTime.Now.ToString();
            db.Insert(job);

            // set the local directory
            string monitorFolder = System.AppDomain.CurrentDomain.BaseDirectory + "\\" + monitor.MonitorID;
            try
            {
                if (File.Exists(monitorFolder))
                {
                    log.InfoFormat("Folder ({0}) exists, setting current location to that", monitorFolder);
                    Directory.SetCurrentDirectory(monitorFolder);
                }
                else
                {
                    log.InfoFormat("Folder ({0}) doesn't exist, creating the folder", monitorFolder);
                    DirectoryInfo di = Directory.CreateDirectory(monitorFolder);
                    log.InfoFormat("setting current location to that {0}", monitorFolder);
                    Directory.SetCurrentDirectory(monitorFolder);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat(@"Unable to create and\or set directory {0}", monitorFolder);
                log.Error(ex.ToString());
            }
            

            int RowCount = 0;
            try
            {
                DataTable dt;
                // switch on the type of monitor
                switch (monitor.Type.ToUpper())
                {
                    case "EVENTVWR":
                        //case Model.MonitorType.Eventvwr:
                        if (monitor.isBathStatement())
                        {
                            if (monitor.intoFileName() == null)
                                throw new Exception("filename (as parsed from the SQL) is null - shouldn't be");

                            string fileName = @".\" + monitor.intoFileName();
                            string newFileName = @".\" + job.JobID + "." + monitor.intoFileName();
                            try
                            {
                                if (String.IsNullOrEmpty(monitor.Checkpoint))
                                    LogParser.BatchEventLog(monitor.SQL);
                                else
                                    LogParser.BatchEventLog(monitor.SQL, monitor.Checkpoint);
                                //rename the output to include the jobId

                                if (File.Exists(fileName) )
                                {
                                    File.Move(fileName, newFileName);
                                    RowCount = -1;
                                } 
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Error running batch execute against Event Log", ex);
                            }
                            job.Batch = 1;
                            job.Info = newFileName;
                            job.FinalState = "Complete";
                        }
                        else
                        {
                            try
                            {
                                if (String.IsNullOrEmpty(monitor.Checkpoint))
                                    dt = LogParser.ParseEventLog(monitor.SQL);
                                else
                                    dt = LogParser.ParseEventLog(monitor.SQL, monitor.Checkpoint);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Error parsing Event Log", ex);
                            }
                            RowCount = dt.Rows.Count;
                            // convert the DataTable into a list of Model.Eventvwr (with the JobId) and insert into the DB
                            try
                            {
                                foreach (DataRow row in dt.Rows) { db.Insert(new Model.Eventvwr(row, job.JobID)); }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Error saving Event Viewer data", ex);
                            }
                            job.Info = "Job returned " + RowCount.ToString() + " rows";
                            job.FinalState = "Complete";
                        }
                        break;
                    case "IIS":
                        if (monitor.isBathStatement())
                        {
                            if (monitor.intoFileName() == null)
                                throw new Exception("filename (as parsed from the SQL) is null - shouldn't be");

                            string fileName = @".\" + monitor.intoFileName();
                            string newFileName = @".\" + job.JobID + "." + monitor.intoFileName();
                            try
                            {
                                if (String.IsNullOrEmpty(monitor.Checkpoint))
                                    LogParser.BatchIISLog(monitor.SQL);
                                else
                                    LogParser.BatchIISLog(monitor.SQL, monitor.Checkpoint);
                                //rename the output to include the jobId

                                if (File.Exists(fileName) )
                                {
                                    File.Move(fileName, newFileName);
                                    RowCount = -1;
                                } 
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Error running batch execute against Event Log", ex);
                            }
                            job.Batch = 1;
                            job.Info = newFileName;
                            job.FinalState = "Complete";
                        }
                        else
                        {
                            try
                            {
                                if (String.IsNullOrEmpty(monitor.Checkpoint))
                                    dt = LogParser.ParseIISLog(monitor.SQL);
                                else
                                    dt = LogParser.ParseIISLog(monitor.SQL, monitor.Checkpoint);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Error parsing IIS Log", ex);
                            }
                            RowCount = dt.Rows.Count;
                            // convert the DataTable into a list of Model.Eventvwr (with the JobId) and insert into the DB
                            try
                            {
                                foreach (DataRow row in dt.Rows) { db.Insert(new Model.IIS(row, job.JobID)); }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Error saving IIS Log data", ex);
                            }
                            job.Info = "Job returned " + RowCount.ToString() + " rows";
                            job.FinalState = "Complete";
                        }
                        break;
                    default:
                        job.Info = "Monitor type [" + monitor.Type + "] not implemented";
                        job.FinalState = "Error";
                        db.Update(job);
                        throw new Exception(job.Info);
                }
            }
            catch (Exception ex)
            {
                job.Info = ex.ToString();
                job.FinalState = "Error";
            }
            finally
            {
                if (monitor.Alert == 1)
                {
                    if (job.FinalState == "Error")
                    {
                        log.Error("Sending error email");
                        log.ErrorFormat("{0}", job.Info);
                        // send email saying job errored
                        Helpers.Email.Send("thompsonson@gmail.com", monitor.EmailAddresses, "Error on Monitor [" + monitor.Name + "] ", job.Info);
                        monitor.State = Model.MonitorState.Alert.ToString();
                    }
                    else if (RowCount > 0)
                    {
                        log.Info("Sending alert email");
                        monitor.State = Model.MonitorState.Alert.ToString();
                        // send email saying rowcount is greater than 0, include link to data
                        Helpers.Email.Send("thompsonson@gmail.com", monitor.EmailAddresses, "Alert on Monitor [" + monitor.Name + "] ", "The monitor returned " + RowCount.ToString() + " rows. </br> More information here: <a href='http://localhost:8014/Job/" + job.JobID + "'>Job Information</a>", true);
                    }
                    else if (RowCount < 0)
                    {
                        //@".\" + job.JobID + "." + monitor.intoFileName()
                        log.Info("Sending alert email");
                        monitor.State = Model.MonitorState.Alert.ToString();
                        // send email saying rowcount is greater than 0, include link to data
                        Helpers.Email.AttachAndSend("thompsonson@gmail.com", monitor.EmailAddresses, "Alert on Monitor [" + monitor.Name + "] ", "The monitor returned the attached file. </br> More information here: <a href='http://localhost:8014/Job/" + job.JobID + "'>Job Information</a>", true, MailPriority.High, @".\" + job.JobID + "." + monitor.intoFileName());
                    }
                }
                else
                {
                    monitor.State = Model.MonitorState.Idle.ToString();
                }
            }

            // catch monitor state still in running
            if (monitor.State == Model.MonitorState.Running.ToString())
                monitor.State = Model.MonitorState.Idle.ToString();

            job.EndTime = DateTime.Now.ToString();
            db.Update(job);
            db.Update(monitor);

        }

        public static void Run(object input)
        {
            var db = new PetaPoco.Database("LocalSQLite");
            
            // cast the input object
            //Model.Monitor monitor = input as Model.Monitor;

            int monitorID = (int)input ;

            Model.Monitor monitor = db.Single<Model.Monitor>("Select * from Monitor WHERE MonitorID = @monitorID", new { monitorID = monitorID });

            Run(monitor);

        }

        public Monitor(int monitorID, int interval)
        {
            _monitorRunPool.Add(new MonitorClass(monitorID, interval));
        }

        public Monitor(Model.Monitor m)
        {
            _monitorRunPool.Add(new MonitorClass(m.MonitorID, m.RunInterval));
        }

        public static bool IsActive(Model.Monitor m){
            var mc = _monitorRunPool.Where(x => x.MonitorID == m.MonitorID);
            if (mc.Count() == 1)
            {
                return true;
            }
            else if (mc.Count() > 1)
            {
                // log an error as something has gone very wrong (should, at most be 1 here).
                // TODO: should they be stopped and one started? will it every get here?
                throw new Exception("Multiple versions of monitor [id: {0}] are running");
            }
            else
            {
                return false;
            }
        }

        public static bool ChangeInterval(int monitorID, int interval)
        {
            // get the monitor (if it's running)
            var mc = _monitorRunPool.Where(x => x.MonitorID == monitorID);
            if (mc.Count() == 1)
            {
                return mc.First().ChangeInterval(interval);
            } else if (mc.Count() > 1){
                // TODO: log an error as something has gone very wrong (should, at most be 1 here).
                throw new Exception("Multiple versions of this monitor [ID: " + monitorID + "] in the monitorRunPool");
            }
            else
            {
                throw new Exception("No versions of this monitor [ID: " + monitorID + "] in the monitorRunPool");
            }
        }

        public static bool Disable(int monitorID)
        {
            // get the monitor (if it's running)
            var mc = _monitorRunPool.Where(x => x.MonitorID == monitorID);
            if (mc.Count() == 1)
            {
                bool ret =  mc.First().DisableTimer();
                // remove from _monitorRunPool
                if (ret)
                    _monitorRunPool.Remove(mc.First());
                // TODO: GC? 
                return ret;
            }
            else if (mc.Count() > 1)
            {
                // TODO: log an error as something has gone very wrong (should, at most be 1 here).
                return false;
            }
            else
            {
                return false;
            }
        }

        public static void setState(Model.Monitor m)
        {
            if (m.Active == 1)
            {
                // verify it's not already running
                if (IsActive(m))
                    throw new Exception("monitor [ID: "+ m.MonitorID+"] already running");
                
                new Monitor(m);
            }
            else
            {
                // verify the monitor isn't running
                if (!IsActive(m))
                    throw new Exception("monitor [ID: " + m.MonitorID + "] isn't running");

                Disable(m.MonitorID);
            }
        }

        public static void StopAll()
        {
            var db = new PetaPoco.Database("LocalSQLite");

            // stop all running threads\monitors
            foreach (MonitorClass mc in _monitorRunPool)
            {
                // stop the Timer
                if (!mc.DisableTimer())
                    log.WarnFormat("Unable to disable the timer for Monitor ID: {0}", mc.MonitorID);
                // TODO: Does the thread need joining back??

                // change the monitor state
                try
                {
                    Model.Monitor monitor = db.Single<Model.Monitor>("Select * from Monitor WHERE MonitorID = @monitorID", new { monitorID = mc.MonitorID });
                    monitor.State = Model.MonitorState.Idle.ToString();
                    db.Update(monitor);
                }
                catch (Exception ex)
                {
                    log.WarnFormat("Unable to set the monitor state to Idle for monitor ID: {0}", mc.MonitorID);
                    log.ErrorFormat(ex.ToString());
                }
            }
        }

    }

    class MonitorClass
    {
        private int _monitorID {get; set;}
        private Timer _t { get; set;}

        public int MonitorID { get { return _monitorID; } }

        public MonitorClass(int monitorID, int Interval)
        {
            _monitorID = monitorID;
            _t = new Timer(new TimerCallback(App.Monitor.Run), monitorID, TimeSpan.Zero, TimeSpan.FromSeconds(Interval));
        }

        public bool ChangeInterval(int IntervalSeconds){
            try
            {
                //_t.Change(Timeout.Infinite, IntervalSeconds * 1000);
                _t.Change(IntervalSeconds * 1000, IntervalSeconds * 1000);
                return true;
            }
            catch (Exception ex)
            {
                // log an exception
                throw new Exception("Unable to change the Interval on monitor [Timer: " + _t.ToString() + "]");
            }
        }

        public bool DisableTimer()
        {
            try
            {
                _t.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
