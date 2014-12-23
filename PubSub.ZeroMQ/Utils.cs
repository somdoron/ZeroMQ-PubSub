using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;

namespace PubSub.ZeroMQ
{
    class Utils
    {
        public static string MsgToString(NetMQMessage msg)
        {
            string res = String.Empty;

            res += String.Format("Number of frames = {0}", msg.FrameCount);
            for (int i = 0; i < msg.FrameCount; i++) {
                res += String.Format("\nFrame = {0} Size = {1}", i, msg[i].MessageSize);
                res += String.Format("\n{0}", Encoding.ASCII.GetString(msg[i].ToByteArray()));
            }

            return res;
        }
    }
}
