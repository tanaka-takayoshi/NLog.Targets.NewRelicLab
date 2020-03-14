# NLog.Targets.NewRelicLab
An NLog target that sends logs to New Relic Logs

![.NET Core](https://github.com/tanaka-takayoshi/NLog.Targets.NewRelicLab/workflows/.NET%20Core/badge.svg)

Note: This is an unofficial package. Since this is an experimental library, please consider using more robust log shipping method (e.g. fluentd) in the production.

## Requirements

- New Relic Logs subscription ([License Key](https://docs.newrelic.com/docs/accounts/install-new-relic/account-setup/license-key) or [Insert API key](https://docs.newrelic.com/docs/apis/get-started/intro-apis/types-new-relic-api-keys#event-insert-key) is required)
- New Relic APM subscription if you'd like to enable Logs in Context
- NLog 4.6.0 or above and NewRelic.LogEnrichers.NLog 1.0.0 or above. (These are dependent libraries.)

## Usage

1. Add [NLog.Targets.NewRelicLab.Logs](https://www.nuget.org/packages/NLog.Targets.NewRelicLab.Logs) package.

2. Configure NLog Target as follows. Note that this Targtet always uses `NewRelicJsonLayout`. You should not specify other targets.

  ```cs 
  using NLog;
  using NLog.Config;
  using NLog.Targets.NewRelicLab.Logs;
  ```
  
  ```cs
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
  ```
  
3. (Option) Instead of specify LicenseKey in the code or configuration file, you can specify one of the following ENVIRONMENT VARIABLEs to start the process: `NEW_RELIC_LICENSE_KEY` is for a New Relic License Key, or `NEW_RELIC_INSERT_KEY` is for an Insert API key.
  
4. Output your log with NLog.

  ```cs
    var logger = LogManager.GetLogger("Example");
    for (int i = 0; i < 100; i++)
    {
        logger.Info($"Hello New Relic Logs {i}");
    }
  ```
  
5. You will see your log in New Relic Logs.

