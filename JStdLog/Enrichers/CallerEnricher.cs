using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JStdLog;
// 20200404
// https://gist.github.com/nblumhardt/0e1e22f50fe79de60ad257f77653c813
// https://github.com/serilog/serilog/wiki/Enrichment
// use to add caller name [ie method name] to logging
// uses reflection, so Invocation example would be better if I can work out how to configure it

// https://stackoverflow.com/questions/17893827/c-sharp-how-to-get-type-name-of-a-callermember

// https://kozmic.net/2008/10/13/slower-than-reflection-meet-stacktrace/
// it’s amazing how slow this class can be. I mean, Slow is redefined here as AlmostStopped.
// https://stackoverflow.com/questions/1348643/how-performant-is-stackframe
// Other than just writing the method name out manually in every log statement, you might look into compile-time aspect-oriented-programming libraries, and hook into a compile-time event to capture the method name, and a run-time aspect to log it. PostSharp is one such library. Might be overkill, though.
// StackTrace, StackFrame, MethodBase.GetCurrentMethod and passing a method as a delegate.
// Generate 100K frames: 1805ms
// About 20 microseconds per generated frame, on my machine. 
// log4net documentation, which calls explicitly calls out how slow generating stackframes (for inclusion in logs) is.

// If you dig deeper into StackFrameHelper you'll find that fNeedFileInfo is actually the performance killer - especially in debug mode. Try setting it false if performance is important. You won't get much useful information in release mode anyway. 

// https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.stackframe?view=netframework-4.8
// 

namespace JStdLog.Enrichers
{
    class CallerEnricher : ILogEventEnricher

    {
        // https://stackoverflow.com/questions/22598323/movenext-instead-of-actual-method-task-name
        // see this if I can be bothered converting async names
        // also has 
        // another example of a logging wrapper

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            // called for each call of Log.XX
        {

            // https://stackoverflow.com/questions/22598323/movenext-instead-of-actual-method-task-name


           // string s1;
            string callerSaved;
            //s1 = MethodBase.GetCurrentMethod().Name; // Enrich
            //                                         //s1 = MethodBase.GetMethodFromHandle().Name; // Enrich
            //bool GetAll = true; // this gets file info from StackFrame but dsnt exist for UWP and will live without it for now
            bool GetAll = false;
            //Debug.WriteLine(  "CallerEnricher.Enrich"+s1);
            string PropName = "MethodName"; // was Caller, but this is the SQL column name
            var skip = 3;
            //Debug.WriteLineIf((skip == 3), "CallerEnricher.Enrich");

            // 20200415
            // https://gist.github.com/nblumhardt/0e1e22f50fe79de60ad257f77653c813
            // would it be more efficient to cache the result of typeof(Log) so that it's not being fetched with each iteration?

            Assembly SerilogAssembly = typeof(Log).Assembly;

            //s1 = LogExtensions.appName;
            //Debug.WriteLine("appName=" + s1);

            while (true)

            {

                var stack = new StackFrame(skip,GetAll);

                if (!stack.HasMethod())
                {
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue("<unknown method>")));
                    return;
                }

                var method = stack.GetMethod();

                if (method.DeclaringType.Assembly != SerilogAssembly)
                {
                    // I don't really care about the params for now at least
                    // or the full name really - don't want namespace
                    var caller2 = $"{method.DeclaringType.Name}.{method.Name}";
                    //                    var caller2 = $"{method.DeclaringType.FullName}.{method.Name}";
                    //                    var caller2 = $"{method.DeclaringType.FullName}.{method.Name}({string.Join(", ", method.GetParameters().Select(pi => pi.ParameterType.FullName))})";

                    callerSaved = caller2;

                    // basic async tidy = just use stuff in brackets
                    // <Button_Click>d__13.MoveNext

                    // https://stackoverflow.com/questions/19205107/operator-cannot-be-applied-to-operands-of-type-char-and-string
                    // 'a' is char
                    // "a" is string!

                    if (callerSaved.StartsWith("<"))
                    {
                        //s1 = callerSaved;
                        //Debug.WriteLine("\r" + "callerSaved before =" + s1);

                        //var x0 = callerSaved.Take(6); // works
                        //var xx = callerSaved.TakeWhile(f => f.Equals('<'));
                        //var xx2 = callerSaved.TakeWhile(f => (f == '<'));
                         var xx3 = callerSaved.Skip(1).TakeWhile(f => (f != '>'));
                        //callerSaved = callerSaved.Skip(1).TakeWhile(f => (f != '>')).ToString();

                        // https://stackoverflow.com/questions/6083687/how-to-take-on-a-string-and-get-a-string-at-the-end

                        caller2 = new string(xx3.ToArray()) ;
                        //Debug.WriteLine("callerSaved after 1=" + s1);

                        // callerSaved = callerSaved.TakeWhile(f => !f.Equals(">")).ToString();

                        //s1 = callerSaved;
                        //Debug.WriteLine("callerSaved after 2=" + s1);

                    }
                    // 20200415
                    // already have appln name so omit it from the methodname
                    // 20200419
                    // using just name above so do not need this now
                    //caller2 = caller2.Replace(LogExtensions.appName + ".", "");


                    // var caller = $"{method.Name}({string.Join(", ", method.GetParameters().Select(pi => pi.ParameterType.FullName))})";
                    //s1 = typeof(Log).Assembly.ToString();
                    //Debug.WriteLine("tol=" + s1);
                    //s1 = stack.GetFileLineNumber().ToString() + " " + stack.GetFileName().ToString();
                    //s1 = caller;
                    //Debug.WriteLine("caller =" + s1);
                    //s1 = caller + " / " + stack.ToString();
                    //Debug.WriteLine("caller/stack=" + s1);
                    //                    Debug.WriteLine("line/name=" + s1);
                    logEvent.AddPropertyIfAbsent(new LogEventProperty(PropName, new ScalarValue(caller2)));
//                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller2)));
                    return;
                }

                skip++;

            }

        }

    }
}
