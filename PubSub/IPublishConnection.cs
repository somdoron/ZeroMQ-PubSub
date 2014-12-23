using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubSub
{
    public interface IPublishConnection : IDisposable
    {
        void Publish(string key, byte[] data);
    }
}
