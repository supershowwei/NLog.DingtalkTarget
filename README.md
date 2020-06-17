# NLog.DingtalkTarget

This is NLog target for Dingtalk. Check following configuration:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" autoReload="true">
  <extensions>
    <add assembly="NLog.DingtalkTarget"/>
  </extensions>
  <targets async="true">
    <target name="Dingtalk" xsi:type="Dingtalk"
            webhookUrl="https://oapi.dingtalk.com/robot/send?access_token=xxx"
            secret="(Optional)xxx"
            layout="${message}" />
  </targets>
  <rules>
    <logger name="*" minlevel="Debug" writeTo="Dingtalk" />
  </rules>
</nlog>
```
