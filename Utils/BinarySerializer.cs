using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Utils
{
    public class BinarySerializer
    {
        private JavaScriptSerializer _serializer;

        public BinarySerializer()
        {
            _serializer = new JavaScriptSerializer();
        }

        public T Deserialize<T>(byte[] input)
        {
            return _serializer.Deserialize<T>(Encoding.ASCII.GetString(input));
        }

        public byte[] Serialize(object obj)
        {
            return Encoding.ASCII.GetBytes(_serializer.Serialize(obj));
        }
    }
}
