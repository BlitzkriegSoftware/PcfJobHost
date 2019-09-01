namespace PcfJobHostApp
{
    #region "Usings"

    using Microsoft.Extensions.Logging;

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
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using PcfJobHostApp.Engine;

    #endregion

    class Program
    {
        #region "Fields, Constants, Properties"
        static bool shouldRun = true;
        static int exitCode = 0; // Zero is good, not zero is bad
        static ILogger logger;

        // Environment Vars
        const string SCHEDULED_TASKS_STOP_ENV_KEY = "JOBHOST_STOP";
        const int SCHEDULED_TASKS_STOP_VALUE = 1;
        const string SCHEDULED_TASKS_SLEEP_ENV_KEY = "JOBHOST_STOP_SLEEP";
        const int SCHEDULED_TASKS_SLEEP_DEFAULT = 5000;
        const int SCHEDULED_TASKS_SLEEP_MIN = 1000;
        
        // Properties
        static string[] Args { get; set; }

        #endregion

        static int Main(string[] args)
        {
            #region "Load up optional args"
            Args = args;
            #endregion

            #region "Global error handler"
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            #endregion

            #region "Useful Info"

            var lf = Services.GetService<ILoggerFactory>();
            logger = lf.CreateLogger<Program>();

            logger.LogInformation("To Stop Scheduler Cleanly Set Environment Variable '{0}' to the number {1}", SCHEDULED_TASKS_STOP_ENV_KEY, SCHEDULED_TASKS_STOP_VALUE);
            logger.LogInformation("To Set Scheduler Sleep MS, Set Environment Variable '{0}' to the number >= {1}", SCHEDULED_TASKS_SLEEP_ENV_KEY, SCHEDULED_TASKS_SLEEP_MIN);

            // This is optional but useful for debugging service startup, comment out if not desired
            var configDumper = Services.GetService<IConfigurationDumper>();
            configDumper.Run();

            #endregion

            #region "Quartz Scheduler"

            var jobEngine = Services.GetService<IJobEngine>();
            jobEngine.Run();

            #endregion

            #region "Infinite loop (nearly) to pace scheduled tasks while detecting exit"

            // Notice a not handled exception from the scheduler will be caught by global error handler

            // We can also gracefully exit, but setting return to false
            while (shouldRun)
            {
                shouldRun = Detect_ShouldRun();
                var sleepTime = SleepTime();
                if (shouldRun)
                {
                    logger.LogInformation("Sleeping for {0} milliseconds", sleepTime);
                    Thread.Sleep(sleepTime);
                }
            }

            jobEngine.Stop();

            #endregion

            #region "Terminate with exit code"
            Environment.ExitCode = exitCode;
            return exitCode;
            #endregion
        }

        #region "Builder"

        private static IServiceProvider _services;

        public static IServiceProvider Services
        {
            get
            {
                if (_services == null)
                {
                    // Create service collection
                    var serviceCollection = new ServiceCollection();

                    // Build DI Stack inc. Logging, Configuration, and Application
                    ConfigureServices(serviceCollection);

                    // Create service provider
                    _services = serviceCollection.BuildServiceProvider();
                }
                return _services;
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Logging
            services.AddLogging(loggingBuilder => {
                // This line must be 1st
                loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);

                // Console is generically cloud friendly
                loggingBuilder.AddConsole();
            });

            // Configuration
            var config = new ConfigurationBuilder()
                           .AddEnvironmentVariables()
                           .AddCommandLine(Args)
                           .Build();

            services.AddSingleton(config);

            services.AddTransient<Engine.IConfigurationDumper, Engine.ConfigurationDumper>();

            services.AddSingleton<Engine.IJobEngine, Engine.JobEngine>();
        }

        #endregion

        #region "environment vars"

        /// <summary>
        /// Detect if the environment variable has been set to shutdown scheduler
        /// </summary>
        /// <returns>True should run, False should stop (environment variable set to 1)</returns>
        private static bool Detect_ShouldRun()
        {
            bool flag = true;
            var value = Environment.GetEnvironmentVariable(SCHEDULED_TASKS_STOP_ENV_KEY);
            if (!string.IsNullOrWhiteSpace(value))
            {
                int i = 0;
                if (int.TryParse(value, out i))
                {
                    if (i == SCHEDULED_TASKS_STOP_VALUE) flag = false;
                }
            }
            return flag;
        }

        private static int SleepTime()
        {
            int sleep = SCHEDULED_TASKS_SLEEP_DEFAULT;
            var value = Environment.GetEnvironmentVariable(SCHEDULED_TASKS_STOP_ENV_KEY);
            if (!string.IsNullOrWhiteSpace(value))
            {
                int i = 0;
                if (int.TryParse(value, out i))
                {
                    if (i == SCHEDULED_TASKS_STOP_VALUE) sleep = i;
                    if (sleep < SCHEDULED_TASKS_SLEEP_MIN) sleep = SCHEDULED_TASKS_SLEEP_MIN;
                }
            }
            return sleep;
        }

        #endregion

        #region "Global Error Handler"

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = new ApplicationException("Unhandled exception with no tracing caused crash, please see logs");
            if ((e != null) && (e.ExceptionObject != null))
            {
                var ex2 = e.ExceptionObject as Exception;
                if (ex2 != null) ex = ex2;
            }

            exitCode = -1; // unhandled exception
            shouldRun = false;
            logger.LogError(ex, "CurrentDomain_UnhandledException");
        }

        #endregion

    }
}
