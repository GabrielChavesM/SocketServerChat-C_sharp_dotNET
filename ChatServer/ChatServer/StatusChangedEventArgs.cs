using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer {
    
    // Treats the arguments for the event StatusChanged
    public class StatusChangedEventArgs : EventArgs {
        
        // Message describing the event
        private string EventMsg;

        // Property that returns and defines an event message
        public string EventMessage
        {
            get { return EventMsg; }
            set { EventMsg = value; }
        }

        // Constructor to define the event message
        public StatusChangedEventArgs(string strEventMsg)
        {
            EventMsg = strEventMsg;
        }
    }
}
