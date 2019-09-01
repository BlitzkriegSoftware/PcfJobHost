using System;
using System.Collections.Generic;
using System.Text;

namespace PcfJobHostApp.Engine
{
    /// <summary>
    /// DI: Job Engine
    /// </summary>
    public interface IJobEngine
    {
        /// <summary>
        /// Entry
        /// </summary>
        void Run();

        /// <summary>
        /// Exit
        /// </summary>
        void Stop();
    }
}
