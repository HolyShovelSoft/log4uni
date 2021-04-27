# log4net.unity

log4net helper classes and UnityEngine.Debug appender for Unity.

For greater compatibility .NET Framework 3.5 is used.

This project was tested only on platforms available to the author (Windows, Android, WebGL), but we expect that plugin must work fine on other plaforms, supported by **Unity**.

> Projects author does not guarantee **log4net** or third-party **Appenders** working properly, as some platforms are limited in their abilities, and could not be supporting some **.net** functions.
> Also this package use custom patched **log4net.dll** for work with **Unity** il2cpp and AOT platforms.

## Instalation

1. Download the latest release at [link](https://github.com/HolyShovelSoft/log4net.unity/releases).
2. Unpack files log4net.unity.dll, log4net.unity.editor.dll and log4net.dll into your Unity project.
3. Make sure that log4net.unity.editor.dll is set up to be used only in the editor..

## UnityDefaultLogAppender and Unity log handlers

**UnityDefaultLogAppender** is an **Appender** for log4net, which integrates log4net loggers with Unity logger (for both runtime and editor). This plugin also replaces default Unity **ILogHandler** with a handler which sends all **Debug.Log** standard calls into log4net and lets you control default logging method with configurations.
All standard calls will be interpreted as log4net loggers with a name "Unity".

## Very Simple Usage

After installation all that's required is to use old methods in the code:

``` csharp
//In this case common ILog instance will be used
Debug.Log("Debug message");
```

Or to use loggers as indended by log4net =)

``` csharp
private static readonly ILog Log = LogManager.GetLogger("MyLogger");
//or
private static readonly ILog Log = LogManager.GetLogger(typeof(MyType));
//and after it use this instances for logs
if (Log.IsInfoEnabled)
{
    Log.Info("Info message");
}
//or for .net 4.6 and higher you can use extention methods
 Log.Info()?.Call("Info message");
```

Thats it, you are great, you are already using log4net! =)

## Simple Usage

But what about configurations and what we love log4net for? There are several ways to configure log4net in this plugin. The easiest is to place log4net configuration file( the only addition requirment is that **log4net** node must be root) in the following places (in the checking order):

1. Into the folder **Application.persistentDataPath**. Valid configuration files are **log4net.xml**, **log4net.config** or **log4net.txt** with xml configuration data.
2. Into the folder **Application.dataPath**. Valid configuration files are **log4net.xml**, **log4net.config** or **log4net.txt** with xml configuration data.
3. Into any Resources folder within the project. Any **TextAsset** with name **log4net** is valid. Files placed in subfolders not supported.

In case none of the following configuration acquisition methods doesn't find a valid configuration, default configuration will be used. In particular:

``` xml
<?xml version="1.0" encoding="utf-8"?>
<log4net>
    <appender name="unityConsole" type="log4net.Unity.UnityDefaultLogAppender">
        <layout type="log4net.Layout.PatternLayout">
            <conversionPattern value="[%thread][%level][%logger] %message"/>
        </layout>
    </appender>
    <root>
        <level value="INFO"/>
        <appender-ref ref="unityConsole"/>
    </root>
</log4net>
```

When any of the files if updated when working in editor, log4net reconfiguration will be called. In build file updating is not tracked, only the state at launch is considered.

> This configuration can also be saved to file using **Unity** command **Tools/log4net/Make Default Config**.

## Advanced Usage

But what if we need to configure log4net with code, and not by updating configuration file, or to use complex conditions for selecting configuration? In this case you can use the following interface to customize configuration process:

``` csharp
public interface IConfigurator
{
    int Order { get; }
    event Action OnChange;
    void TryConfigure();
}
```

To understand this interface, we need to understand how configuration works in this plugin. When launching editor (and when recompiling) or during the build running the following operations occur:

1. Reset of existing log4net configuration.
2. Collect of all information about **IConfigurator** interface implementations.
3. Filling in the list of all found configuratiors according to the following rules:
    - Classes (not nested), not inhereted from **UnityEngine.Object**, that have constructor without parameters (or without declared constructors) and not marked with a **[ExcludeFromSearch]** attribute, are instantiated automatically and are placed in the common configurators list.
    - Objects inherited from **ScriptableObject** and placed in **Resources** are also placed in the common configurators list.
    - Default configurators added to the list (with the highest possible **Order** value).
4. Resulting list is sorted from the lowest **Order** value to highest.
5. Every configurator in the list is calling a method **TryConfigure** one by one. If after calling that method **log4net** is configurated, then list iterating is stopped.

Besides this you can also use following methods to add or delete configurators:

``` csharp
ConfigProcessor.AddConfigurator(myConfigurator);
ConfigProcessor.RemoveConfigurator(myConfigurator);
```

Calling these methods causes reconfiguration, as well as triggering **OnChange** event in any configurator added to the list.

> **Imprtant**
> If you need to use logging inside configurator, use **UnityDefaultLogHandler.DefaultUnityLogger**. This is necessary because during configurators work, log4net is not configured and all logs will go into the void. **UnityDefaultLogHandler.DefaultUnityLogger** is a fallback to a standard **Unity** logging system.

## Configurators samples

``` csharp
[ExcludeFromSearch]
public class SimpleXMLConfigurator : IConfigurator
{
    public int Order { get; }
    public event Action OnChange;
    private string xml;

    public SimpleXMLConfigurator(int order, string xml)
    {
        Order = order;
        this.xml = xml;
    }

    public void TryConfigure()
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        log4net.Config.XmlConfigurator.Configure(xmlDoc.DocumentElement);
    }
}
```

``` csharp
[ExcludeFromSearch]
public class SimpleCodeConfigurator : IConfigurator
{
    public int Order { get; }
    public event Action OnChange;

    public SimpleCodeConfigurator(int order)
    {
        Order = order;
    }

    public void TryConfigure()
    {
        var hierarchy = (Hierarchy)LogManager.GetRepository();

        var patternLayout = new PatternLayout();
        patternLayout.ConversionPattern = "[%thread][%level][%logger] %message";
        patternLayout.ActivateOptions();

        var appender = new UnityDefaultLogAppender();
        appender.Layout = patternLayout;
        hierarchy.Root.AddAppender(appender);

        hierarchy.Root.Level = Level.Info;
        hierarchy.Configured = true;
    }
}
```

## Contacts

- [twitter.com/holyshovelsoft](https://twitter.com/holyshovelsoft)
- [support@holyshovelsoft.com](mailto:support@holyshovelsoft.com)
- [andreich@holyshovelsoft.com](mailto:andreich@holyshovelsoft.com)
