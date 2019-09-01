using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PcfJobHostApp.Engine
{
    /// <summary>
    /// Job Engine
    /// </summary>
    public class JobEngine : IJobEngine
    {
        #region "Fields"
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;

        // This is here incase you want to inject any of your configuration into the map
        private readonly Microsoft.Extensions.Configuration.IConfigurationRoot _config;

        private readonly IScheduler scheduler;
        readonly ISchedulerFactory factory;

        #endregion

        #region "CTOR"

        /// <summary>
        /// Public CTOR, DI does not work correctly, so we pull what we need from Program's statics
        /// </summary>
        public JobEngine() {
            _loggerFactory = Program.Services.GetService<ILoggerFactory>();
            _config = Program.Services.GetService<IConfigurationRoot>();

            // TODO: Set-up persistant storage like SQL or REDIS
            NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };

            factory = new StdSchedulerFactory(props);
            scheduler =  factory.GetScheduler().GetAwaiter().GetResult();
        }

        #endregion

        /// <summary>
        /// Entry
        /// </summary>
        public void Run()
        {
            LogProvider.SetCurrentLogProvider(new Libs.QuartzConsoleLogProvider(_loggerFactory));
            JobFactory().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Exit
        /// </summary>
        public void Stop()
        {
            if (scheduler != null) scheduler.Shutdown(true).Wait();
        }

        #region "Async Job Factory"

        /// <summary>
        /// Set up jobs by spinning though Jobs folder
        /// </summary>
        /// <returns>Task</returns>
        private async Task JobFactory()
        {
            const string loggerKey = "logger";

            ILogger logger = _loggerFactory.CreateLogger<JobEngine>();

            try
            {
                await scheduler.Start();

                DirectoryInfo di = new DirectoryInfo("Jobs");

                int jobIndex = 0;
                foreach (var fi in di.GetFiles("*-job.json"))
                {
                    try
                    {
                        logger.LogInformation($"Attempting to configure job: {fi.Name}");

                        jobIndex++;
                        var json = File.ReadAllText(fi.FullName);
                        var jobInfo = JsonConvert.DeserializeObject<Models.JobInfo>(json);

                        var dllFile = Path.Combine(di.FullName, jobInfo.NameSpace);
                        dllFile += ".dll";
                        var myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllFile);
                        var myType = IJob_Get(myAssembly);

                        JobDataMap map = new JobDataMap();

                        map.Add(loggerKey, logger);

                        foreach (var p in jobInfo.Parameters)
                        {
                            map.Put(p.Name, p.Value);
                        }

                        // TODO: if you have additional data for the job, put it here by .Put() it to the map

                        IJobDetail job = JobBuilder.Create(myType)
                             .WithIdentity("job" + jobIndex.ToString(), "group" + jobIndex.ToString())
                             .UsingJobData(map)
                             .Build();

                        ITrigger trigger = TriggerBuilder.Create()
                            .WithIdentity("trigger" + jobIndex.ToString(), "group" + jobIndex.ToString())
                            .WithCronSchedule(jobInfo.Schedule)
                            .Build();

                        await scheduler.ScheduleJob(job, trigger);

                        logger.LogInformation("\tScheduled: {0}", jobInfo.Schedule);

                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "\tUnable to configure: {0}", e.Message);
                    }
                }

            }
            catch (SchedulerException se)
            {
                // this only gets called if the schedule setup fails
                logger.LogCritical(se, "Scheduler Setup Error");
                throw;
            }
            catch (Exception ex)
            {
                // other types of errors
                logger.LogCritical(ex, "Scheduler Setup Error (Generic)");
                throw;
            }

        }

        /// <summary>
        /// Get the type of the class that implements IJob
        /// </summary>
        /// <param name="assembly">Loaded assembly</param>
        /// <returns>Job Class or Null</returns>
        private static Type IJob_Get(Assembly assembly)
        {
            foreach (System.Reflection.TypeInfo ti in assembly.DefinedTypes)
            {
                if (ti.ImplementedInterfaces.Contains(typeof(IJob)))
                {
                    return ti;
                }
            }
            return null;
        }

        #endregion

    }
}
