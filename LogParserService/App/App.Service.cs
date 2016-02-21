using System;
using log4net;
using Nancy.Hosting.Self;

namespace LogParserService.App
{

    class Service
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Service));

        private static NancyHost host;

        public static void Start()
        {
            // start Nancy interface
            // initialize an instance of NancyHost (found in the Nancy.Hosting.Self package)
            // TODO: put this in config
            string url = "http://localhost:8014";
            host = new NancyHost(new Uri(url), new Web.Bootstrapper());
            host.Start(); // start hosting

            // start up monitors that are active
            var db = new PetaPoco.Database("LocalSQLite");

            foreach (var a in db.Query<Model.Monitor>("SELECT * FROM Monitor where Active = 1;"))
            {
                log.InfoFormat("Starting monitor name is {0}, running sql ({1}) every {2} seconds", a.Name, a.SQL, a.RunInterval);

                new App.Monitor(a.MonitorID, a.RunInterval);
            }
        }

        public static void Stop()
        {
            // stop Nancy
            host.Stop();

            // stop the running threads
            App.Monitor.StopAll();
        }

    }
}
