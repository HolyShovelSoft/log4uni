# log4net.unity

log4net helper classes and UnityEngine.Debug appender for Unity.

Для большей совместимости использован .NET Framework 3.5.

Данный проект тестировался только на стэндалон платформах (преимущественно на Windows), но должно работать и на остальных платформах.

## Instalation

1. Скачайте последний релиз по [ссылке](https://github.com/HolyShovelSoft/log4net.unity/releases).
2. Распакуйте файлы log4net.unity.dll, log4net.unity.editor.dll и log4net.dll в ваш Unity проект.
3. Убедитесь, что log4net.unity.editor.dll настроена на использование только в редакторе.

МЕСТО ДЛЯ СКРИНШОТА

## UnityDefaultLogAppender and Unity log handlers

**UnityDefaultLogAppender** это **Appender** для log4net который интегрирует логгеры log4net с логгером Unity (как для рантайма, так и для редактора). Так же данный плагин заменяет стандартный Unity **ILogHandler** на handler который отправляет все стандартные вызовы **Debug.Log** в log4net и позволяет контролировать посредством конфигураций стандартный способ логгирования.
Все стандартные вызовы будут интерпретированы как log4net логгеры с именем "Unity".

## Very Simple Usage

После установки достаточно в коде использовать старые методы:

``` csharp
//В данном случае будет использована общая для всех log4net ILog инстанция
Debug.Log("Debug message");
```

или использовать логгеры как это задумано самим log4net =)

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

И все, вы великолепны, вы уже используете log4net! =)

## Simple Usage

Но как же конфигурации и все то, за что мы любим log4net? Есть несколько способов конфигурировать log4net в рамках данного плагина. Самый простой из них, это расположить файл log4net конфигурации (единственным дополнительным требованием к конфигу требованием являетсмя то, что log4net node должен быть рутом) в следующих местах (в порядке проверки):

1. В папку **Application.persistentDataPath**. Валидными конфигурационными файлами являются файлы **log4net.xml**, **log4net.config** или **log4net.txt** с xml данными конфигурации.
2. В папку **Application.dataPath**. Валидными конфигурационными файлами являются файлы **log4net.xml**, **log4net.config** или **log4net.txt** с xml данными конфигурации.
3. В любую папку Resources в рамках проекта. Валидным является любой **TextAsset** с именем **log4net**.

В случае если ни один из данных способов получения конфигурации не обнаружит валидного конфига, будет использована конфигурация по умолчанию. А именно:

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

Во всех случаях при изменении какого либо из файлов при работе в редакторе будет вызвано переконфигурирование log4net. При этом в билде изменение файлов не отслеживается, учитывается состояние на момент запуска.

> Данную конфигурацию так же можно сохранить в файл с помощью **Unity** комманды **Tools/log4net/Make Default Config**.

## Advanced Usage

Но что если нам нужно сконфигурировать log4net кодом, а не из конфигурационного файла, или использовать сложные условия для выбора конфигурации? Для данного случая предусмотрен способ кастомизации процесса конфигурирования посредством имплементации следующего интерфейса:

``` csharp
public interface IConfigurator
{
    int Order { get; }
    event Action OnChange;
    void TryConfigure();
}
```

Для понимания данного интерфейса нужно разобрать как действует конфигуратор в данном плагине. При запуске редактора (а так же при рекомпиляции) или билда происходят следующие операции:

1. Очистка существующей конфигурации log4net
2. Сбор информации обо имплементациях интерфейса **IConfigurator**
3. Заполнение списка всех найденых конфигураторов по следующим правилам:
    - Классы (not nested) не унаследованные от **UnityEngine.Object**, имеющие конструктор без параметров (или без объявленных конструкторов) и не помеченные **[ExcludeFromSearch]** аттрибутом инстанцируются автоматически и помещается в общий список конфигураторов.
    - Объекты наследованные от **ScriptableObject** помещенные в **Resources** так же помещаются в общий список конфигураторов.
    - В список добавляются конфигураторы по умолчанию с наибольшими возможными значениями поля **Order**.
4. Полученный список сортируется от наименьшого значения поля **Order** к наибольшему.
5. Происходит перебор списка конфигураторов, у каждого происходит вызов метода **TryConfigure**. В случае если после этого вызова **log4net** сконфигурирован, то перебор заканчивается.

Помимо этого для добавления или удаления конфигураторов можно использовать следующие методы:

``` csharp
ConfigProcessor.AddConfigurator(myConfigurator);
ConfigProcessor.RemoveConfigurator(myConfigurator);
```

Вызов данных методов вызовет переконфигурирование, так же как и срабатывания события **OnChange** у любого добавленого в общий список конфигуратора.

> **Imprtant**
> Если вам необходимо логгирование внутри конфигуратора используйте **UnityDefaultLogHandler.UnityHandler**. Это необходимо потому что во время работы конфигураторов log4net не сконфигурирован и все логи будут уходить "вникуда". **UnityDefaultLogHandler.UnityHandler** это фолбэк в стандартную систему логгирования **Unity**.

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