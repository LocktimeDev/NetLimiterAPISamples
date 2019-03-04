using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLimiter.Service;
using CoreLib.Net;

namespace NLApiSamples
{
    class Program
    {
        static void Main(string[] args)
        {
            // create instance of NLService
            using (NLService svc = new NLService())
            {
                try
                {
                    // connect to NL service on local machine
                    svc.Connect();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Connect exception: {0}", e.Message);
                    return;
                }

                // register event handler - it is called every time when there are any Blocker requests
                svc.FirewallRequestChange += new EventHandler(delegate (Object o, EventArgs a)
                {
                    foreach (var r in svc.FirewallRequests)
                    {
                        // reply to all request, by returning BLOCK
                        svc.ReplyFirewallRequest(r, FwAction.Block);

                        Console.WriteLine("Request local/remote {0}:{1}/{2}:{3} - {4} - Denied!",
                        r.RemoteAddress, r.RemotePort, r.LocalAddress, r.LocalPort, r.ApplicationPath);
                    }
                });

                Console.ReadKey();
            }
        }
    }
}
