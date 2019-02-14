using Cake.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cake.XmlDoc.Checker
{
    public class ConsoleLogger : ICakeLog
    {
        Verbosity ICakeLog.Verbosity { get => Verbosity.Normal; set { } }

        void ICakeLog.Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}
