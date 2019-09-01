using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace PcfJobHostApp.Libs
{
    /// <summary>
    /// Wrap Microsoft.Extensions.Logging for Quartz
    /// </summary>
    public class QuartzConsoleLogProvider : Quartz.Logging.ILogProvider
    {
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;

        /// <summary>
        /// CTOR w. Parameter
        /// </summary>
        /// <param name="loggerFactory">ILoggerFactory</param>
        public QuartzConsoleLogProvider(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Required Method to Get a Logger, for the given name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Quartz.Logging.Logger</returns>
        public Quartz.Logging.Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (func != null)
                {
                    var state = string.Format(func(), parameters);

                    var logger = _loggerFactory.CreateLogger("Quartz");

                    switch (level)
                    {
                        case Quartz.Logging.LogLevel.Trace:
                            logger.LogTrace(state);
                            break;
                        case Quartz.Logging.LogLevel.Debug:
                            logger.LogDebug(state);
                            break;
                        case Quartz.Logging.LogLevel.Warn:
                            if (exception != null)
                                logger.LogWarning(exception, state);
                            else
                                logger.LogWarning(state);
                            break;
                        case Quartz.Logging.LogLevel.Error:
                            if (exception != null)
                                logger.LogError(exception, state);
                            else
                                logger.LogError(state); break;
                        case Quartz.Logging.LogLevel.Fatal:
                            if (exception != null)
                                logger.LogCritical(exception, state);
                            else
                                logger.LogCritical(state);
                            break;
                        default:
                            logger.LogInformation(state);
                             break;
                    }


                }
                return true;
            };
        }

        /// <summary>
        /// Not Used
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Used
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IDisposable OpenMappedContext(string key, string value)
        {
            throw new NotImplementedException();
        }
    }
}
