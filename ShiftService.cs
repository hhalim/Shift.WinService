using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Shift.WinService
{
    partial class ShiftService : ServiceBase
    {
        private static JobServer jobServer;

        public ShiftService()
        {
            InitializeComponent();
            var appServiceName = ConfigurationManager.AppSettings["ServiceName"];

            if (jobServer == null)
            {
                var config = new Shift.ServerConfig();
                config.MaxRunnableJobs = Convert.ToInt32(ConfigurationManager.AppSettings["MaxRunableJobs"]);
                //config.ProcessID = ConfigurationManager.AppSettings["ShiftPID"];
                config.DBConnectionString = ConfigurationManager.ConnectionStrings["ShiftDBConnection"].ConnectionString;
                config.DBAuthKey = ConfigurationManager.AppSettings["DocumentDBAuthKey"];
                config.Workers = Convert.ToInt32(ConfigurationManager.AppSettings["ShiftWorkers"]);

                config.StorageMode = ConfigurationManager.AppSettings["StorageMode"];
                var progressDBInterval = ConfigurationManager.AppSettings["ProgressDBInterval"];
                if (!string.IsNullOrWhiteSpace(progressDBInterval))
                    config.ProgressDBInterval = TimeSpan.Parse(progressDBInterval); //Interval when progress is updated in main DB

                var autoDeletePeriod = ConfigurationManager.AppSettings["AutoDeletePeriod"];
                config.AutoDeletePeriod = string.IsNullOrWhiteSpace(autoDeletePeriod) ? null : (int?)Convert.ToInt32(autoDeletePeriod);

                config.AssemblyFolder = ConfigurationManager.AppSettings["AssemblyFolder"];
                //config.AssemblyListPath = ConfigurationManager.AppSettings["AssemblyListPath"];

                config.ForceStopServer = Convert.ToBoolean(ConfigurationManager.AppSettings["ForceStopServer"]); //Set to true to allow windows service to shut down after a set delay in StopServerDelay
                config.StopServerDelay = Convert.ToInt32(ConfigurationManager.AppSettings["StopServerDelay"]);

                config.ServerTimerInterval = Convert.ToInt32(ConfigurationManager.AppSettings["TimerInterval"]); //optional: default every 5 sec for getting jobs ready to run and run them
                config.ServerTimerInterval2 = Convert.ToInt32(ConfigurationManager.AppSettings["CleanUpTimerInterval"]); //optional: default every 10 sec for server CleanUp()

                //config.AutoDeleteStatus = new List<JobStatus?> { JobStatus.Completed, JobStatus.Error }; //Auto delete only the jobs that had Stopped or with Error
                //config.EncryptionKey = ConfigurationManager.AppSettings["ShiftEncryptionParametersKey"];
                config.PollingOnce = Convert.ToBoolean(ConfigurationManager.AppSettings["PollingOnce"]);

                jobServer = new Shift.JobServer(config);
            }

            this.ServiceName = appServiceName;
        }

        protected async override void OnStart(string[] args)
        {
            if (Array.Find<string>(args, s=> s == "-debug") == "-debug")
            {
                await StartWithNoTimer();
            }
            else
            {
                await StartWithTimer();
            }
        }

        protected override void OnStop()
        {
            jobServer.StopServerAsync().GetAwaiter().GetResult(); //Run synchronously or it will exit before marking running jobs with 'STOP' command!!!
        }

        //This is for debugging OnStart and OnStop as Console App
        internal void TestStartAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }

        protected async Task StartWithTimer()
        {
            try
            {
                await jobServer.RunServerAsync();
            }
            catch (Exception ex)
            {
                this.EventLog.WriteEntry(ex.Message + Environment.NewLine + ex.StackTrace, EventLogEntryType.Error);

                // Log exception
                this.ExitCode = 1064;
                this.Stop();
                throw;
            }
        }

        //FOR DEBUGGING ONLY
        protected async Task StartWithNoTimer()
        {
            try
            {
                await jobServer.StopJobsAsync();
                await jobServer.RunJobsAsync();

                await jobServer.CleanUpAsync();
            }
            catch (Exception ex)
            {
                this.EventLog.WriteEntry(ex.Message + Environment.NewLine + ex.StackTrace, EventLogEntryType.Error);

                // Log exception
                this.ExitCode = 1064;
                this.Stop();
                throw;
            }
        }

    }
}
