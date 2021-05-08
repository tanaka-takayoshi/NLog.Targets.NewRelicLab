using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.NewRelicLab.Logs;
using NLog.Targets.Wrappers;
using System;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //NLog.Common.InternalLogger.LogToConsole = true;
            //NLog.Common.InternalLogger.LogLevel = LogLevel.Trace;
            //NLog.Common.InternalLogger.LogFile = @"C:\temp\internal.log";
            //var loggerConfig = new LoggingConfiguration();

            //var newRelicTarget = new NewRelicLogsTarget()
            //{
            //    LicenseKey = "<REPLACE_YOUR_LICENSE_KEY>",
            //    Endpoint = "https://log-api.newrelic.com/log/v1", //default Endpoint is US
            //    EscapeJson = true, // If you log message is formatted as JSON, you can forcely formate the JSON to plain formatted string.
            //    // You can specify all properties of AsyncLogTarget
            //    BatchSize = 10, 
            //    RetryCount = 3
            //    // you don't have to specify Layout. We always use `NewRelicJsonLayout` whatever layout you specify.
            //};
            //loggerConfig.AddRuleForAllLevels(newRelicTarget);
            //loggerConfig.AddRuleForAllLevels(new ConsoleTarget());
            //LogManager.Configuration = loggerConfig;
            
            var logger = LogManager.GetLogger("Example");
            int i = 0;
            while (true)
            {

                logger.Info($"こんにちはNew Relicログ {i++}");
                Task.Delay(TimeSpan.FromMilliseconds(10)).GetAwaiter().GetResult();
            }
            Console.ReadLine();
            LogManager.Shutdown();

        }
    }
}
