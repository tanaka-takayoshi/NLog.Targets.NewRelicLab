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

2. Configure NLog Target with your favorite way. Note that this Targtet always uses `NewRelicJsonLayout`. You should not specify other targets. You can configure [AsyncTaskTarget properties](https://github.com/NLog/NLog/wiki/How-to-write-a-custom-async-target#asynctasktarget-features), since `NewRelicLogsTarget` inherits `AsyncTaskTarget`.

   Here are configuration examples. You can configure other options as long as NLog supports.

   1. From code

    ```cs 
    using NLog;
    using NLog.Config;
    using NLog.Targets.NewRelicLab.Logs;
    ```
   
    ```cs
    var loggerConfig = new LoggingConfiguration();

    var newRelicTarget = new NewRelicLogsTarget()
    {
        LicenseKey = "<REPLACE_YOUR_LICENSE_KEY>",
        Endpoint = "https://log-api.newrelic.com/log/v1", //default Endpoint is US
        EscapeJson = true, // If you log message is formatted as JSON, you can forcely formate the JSON to plain formatted string.
        // You can specify all properties of AsyncLogTarget
        BatchSize = 10,
        RetryCount = 3
        // you don't have to specify Layout. We always use `NewRelicJsonLayout` whatever layout you specify.
    };
    loggerConfig.AddRuleForAllLevels(newRelicTarget);
    loggerConfig.AddRuleForAllLevels(new ConsoleTarget());
    LogManager.Configuration = loggerConfig;
    ```
   
    2. With NLog.config

    ```xml
    <?xml version="1.0" encoding="utf-8" ?>
    <nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
          xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

      <extensions>
        <add assembly="NLog.Targets.NewRelicLab.Logs"/>
      </extensions>

      <targets>
        <target name="logNewRelic" xsi:type="NewRelicLogs" 
                endpoint="https://log-api.newrelic.com/log/v1"
                licenseKey="REPLACE_YOUR_KEY"
                escapeJson="false"
                batchSize="100"
                retryCount="3"
                taskDelayMilliseconds="1000"
                />
        <target name="logconsole" xsi:type="Console" />
      </targets>

      <rules>
        <logger name="*" minlevel="Debug" writeTo="logconsole" />
        <logger name="*" minlevel="Debug" writeTo="logNewRelic" />
      </rules>
    </nlog>
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

## Release Notes

### 0.2.1 (secirity fix)

- Bumps System.Text.Encodings.Web from 5.0.0 to 5.0.1.

### 0.2.0

- more InternalLog
- refactor GZipZStringContent class
- support force escape JSON
- add ConfigureAwait(false) to async invocation
- add NLog.config example

### 0.1.0

Experimental Release.

## File an issue

Please enable [NLog Internal Logging](https://github.com/NLog/NLog/wiki/Internal-Logging) and submit issue with your environment, configuration and logs.
