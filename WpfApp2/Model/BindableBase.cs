using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2.Model
{
    public class BindableBase : INotifyPropertyChanged
    {
        ///
        /// Multicast event for property change notifications.
        ///
        public event PropertyChangedEventHandler PropertyChanged;

        ///
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        ///
        ///Type of the property.
        ///Reference to a property with both getter and setter.
        ///Desired value for the property.
        ///Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.
        ///True if the value was changed, false if the existing value matched the
        /// desired value.
        protected bool SetProperty(ref List< MessageItem> storage, List<MessageItem> value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        ///
        /// Notifies listeners that a property value has changed.
        ///
        ///Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support .
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
}
