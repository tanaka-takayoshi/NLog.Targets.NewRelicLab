using NLog;
using NLog.Config;
using NLog.Targets.NewRelicLab.Logs;
using System;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var loggerConfig = new LoggingConfiguration();

            var newRelicTarget = new NewRelicLogsTarget()
            {
                LicenseKey = "<Your_NewRelic_LicenseKey>",
                BatchSize = 10, //default size is 10
                Endpoint = "https://log-api.newrelic.com/log/v1", //default Endpoint is US
                // you should not specify Layout. We always use `NewRelicJsonLayout` whatever layout you specify.
            };
            loggerConfig.AddRuleForAllLevels(newRelicTarget);
            LogManager.Configuration = loggerConfig;
            var logger = LogManager.GetLogger("Example");
            for (int i = 0; i < 100; i++)
            {
                logger.Info($"Hello New Relic Logs {i}");
            }

            Console.ReadLine();
        }
    }
}
