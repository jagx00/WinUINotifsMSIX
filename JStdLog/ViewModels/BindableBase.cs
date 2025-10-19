using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
//using Windows.UI.Xaml.Data;
//   // https://xamlbrewer.wordpress.com/2016/01/31/binding-a-slider-to-an-enumeration-in-uwp/

namespace JStdLog
{
    /// <summary>
    /// Implementation of <see cref="INotifyPropertyChanged"/> to simplify models.
    /// </summary>
    /// 
    // An abstract class is a class in C# that can’t be instantiated. An abstract class can have both abstract methods and Concrete method. Concrete methods are those methods that have methods with their implementation.
    // https://www.guru99.com/c-sharp-abstract-class.html
    // An abstract class should have a minimum of one abstract method. ?? 
    // Abstract class acts as a base class and is designed to be inherited by subclasses that either implement or either override its method. 

    public abstract class BindableBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Multicast event for property change notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        //string s1;

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        //protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        //{
        //    if (object.Equals(storage, value)) return false;
        //    // should be called SetField as cannot set a ppty this way!
        //    // storage should be called field or destination too?
        //    storage = value;
        //    this.OnPropertyChanged(propertyName);
        //    return true;
        //}
        // 20200408
        // for when setting ppty
        //   set { SetProperty(ref importance, value); } 
        //  set { SetProperty(ref _ClickCount, value); } = setting a field
        //  set { SetProperty2(storage => App.gDiagLevel = storage, value); } works
        //protected bool SetProperty2<T>(Action<T> storage, T value, [CallerMemberName] String propertyName = null)
        //{
        //    // works but pointless as SetProperty will always return true so may as well just execute App.gDiagLevel = storage directly 
        //    // rather than calling this function to do the same thing
        //    T value2 = default;
        //    bool result = SetProperty(ref value2,value);
        //    if (result) storage(value2);
        //    return result;

        //    //if (object.Equals(storage, value)) return false;

        //    //storage = value;
        //    //this.OnPropertyChanged(propertyName);
        //    //return true;
        //}
        //not wrkg yet = storage is input only and contains a value not the actual object I want to update
        // https://stackoverflow.com/questions/1402803/passing-properties-by-reference-in-c-sharp
        // in detail
        // conclusion = do a variation of the LINQ Expression vern or Accessor of SO above
        // but hardly worth it and may even take more work
//        protected bool SetProperty3<T>(string storage, T value, [CallerMemberName] String propertyName = null)
//        {
//            //Action<T> storageA = storagep => storage = storagep;
//            // https://stackoverflow.com/questions/3059420/accessing-properties-through-generic-type-parameter
//var xx = typeof(T).GetProperty(storage);
//            s1 = typeof(T).GetProperty(storage).ToString();
//            //
//            bool result = true;
//            //T value2 = default;
//            //bool result = SetProperty(ref value2, value);
//            //if (result) storageA(value2);
//            return result;

//            //if (object.Equals(storage, value)) return false;

//            //storage = value;
//            //this.OnPropertyChanged(propertyName);
//            //return true;
//        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
