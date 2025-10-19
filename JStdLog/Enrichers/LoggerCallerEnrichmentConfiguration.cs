using Serilog;
using Serilog.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// 20200404
// https://gist.github.com/nblumhardt/0e1e22f50fe79de60ad257f77653c813
namespace JStdLog.Enrichers
{
    static class LoggerCallerEnrichmentConfiguration

    {

        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/how-to-implement-and-call-a-custom-extension-method

        // extension method

        // extends the LoggerEnrichmentConfiguration class
        // returns a LoggerConfiguration object

        // all the methods in LoggerEnrichmentConfiguration return a LoggerConfiguration object

        // used as follows:

        //   Log.Logger = new LoggerConfiguration().Enrich.WithCaller() 


        // 
        public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration)

        {

            // variations are:

            // public LoggerConfiguration With(params ILogEventEnricher[] enrichers);
            // public LoggerConfiguration With<TEnricher>() where TEnricher : ILogEventEnricher, new();

            // http://dotnetpattern.com/csharp-generic-methods
            // Generic Methods Constraints
            // Constraints are validations on type arguments which type of parameter we can pass in generic method. Generic class and Generic methods follow the same constraints.
            // There are six types of constraints.
            // where T : <interface> –  Type argument must implement from <interface> interface.
            // where T : new() – Type argument must have a public parameterless constructor.


            // so, With is a generic method in 
            //  public class LoggerEnrichmentConfiguration
            // that returns an object of type LoggerConfiguration

            // the With<T> form of the method used here
            // public LoggerConfiguration With<TEnricher>() where TEnricher : ILogEventEnricher, new();
            // is defined so that T [ie CallerEnricher] must implement from ILogEventEnricher and have a public parameterless constructor.

            // the first form
            // https://stackoverflow.com/questions/7580277/why-use-the-params-keyword
            // params allows you to use a shortcut when calling the method. where the args are a 1D array
            // static public int addTwoEach(params int[] args)
            // addTwoEach(new int[] { 1, 2, 3, 4, 5 });
            // vs
            // addTwoEach(1, 2, 3, 4, 5);
            // addTwoEach(1);
            // addTwoEach();

            // means that one can pass an array of enrichers that implement ILogEventEnricher or just one or none

            //LoggerEnrichmentConfiguration lec = new LoggerEnrichmentConfiguration(CallerEnricher);
            //_ = lec.With<CallerEnricher>();

            // add 2 enrichers
            // adds them ok but InvocationContextEnricher always reports logging from this line so no good
            // happens bcos the InvocationContextEnricher ctor sets the values and that is only called here

            // The CallerEnricher works
            //            LoggerConfiguration lcx = enrichmentConfiguration.With(new InvocationContextEnricher(), new CallerEnricher());
            // same as enrichmentConfiguration.With<CallerEnricher>();
            // because it doesn't have a ctor - its Enrich method examines the stack anew each time

            LoggerConfiguration lcx = enrichmentConfiguration.With(new CallerEnricher());

            //LoggerConfiguration lcx = enrichmentConfiguration.With<CallerEnricher>();
            return lcx;

            //return enrichmentConfiguration.With<CallerEnricher>();

        }


        //public static LoggerConfiguration ZWith<LoggerConfiguration>(params int[] args) where LoggerConfiguration : ILogEventEnricher
        //{   LoggerConfiguration lcx = enrichmentConfiguration.With<CallerEnricher>();
        //    return lcx;}


    // https://gist.github.com/litetex/b88fe0531e5acea82df1189643fb1f79
    // gave up on this as it doesn't seem to map onto the std cfg for enrichers
    // how to configure it?? example doesn't say, but ok as the enricher seems to need to be explicitly called in Here<T>

    // just using WithCaller  

    // https://www.ctrlaltdan.com/2018/08/14/custom-serilog-enrichers/
    // basic no params example as per Caller

    // https://github.com/serilog/serilog/wiki/Configuration-Basics

    //public static LoggerConfiguration WithInvocationContext(this LoggerEnrichmentConfiguration enrichmentConfiguration)

    //{

    //    return enrichmentConfiguration.With(InvocationContextEnricher(typeof(T).FullName, callerMemberName, callerFilePath, callerLineNumber));

    //}
}
}
