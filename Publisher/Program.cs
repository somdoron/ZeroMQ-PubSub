using System;
using System.Threading;
using PubSub.ZeroMQ;
using Utils;

namespace Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            bool send = true;
            var worker = new Thread(new ThreadStart(() => {
                var serializer = new BinarySerializer();
                using (var pub = new ZmqPublisher("tcp://127.0.0.1:2345")) {
                    while (send) {
                        pub.Publish("MSFT", serializer.Serialize(new Quote { Ask = 100, Bid = 99, Last = 99 }));
                        Thread.Sleep(100);
                    }
                }
            }));
            worker.Start();
            Console.WriteLine("Press enter to stop.");
            Console.ReadLine();
            send = false;
            worker.Join();
        }
    }
}
