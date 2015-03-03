using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ComponentModel;
using System.ServiceProcess;
using System.Diagnostics;
using tracktor.service;

namespace tracktor.host
{
    class Program
    {
        public class TracktorWindowsService : ServiceBase
        {
            public System.ServiceModel.ServiceHost serviceHost = null;
            public TracktorWindowsService()
            {
                ServiceName = "TracktorHost";
            }

            // Start the Windows service.
            protected override void OnStart(string[] args)
            {
                Trace.TraceInformation("Starting Tracktor Service in SERVICE mode.");
                if (serviceHost != null)
                {
                    serviceHost.Close();
                }
                serviceHost = new System.ServiceModel.ServiceHost(typeof(TracktorService));
                serviceHost.Open();
                Trace.TraceInformation("Tracktor Service is running on:");
                foreach (var address in serviceHost.BaseAddresses)
                {
                    Trace.TraceInformation(address.ToString());
                }
            }

            protected override void OnStop()
            {
                Trace.TraceWarning("Stopping Tracktor Service...");
                if (serviceHost != null)
                {
                    serviceHost.Close();
                    serviceHost = null;
                }
            }
        }

        protected static void RunStandalone()
        {
            Console.WriteLine("Starting Tracktor Service in STANDALONE mode.");
            System.ServiceModel.ServiceHost svcHost = null;
            try
            {
                svcHost = new System.ServiceModel.ServiceHost(typeof(TracktorService));
                svcHost.Open();
                Console.WriteLine("Service is running on:");
                foreach (var address in svcHost.BaseAddresses)
                {
                    Console.WriteLine(address.ToString());
                }
            }
            catch (Exception ex)
            {
                svcHost = null;
                Console.WriteLine("Service can not be started." + ex.Message);
            } if (svcHost != null)
            {
                Console.WriteLine("Press any key to close the Service");
                System.Console.ReadKey();
                svcHost.Close();
                svcHost = null;
            }
        }

        static void Main(string[] args)
        {
            TracktorStartup.Initialize();
            if (args != null && args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]) && args[0].Equals("/standalone", StringComparison.InvariantCultureIgnoreCase))
            {
                RunStandalone();
            }
            else
            {
                ServiceBase.Run(new TracktorWindowsService());
            }
        }
    }
}
