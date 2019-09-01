using Quartz;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ExampleJob
{
    /// <summary>
    /// A Very Basic Example of a job that does one unit-of-work
    /// </summary>
    public class ExampleJob : IJob
    {
        const string loggerKey = "logger";

        /// <summary>
        /// Empty CTOR must be public
        /// </summary>
        public ExampleJob() { }

        /// <summary>
        /// Implemenation of IJob
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            ILogger logger = null; 

            try
            {
                var dataMap = context.JobDetail.JobDataMap;

                #region "Unit of Work"
                // In this example we do nothing except dump out job info and wait

                var sb = new StringBuilder();

                logger = (ILogger) dataMap["logger"];

                foreach (var d in dataMap)
                {
                    if (!d.Key.Equals(loggerKey))
                    {
                        sb.Append(d.Key);
                        sb.Append('=');
                        sb.Append(d.Value);
                        sb.Append(';');
                    }
                }

                var msg = string.Format("One Job, Ok. Key: {0}, InstanceId: {1}, Date: {2}, Data: {3}", context.JobDetail.Key, context.FireInstanceId, context.FireTimeUtc, sb.ToString());

                if (logger != null) logger.LogInformation(msg);
                else await Console.Out.WriteLineAsync(msg);

                await Task.Delay(1);

                #endregion
            }
            catch (Exception ex)
            {
                // MUST Wrap Exceptions! (Sigh)
                var ex2 = new JobExecutionException(ex, false);
                var msg = ex.Message;
                if (logger != null) logger.LogError(ex, "ExampleJob");
                else await Console.Error.WriteLineAsync(msg);
                throw ex2;
            }
        }

    }
}
