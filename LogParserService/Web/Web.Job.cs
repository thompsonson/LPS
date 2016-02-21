using System.Collections.Generic;
using log4net;
using Nancy;
using Nancy.Security;

namespace LogParserService.Web
{
    public class Job : BaseModule
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Job));
        public Job()
            : base("/Job")
        {
            this.RequiresAuthentication();

            // db reference
            var db = new PetaPoco.Database("LocalSQLite");
            // datatable functions
            Get["/dt"] = parameters =>
            {
                var jobs = db.Query<Model.JobView>("select j.JobID, j.MonitorID, m.Name, m.Type, j.StartTime, j.EndTime, j.FinalState, j.Info from Job J, Monitor m WHERE j.MonitorID = m.MonitorID;");

                return Response.AsJson(new
                {
                    aaData = jobs
                });
            };
            Get["/dt/{id}"] = parameters =>
            {
                Model.JobView j = db.Single<Model.JobView>("select j.JobID, j.MonitorID, m.Name, m.Type, j.StartTime, j.EndTime, j.FinalState, j.Batch, j.Info from Job J, Monitor m WHERE j.MonitorID = m.MonitorID AND JobID = @JobID", new { JobID = parameters.id });

                return Response.AsJson(new
                {
                    aaData = j
                });
            };
            Get["/dt/{type}/{id}"] = parameters =>
            {
                string MonitorType = parameters.type.ToString();

                switch (MonitorType.ToUpper())
                {
                    case "EVENTVWR":
                        //TODO: limited to 50 to avoid generating too much data - needs to be paged
                        IEnumerable<Model.Eventvwr> Eventvwrdata = db.Query<Model.Eventvwr>("select * from Eventvwr WHERE JobID = @JobID", new { JobID = parameters.id });
                        return Response.AsJson(new
                        {
                            aaData = Eventvwrdata
                        });

                    case "IIS":
                        //TODO: limited to 50 to avoid generating too much data - needs to be paged
                        IEnumerable<Model.IIS> IISdata = db.Query<Model.IIS>("select * from IIS WHERE JobID = @JobID limit 50", new { JobID = parameters.id });
                        return Response.AsJson(new
                        {
                            aaData = IISdata
                        });

                    default:
                        log.WarnFormat("Type ({0}) not supported in Nancy Module for Job", MonitorType);
                        break;
                }

                var jobs = db.Query<Model.JobView>("select j.JobID, j.MonitorID, m.Name, m.Type, j.StartTime, j.EndTime, j.FinalState, j.Info from Job J, Monitor m WHERE j.MonitorID = m.MonitorID AND j.JobId=@id;", new { id = parameters.id });

                return Response.AsJson(new
                {
                    aaData = jobs
                });
            };

            Get["/vanilla"] = parameters =>
            {
                var jobs = db.Query<Model.JobView>("select j.JobID, j.MonitorID, m.Name, m.Type, j.StartTime, j.EndTime, j.FinalState, j.Info from Job J, Monitor m WHERE j.MonitorID = m.MonitorID;");

                return View["Web\\Views\\Job-vanilla.html", new { jobs = jobs }];
            };

            Get["/"] = parameters =>
            {

                return View["Web\\Views\\Job.html", baseModel];
            };

            Get["/{id}"] = parameters =>
            {
                
                /*
                 * IEnumerable<Model.JobView> jobs = db.Query<Model.JobView>("select j.JobID, j.MonitorID, m.Name, m.Type, j.StartTime, j.EndTime, j.FinalState, j.Info from Job J, Monitor m WHERE j.MonitorID = m.MonitorID AND j.JobId=@id;",new { id = parameters.id });
                Model.JobView job = jobs.FirstOrDefault();
                 */
                Model.JobView jobView = db.Single<Model.JobView>("select j.JobID, j.MonitorID, m.Name, m.Type, j.StartTime, j.EndTime, j.FinalState, j.Batch, j.Info from Job J, Monitor m WHERE j.MonitorID = m.MonitorID AND j.JobId=@id;", new { id = parameters.id });
                if (jobView.Batch == 1)
                {
                    baseModel.jobs = jobView;
                    return View["Web\\Views\\JobBatch.html", baseModel];
                }
                else
                {
                    // switch on the type of monitor
                    switch (jobView.Type.ToUpper())
                    {
                        case "EVENTVWR":
                            IEnumerable<Model.Eventvwr> Eventvwrdata = db.Query<Model.Eventvwr>("select * from Eventvwr WHERE JobID = @JobID", jobView);
                            baseModel.jobs = jobView;
                            baseModel.Results = Eventvwrdata;
                            return View["Web\\Views\\JobEventvwr.html", baseModel ]; // new { jobs = job, Results = Eventvwrdata }];
                        case "IIS":
                            IEnumerable<Model.IIS> IISdata = db.Query<Model.IIS>("select * from IIS WHERE JobID = @JobID", jobView);
                            baseModel.jobs = jobView;
                            baseModel.Results = IISdata;
                            return View["Web\\Views\\JobIIS.html", baseModel]; // new { jobs = job, Results = IISdata }];

                        default:
                            log.WarnFormat("Type ({0}) not supported in Nancy Module for Job", jobView.Type);
                            break;
                    }
                }
                return "ah, didn't expect you to be here!";
            };

        }
    }
}
