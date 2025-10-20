//using JAGLog.Models;
//using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JAGLog.Enrichers;

namespace JAGLog
{
    // 20251020
    // Note: This project needs an explicit reference to System.Configuration.ConfigurationManager in Nuget before adding Serilog.Sinks.MSSqlServer due to a bug somewhere
    // otherwise get Nuget failure.
    // Reason: Adding an explicit reference to the latest version of System.Configuration.ConfigurationManager (in this case, 9.0.10) can help resolve the version conflict and ensure that your project uses the correct version of the package.
    //  By doing so, you're effectively overriding the transitive dependency version specified by Serilog.Sinks.MSSqlServer and ensuring that the latest version of System.Configuration.ConfigurationManager is used throughout your project.
    public static class JAGLogExtensions

    {
        // The calling app
        internal static string appName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name; 
        
        // 20200601

        //internal static int HousekeepingDaysAgo = -60;

        // 20200426
        // The VM properties are bound to these fields which act as local copies
        // All data access to this class via view etc. is only via the public VM
      
        public static JAGLogViewModel JAGLogViewModel;

        internal static LoggingLevelSwitch levelSwitchOS; // init in ConfigureSerilog // works, so cannot be a ppty
        internal static LoggingLevelSwitch frameworkSwitchOS; // works, so cannot be a ppty
                                                            //  static string s1;

        static JAGLogExtensions()

        {
           
            JAGLogViewModel = new JAGLogViewModel();
        }


        public static void ConfigureSerilog(string ConnectionString)
        {
        
            string LogTemplate;
         
            // This logs internal Serilog errors to the debug immed window
            Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));

            //https://github.com/serilog/serilog-sinks-mssqlserver#columnoptions-object

            var columnOptions = new ColumnOptions
            {
                AdditionalColumns = new Collection<SqlColumn>
                {
                new SqlColumn
                    {ColumnName = "Application", DataType = SqlDbType.NVarChar, DataLength = 40},

                new SqlColumn
                    {ColumnName = "MethodName", DataType = SqlDbType.NVarChar, DataLength = 120},

                }
            };

            LogTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] <{MethodName}> {Message,-120:j}                 {NewLine}{Exception}";
            levelSwitchOS = new LoggingLevelSwitch(); //  works
            frameworkSwitchOS = new LoggingLevelSwitch(); //  works
            levelSwitchOS.MinimumLevel = LogEventLevel.Information;
            frameworkSwitchOS.MinimumLevel = LogEventLevel.Warning; // dflt is info

            Log.Logger = new LoggerConfiguration()
                     .MinimumLevel.ControlledBy(levelSwitchOS) // 20200408 - static obj works
                     .MinimumLevel.Override("System", frameworkSwitchOS)
                     .MinimumLevel.Override("Microsoft", frameworkSwitchOS)
                     .MinimumLevel.Override("Microsoft.EntityFrameworkCore", frameworkSwitchOS)

                .Enrich.WithProperty("Application", appName)
                .Enrich.FromLogContext()
                .Enrich.WithCaller() // 20200404 - uses reflection, so not ideal - maybe serilog will change one day  
                .WriteTo.Debug(outputTemplate: LogTemplate)// writes to immed window
                .WriteTo.Console(outputTemplate: LogTemplate) // does nothing in uwp
                .WriteTo.MSSqlServer(
                 connectionString: ConnectionString,
                 sinkOptions: new MSSqlServerSinkOptions { TableName = "SeriLog", AutoCreateSqlTable = false },
                 columnOptions: columnOptions
                ).CreateLogger();

            // https://github.com/serilog/serilog-sinks-mssqlserver#sinkoptions-object

            // by dflt logger has no context
            LogEventLevel lel = LogEventLevel.Fatal; // start with worse case
            GetSerilogMinimumLevel(out lel);
            Log.Information("Serilog logger has been created with MinimumLevel={ml}", lel);

            //SQLLogHousekeeping(ConnectionString); // call betting3 api one day to do this
         
        }

    

        public static void GetSerilogMinimumLevel(out LogEventLevel lel)
        // 20200408
        // https://stackoverflow.com/questions/49226533/how-to-get-current-serilog-minimumlevel
        {
            lel = LogEventLevel.Fatal;
            if (Log.IsEnabled(LogEventLevel.Verbose))
                lel = LogEventLevel.Verbose;
            else if (Log.IsEnabled(LogEventLevel.Debug))
                lel = LogEventLevel.Debug;
            else if (Log.IsEnabled(LogEventLevel.Information))
                lel = LogEventLevel.Information;
            else if (Log.IsEnabled(LogEventLevel.Warning))
                lel = LogEventLevel.Warning;
            else if (Log.IsEnabled(LogEventLevel.Error))
                lel = LogEventLevel.Error;
            else if (Log.IsEnabled(LogEventLevel.Fatal))
                lel = LogEventLevel.Fatal;
        }


        public static ILogger Here(this ILogger logger,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {

            // https://stackoverflow.com/questions/47576733/c-sharp-asp-net-core-serilog-add-class-name-and-method-to-log?rq=1

            // called like this:
            // Log.Logger.Here().Information("S");

            // https://stackoverflow.com/questions/17893827/c-sharp-how-to-get-type-name-of-a-callermember
            // to get type

            return logger
                .ForContext("MemberName", memberName)
                .ForContext("FilePath", sourceFilePath)
                .ForContext("LineNumber", sourceLineNumber);
        }
        public static ILogger Here<T>(this ILogger logger,
      [CallerMemberName] string memberName = "",
      [CallerFilePath] string sourceFilePath = "",
      [CallerLineNumber] int sourceLineNumber = 0)
        {

            // https://stackoverflow.com/questions/47576733/c-sharp-asp-net-core-serilog-add-class-name-and-method-to-log?rq=1

            // https://gist.github.com/litetex/b88fe0531e5acea82df1189643fb1f79
            // works  
            // may be more thread safe than Here above???
            InvocationContextEnricher enricher = new InvocationContextEnricher(typeof(T).FullName, memberName, sourceFilePath, sourceLineNumber);
            return Log.Logger.ForContext(enricher);

        }
    }
}
