#region "usings"

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PcfJobHostApp.Libs;
using System.Collections.Generic;

#endregion

namespace PcfJobHostApp.Engine
{

    /// <summary>
    /// This dumps assembly and configuration info, which is useful for debugging
    /// </summary>
    public class ConfigurationDumper : IConfigurationDumper
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly Microsoft.Extensions.Configuration.IConfigurationRoot _config;

        /// <summary>
        /// Must be private for dependancy injection to find the right CTOR
        /// </summary>
        private ConfigurationDumper() { }

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="loggerFactory">ILoggerFactory</param>
        /// <param name="config">IConfigurationRoot</param>
        public ConfigurationDumper(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, IConfigurationRoot config)
        {
            _logger = loggerFactory.CreateLogger<ConfigurationDumper>();
            _config = config;
        }

        /// <summary>
        /// Main Entry Point
        /// </summary>
        public void Run()
        {
            _logger.LogInformation("\nAssembly Info\n");
            var ai = AssemblyInfo();
            foreach (var kv in ai)
            {
                _logger.LogInformation("{0}: {1}", kv.Key, kv.Value);
            }

            _logger.LogInformation("\nConfiguration\n");
            foreach (var c in _config.AsEnumerable())
            {
                _logger.LogInformation("Key: {0}, Value: {1}", c.Key, c.Value);
            }
        }

        private List<KeyValuePair<string, string>> AssemblyInfo()
        {
            var results = new List<KeyValuePair<string, string>>();
            var propsToGet = new List<string>() { "AssemblyProductAttribute", "AssemblyCopyrightAttribute", "AssemblyCompanyAttribute", "AssemblyDescriptionAttribute", "AssemblyFileVersionAttribute" };
            var assembly = typeof(Program).Assembly;
            foreach (var attribute in assembly.GetCustomAttributesData())
            {
                if (propsToGet.Contains(attribute.AttributeType.Name))
                {
                    if (!attribute.TryParse(out string value))
                    {
                        value = string.Empty;
                    }

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        results.Add(new KeyValuePair<string, string>(attribute.AttributeType.Name, value));
                    }
                }
            }
            return results;
        }

    }
}
