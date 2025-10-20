using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
//using UWPExampleCS.Constants;

// 20200404
// https://gist.github.com/litetex/b88fe0531e5acea82df1189643fb1f79
// https://stackoverflow.com/questions/29470863/serilog-output-enrich-all-messages-with-methodname-from-which-log-entry-was-ca
// https://stackoverflow.com/questions/47576733/c-sharp-asp-net-core-serilog-add-class-name-and-method-to-log

// only used by Here() extension method
// as it has to be called for each logging

// but keeping as an example
// Also see CallerEnricher which is being used [and has no ctor] but uses reflection, so big overhead

// this code is designed to be used as an extension 
// eg Logger.Here().Information("Hello, world!");
// see LogExtensions
// https://stackoverflow.com/questions/48268854/how-to-get-formatted-output-from-logevent/48272467#48272467

// https://dejanstojanovic.net/aspnet/2018/october/extending-serilog-with-additional-values-to-log/
// Pulling out values that you added from the LogContext is the same as with using log enricher. Just specify the property name in the log line pattern in your config and values from the runtime will start appearing in your log.
// Enrichers work pretty much the same way as adding properties to the context. They are just nicer way to re-use your logic and share it among the projects, 
// pretty much the same concept .NET Core uses with extension methods for adding middlewares to the pipeline.

// InvocationContextEnricher enricher = new InvocationContextEnricher(typeof(T).FullName, callerMemberName, callerFilePath, callerLineNumber);
//        return Log.Logger.ForContext(enricher);
namespace JAGLog.Enrichers
{
    public class InvocationContextEnricher : ILogEventEnricher
    {
//        public InvocationContextEnricher(string sourceContext, string callerMemberName, string callerFilePath, int callerLineNumber)
// see logextensions
        public InvocationContextEnricher(string sourceContext = "", [CallerMemberName] string callerMemberName = "", 
            [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            SourceContext = sourceContext;
            CallerMemberName = callerMemberName;
            CallerFilePath = callerFilePath;
            CallerLineNumber = callerLineNumber;
            //Debug.WriteLine("In ctor"+CallerMemberName);
        }

        public string SourceContext { get; protected set; }
        public string CallerMemberName { get; protected set; }
        public string CallerFilePath { get; protected set; }
        public int CallerLineNumber { get; protected set; }

     //   public static string SourceContextPropertyName { get; } = Constants.OtherConstants.SourceContextPropertyName;
        public static string SourceContextPropertyName { get; } = "SourceContext";
        public static string CallerMemberNamePropertyName { get; } = "CallerMemberName";
        public static string CallerFilePathPropertyName { get; } = "CallerFilePath";
        public static string CallerLineNumberPropertyName { get; } = "CallerLineNumber";

        // 20200413
        // this ref is linked to caller enricher????
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
//        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, [CallerMemberName] string callerMemberName = "")
        {
            // what we really want is to able to pass [CallerMemberName] etc. into Enrich here, but Serilog does not allow that method signature for Enrich:
            //Error CS0535  'InvocationContextEnricher' does not implement interface member 'ILogEventEnricher.Enrich(LogEvent, ILogEventPropertyFactory)'	

           // EnrichCtx(); // of course always says Enrich
           // 
            //Debug.WriteLine("In ICE Enrich: CMN=" + CallerMemberName);
            //Debug.WriteLine("In ICE Enrich: cMN=" + callerMemberName);

            logEvent.AddPropertyIfAbsent(new LogEventProperty(SourceContextPropertyName, new ScalarValue(SourceContext)));
            logEvent.AddPropertyIfAbsent(new LogEventProperty(CallerMemberNamePropertyName, new ScalarValue(CallerMemberName)));
            logEvent.AddPropertyIfAbsent(new LogEventProperty(CallerFilePathPropertyName, new ScalarValue(CallerFilePath)));
            logEvent.AddPropertyIfAbsent(new LogEventProperty(CallerLineNumberPropertyName, new ScalarValue(CallerLineNumber)));
        }

        // public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, [CallerMemberName] string callerMemberName = "")
        {
            // what we really want is to able to pass [CallerMemberName] etc. in here, but Serilog does not allow that method signature for Enrich:
            //Error CS0535  'InvocationContextEnricher' does not implement interface member 'ILogEventEnricher.Enrich(LogEvent, ILogEventPropertyFactory)'	

            // when I defined the reqd Enrich sig plus this sig this version of Enrich never gets called

            Debug.WriteLine("In ICE Enrich: CMN=" + CallerMemberName);
            Debug.WriteLine("In ICE Enrich: cMN=" + callerMemberName);
         //   EnrichCtx();

            logEvent.AddPropertyIfAbsent(new LogEventProperty(SourceContextPropertyName, new ScalarValue(SourceContext)));
            logEvent.AddPropertyIfAbsent(new LogEventProperty(CallerMemberNamePropertyName, new ScalarValue(CallerMemberName)));
            logEvent.AddPropertyIfAbsent(new LogEventProperty(CallerFilePathPropertyName, new ScalarValue(CallerFilePath)));
            logEvent.AddPropertyIfAbsent(new LogEventProperty(CallerLineNumberPropertyName, new ScalarValue(CallerLineNumber)));
        }
        public void EnrichCtx( [CallerMemberName] string callerMemberName = "")
        {
           // No use - is always enruch
            Debug.WriteLine("In ICE EnrichCtx: cMN=" + callerMemberName);

        }

    }
}
