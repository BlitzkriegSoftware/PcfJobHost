using System;
using System.Collections.Generic;
using System.Text;

namespace PcfJobHostApp.Models
{
    /// <summary>
    /// This defines a job
    /// </summary>
    public class JobInfo
    {
        /// <summary>
        /// The namespace to try and load dynamically
        /// </summary>
        public string NameSpace { get; set; }
        /// <summary>
        /// Quatrz version of CRON Schedule
        /// </summary>
        public string Schedule { get; set; }
        /// <summary>
        /// Parameters to pass job, should not be NULL
        /// </summary>
        public List<Parameter> Parameters { get; set; }

    }
}
