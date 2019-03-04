using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLimiter.Service;
using CoreLib.Net;

namespace FiltersAndRules
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

                Filter fltWhole;

                if (null == (fltWhole = svc.Filters.Find(x => x.Name == "Test-WholeMachineExceptSystemServices")))
                {
                    // create a filter for whole computer, but by-pass system services - all "system" and svchost.exe processes
                    fltWhole = new Filter("Test-WholeMachineExceptSystemServices");

                    FFPathEqual fnc1 = new FFPathEqual();
                    fnc1.IsMatch = false; // catch all traffic that DOESN'T match the value
                    fnc1.Values.Add(@"system"); // system process
                    fnc1.Values.Add(@"c:\windows\system32\svchost.exe"); // path - it could be different on your machine

                    fltWhole.Functions.Add(fnc1);

                    svc.AddFilter(fltWhole);

                    fltWhole = svc.Filters.Find(x => x.Name == "Test-WholeMachineExceptSystemServices");

                    // NOTE: If you create a filter without any filter functions, then it will catch all traffic on the machine
                    // read more about filters here: https://www.netlimiter.com/docs/basic-concepts/filters
                }


                LimitRule limWhole = null;
                List<Rule> ruleWholeList;

                if (null == (ruleWholeList = svc.Rules.FindAll(x => x.FilterId == fltWhole.Id)) || ruleWholeList.Count == 0)
                {
                    limWhole = new LimitRule(RuleDir.In, 200000); // create limit rule (size in Bytes per second)
                    limWhole.IsEnabled = true;

                    // create Start/Stop conditions - limit is activated every day at 12PM and deactivated at 18PM
                    TimeCondition cndStartLimitWhole = new TimeCondition();
                    cndStartLimitWhole.Action = RuleConditionAction.Start;
                    cndStartLimitWhole.TimeConditionType = TimeConditionType.EveryDay;
                    cndStartLimitWhole.Time = new DateTime(2019, 1, 1, 12, 0, 0);

                    TimeCondition cndStopLimitWhole = new TimeCondition();
                    cndStopLimitWhole.Action = RuleConditionAction.Stop;
                    cndStopLimitWhole.TimeConditionType = TimeConditionType.EveryDay;
                    cndStopLimitWhole.Time = new DateTime(2019, 1, 1, 18, 0, 0);

                    limWhole.Conditions.Add(cndStartLimitWhole);
                    limWhole.Conditions.Add(cndStopLimitWhole);

                    // add the rule to NetLimiter system
                    limWhole = svc.AddRule(fltWhole.Id, limWhole) as LimitRule;

                    // load all rules for the filter
                    ruleWholeList = svc.Rules.FindAll(x => x.FilterId == fltWhole.Id);
                }
                else
                {// rules for our filter already exist
                    foreach (var r in ruleWholeList)
                    {
                        Console.WriteLine("Rule (Filter: {0}) - id: {1} ", r.FilterId, r.Id);
                    }
                }

                Console.WriteLine("A test Filter and rule were created. Check NetLimiter if they created correctly.");
                Console.WriteLine("Press ENTER to delete the filter, delete the rule and Exit.");
                Console.ReadLine();

                // remove rules
                foreach (var r in ruleWholeList)
                    svc.RemoveRule(r);

                // remove filter
                svc.RemoveFilter(fltWhole);
            }

        }
    }
}
