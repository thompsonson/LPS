﻿using System;
using log4net;
using log4net.Config;
using Topshelf;

namespace LogParserService
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            string log4netConfigPath;
            try
            {
                // Set up a simple configuration that logs on the console.
                // BasicConfigurator.Configure();
                log4netConfigPath = System.Configuration.ConfigurationManager.AppSettings["log4net"];
                //XmlConfigurator.Configure(new System.IO.FileInfo(args[0]));
                XmlConfigurator.Configure(new System.IO.FileInfo(log4netConfigPath));
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to set up log4net", ex);
            }
            log.Info("log4net config loaded - starting the service");
            try
            {
                // get things moving
                //App.Monitor.Start();

                HostFactory.Run(x =>
                {
                    x.UseLog4Net(log4netConfigPath);
                    x.Service<App.Service>(s =>
                    {
                        s.ConstructUsing(name => new App.Service());
                        s.WhenStarted(ap => App.Service.Start());
                        s.WhenStopped(ap => App.Service.Stop());
                    });
                    x.RunAsLocalSystem();

                    x.SetDescription("Monitors Event Viewers, IIS Log files as configured.");
                    x.SetDisplayName("LogParser.Monitor");
                    x.SetServiceName("LogParser.Monitor");
                });

            }
            catch (Exception ex)
            {
                log.Fatal("Failed to start the service");
                throw new Exception("Failed to start the service", ex);
            }
        }

    }
}
