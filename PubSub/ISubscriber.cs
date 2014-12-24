using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubSub
{
    public interface ISubscriber
    {
        void SubscribeAll();
        void Subscribe(string key);

        // Unsubscribe

        void Receive(out string key, out byte[] data);
        bool Receive(out string key, out byte[] data, TimeSpan timeout);
    }
}
