using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubSub
{
    public interface ISubscriber
    {
        void Receive(out string key, out byte[] data);
    }
}
