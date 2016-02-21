using System;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace LogParserService.Web
{
    public class Monitor : BaseModule
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Monitor));
        public Monitor()
            : base("/Monitor")
        {
            this.RequiresAuthentication();
            this.RequiresAnyClaim(new[] { "editMonitor", "admin", "viewMonitor" });

            // db reference
            var db = new PetaPoco.Database("LocalSQLite");
            // datatable functions
            Get["/dt"] = pameters =>
            {
                var monitors = db.Query<Model.Monitor>("SELECT MonitorID,Name,Description,RunInterval,SQL,Type,State,RunMultiple,Active,Checkpoint,Alert,EmailAddresses FROM Monitor");

                return Response.AsJson(new
                {
                    aaData = monitors
                });
            };

            Get["/vanilla"] = parameters =>
            {
                var monitors = db.Query<Model.Monitor>("SELECT * FROM Monitor");

                return View["Web\\Views\\Monitor.vanilla.html", monitors];
            };

            Get["/run/{id}"] = parameters =>
            {
                return "You is in the wrong place";
            };

            Get["/"] = parameters =>
            {
                // check the claims and set the Action menu option
                // TODO: can this be added to the Context? bool canEditMonitor = this.RequiresOneCliamFrom(new [] { "claim1", "claim2" });
                // https://github.com/NancyFx/Nancy/issues/485
                string[] canEditClaims = new[] { "editMonitor", "admin"};
                IEnumerable<string> claims = this.Context.CurrentUser.Claims;
                if (claims.Any(claim => canEditClaims.Contains(claim)))
                    return View["Web\\Views\\MonitorCanEdit.html", baseModel];
                else
                    return View["Web\\Views\\Monitor.html", baseModel];
            };
        }

        
    }

    public class RunMonitor : BaseModule
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WriteMonitor));
        public RunMonitor()
            : base("/run/monitor")
        {
            this.RequiresAuthentication();
            // TODO: need to change to have run claims
            this.RequiresAnyClaim(new[] { "editMonitor", "admin" });

            var db = new PetaPoco.Database("LocalSQLite");

            Get["/{id}"] = parameters =>
            {
                try
                {

                    int monitorID = (int)parameters.id;

                    Model.Monitor monitor = db.Single<Model.Monitor>("Select * from Monitor WHERE MonitorID = @monitorID", new { monitorID = monitorID });

                    // check the monitor should run
                    if (monitor.State == Model.MonitorState.Running.ToString() && monitor.RunMultiple == 0)
                    {
                        log.InfoFormat("Monitor [{0}] is in a running state - not running a multiple version (initiated from web request)", monitor.MonitorID);
                        return Response.AsJson(new
                        {
                            success = false,
                            message = "Monitor is already running and is configured not to run multiple versions."
                        }); 
                    }

                    // should check id is an int and the state of the monitor
                    //Thread thread = new Thread(App.Monitor.Run(monitor));
                    // maybe change to a task factory or thread pool
                    //http://stackoverflow.com/questions/8014037/c-sharp-call-a-method-in-a-new-thread
                    Thread thread = new Thread(() => App.Monitor.Run(monitor));
                    thread.Start();
                    return Response.AsJson(new
                    {
                        success = true,
                        message = "Running the monitor"
                    }); ;
                }
                catch (Exception ex)
                {
                    // Log error on the server - maybe should have ID in the URL so it can be part of the exception
                    log.WarnFormat("Error whilst trying to run monitor. Exception: {0}", ex.ToString());
                    // TODO: alert? 
                    return Response.AsJson(new
                    {
                        success = false,
                        message = ex.ToString()
                    });
                }
            };

        }
    }


    public class WriteMonitor : BaseModule
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WriteMonitor));
        public WriteMonitor()
            : base("/write/monitor")
        {
            this.RequiresAuthentication();
            this.RequiresAnyClaim(new[] { "editMonitor", "admin" });

            // db reference
            var db = new PetaPoco.Database("LocalSQLite");
            // datatable functions

            Post["/dt/edit"] = pameters =>
            {
                // try\catch return  success\fail
                try
                {
                    Model.Monitor m = new Model.Monitor();
                    // get the previous verison of the monitor and then set up the monitor event handlers 
                    // not doing it like this anymore
                    //m.PropertyChanged += new PropertyChangedEventHandler(App.Monitor.HandleActiveChangeEvent);
                    m = this.Bind();

                    log.InfoFormat("Updating monitor (ID: {0})", m.MonitorID.ToString());

                    // change the monitor state in the app layer/
                    if (Convert.ToBoolean(m.Active) != App.Monitor.IsActive(m))
                    {
                        App.Monitor.setState(m);
                    }

                    // TODO: get a previous version of the monitor and check if the interval has changed.
                    // Change the interval
                    if (m.Active == 1)
                        App.Monitor.ChangeInterval(m.MonitorID, m.RunInterval);

                    // persist the monitor, without the State column
                    db.Update(m, m.ColumnsToUpdateFromWeb);

                    return Response.AsJson(new
                    {
                        success = true,
                        aaData = m
                    });
                }
                catch (Exception ex)
                {
                    // Log error on the server - maybe should have ID in the URL so it can be part of the exception
                    log.WarnFormat("Error whilst editing monitor. Exception: {0}", ex.ToString());
                    // TODO: alert? 
                    return Response.AsJson(new
                    {
                        success = false,
                        message = ex.ToString()
                    });
                }

            };

            Post["/dt/new"] = pameters =>
            {
                // try\catch return  success\fail
                Model.Monitor m = new Model.Monitor();

                log.InfoFormat("Adding new monitor (ID: {0})", m.MonitorID.ToString());

                try
                {
                    db.Insert(m);
                    // TODO: check if the monitor needs to start
                    return Response.AsJson(new
                    {
                        success = true,
                        monitor = m
                    });
                }
                catch (Exception ex)
                {
                    return Response.AsJson(new
                    {
                        success = false,
                        message = ex.ToString()
                    });

                }
            };

            Post["/dt/delete"] = parameter =>
            {
                try
                {
                    Model.Monitor m = new Model.Monitor();
                    m = this.Bind();

                    log.InfoFormat("Deleting monitor (ID: {0})", m.MonitorID.ToString());

                    db.Delete(m);

                    return Response.AsJson(new
                    {
                        success = true
                    });

                }
                catch (Exception ex)
                {
                    // Log error on the server - maybe should have ID in the URL so it can be part of the exception
                    log.WarnFormat("Error whilst deleting monitor. Exception: {0}", ex.ToString());
                    // TODO: alert? 
                    return Response.AsJson(new
                    {
                        success = false,
                        message = ex.ToString()
                    });
                }

            };

        }
    }



}
