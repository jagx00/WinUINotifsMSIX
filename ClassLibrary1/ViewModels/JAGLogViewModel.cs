using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
//using static UWPExampleCS.Constants.Enums;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
//using Mvvm;
//using UWPExampleCS.Extensions;
using JAGLog;

namespace JAGLog
{


    // partials
    // https://stackoverflow.com/questions/5229645/including-partial-views-when-applying-the-mode-view-viewmodel-design-pattern
    // https://stackoverflow.com/questions/3448019/calling-or-sending-values-from-mainviewmodel-to-other-viewmodels-mvvm-mvvm-light?rq=1


    public class JAGLogViewModel : BindableBase, INotifyPropertyChanged
//    public class jStdLogViewModel : ViewModelBase, INotifyPropertyChanged
    {
        string s1;
        public JAGLogViewModel()
        {
           
            s1 = "In jStdLogViewModel ctor";
            
            Log.Information(s1); // works 

                       // test
            //App.levelSwitchOS.MinimumLevel = LogEventLevel.Verbose; // static obj
            //s1 = "in main vm ctor after setting verbose";
            //Log.Debug(s1); //  

            //  Log.Logger.Debug(s1);
        }

       
        // Serilog section

        // https://devblogs.microsoft.com/dotnet/sharing-code-across-platforms/
        // https://stackoverflow.com/questions/977652/mvvm-inheritance-with-view-models
        // https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.usercontrol?redirectedfrom=MSDN&view=netframework-4.8
        // https://stackoverflow.com/questions/27309302/how-do-i-implement-the-same-search-functionality-for-multiple-view-models?noredirect=1&lq=1
        // inherit your ViewModel classes directly from a common abstract base class.
        // https://stackoverflow.com/questions/2045904/dependency-inject-di-friendly-library/2047657#2047657// https://stackoverflow.com/questions/2045904/dependency-inject-di-friendly-library/2047657#2047657
        // https://stackoverflow.com/questions/27220714/ramifications-of-using-dbcontext-with-dependency-injection
        // DI Container can be compared to a dictionary where key is interface and the value is a cooked object. 
        // Once its setup, you can anytime ask for any object by using the right key through out your application.


        // 20200405
        // https://stackoverflow.com/questions/652673/enum-boxing-and-equality
        // 
        public LogEventLevel levelSwitchMinimumLevel
        {
            get => JAGLogExtensions.levelSwitchOS.MinimumLevel;
            set
            {
                if (!value.Equals(JAGLogExtensions.levelSwitchOS.MinimumLevel))
                {
                    JAGLogExtensions.levelSwitchOS.MinimumLevel = value;
                    OnPropertyChanged();
                }
            }
        }
        public LogEventLevel frameworkSwitchMinimumLevel
        {
            get => JAGLogExtensions.frameworkSwitchOS.MinimumLevel;
            set
            {
                if (!JAGLogExtensions.frameworkSwitchOS.MinimumLevel.Equals(value))
                {
                    JAGLogExtensions.frameworkSwitchOS.MinimumLevel = value;
                    OnPropertyChanged();
                }
            }
        }

   
    }
}
