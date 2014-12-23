using System;
using System.Threading;
using PubSub.ZeroMQ;
using Utils;

namespace Subscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            bool recv = true;
            var worker = new Thread(new ThreadStart(() => {
                var serializer = new BinarySerializer();
                using (var sub = new ZmqSubscriber("tcp://127.0.0.1:2345")) {
                    while (recv) {
                        string key;
                        byte[] data;
                        sub.Receive(out key, out data);;
                        Console.WriteLine("{0} {1}", key, serializer.Deserialize<Quote>(data));
                    }
                }
            }));
            worker.Start();
            Console.WriteLine("Press enter to stop.");
            Console.ReadLine();
            recv = false;
            worker.Join();
        }
    }
}
