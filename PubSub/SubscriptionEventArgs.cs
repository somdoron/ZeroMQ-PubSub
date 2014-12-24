using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubSub
{
    public class SubscriptionEventArgs : EventArgs
    {
        public bool IsSubscription { get; set; }
        public string Token { get; set; }
    }
}
