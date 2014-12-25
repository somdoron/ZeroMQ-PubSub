using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PubSub.ZeroMQ;
using Utils;

namespace Subscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            var cases = new List<Tuple<string, int>>()
            {
                new Tuple<string, int>("MSFT", 1000),
                new Tuple<string, int>("FB", 3000),
                new Tuple<string, int>("AAPL", 2000)
            };

            var tasks = new List<Task>();
            foreach (var c in cases) {
                tasks.Add(AddTask(c.Item1, c.Item2));
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("Press enter to stop.");
            Console.ReadLine();
        }

        private static Task AddTask(string symbol, int N)
        {
            var task = Task.Run(() => {
                Console.WriteLine("Starting {0} worker for {1} updates.", symbol, N);
                using (var sub = new ZmqSubscriber("tcp://127.0.0.1:2345")) {
                    int count = 0;
                    var serializer = new BinarySerializer();
                    sub.Subscribe(symbol);
                    while (count < N) {
                        string key;
                        byte[] data;
                        if (sub.Receive(out key, out data, new TimeSpan(0, 0, 1))) {
                            Console.WriteLine("{0} {1}", key, serializer.Deserialize<Quote>(data));
                            count++;
                        }
                    }
                    //                    sub.Unsubscribe(symbol);
                }
            });
            return task;
        }
    }
}
