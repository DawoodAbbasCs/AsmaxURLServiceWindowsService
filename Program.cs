using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace WindowsService1
{
    internal static class Program
    {
        private static ILogger logger;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static async Task Main()
        {
            ConfigureLogger();
            Service1 objService1 = new Service1();
            await objService1.GetTemplates();
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            ServiceBase.Run(ServicesToRun);
        }

        private static void ConfigureLogger()
        {
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            string logFile = Path.Combine(logDirectory, $"log_{DateTime.Now.ToString("dd_MM_yyyy")}.txt");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFile)
                .CreateLogger();

            logger = Log.Logger;
        }
    }
}
