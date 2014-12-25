using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PubSub.ZeroMQ;
using Utils;

namespace Publisher
{
    class Program
    {
        static Dictionary<string, Tuple<Task, CancellationTokenSource>> workers = new Dictionary<string, Tuple<Task, CancellationTokenSource>>();
        static List<string> symbols = new List<string>();

        private static void FillSymbols()
        {
            symbols.Add("MSFT");
            symbols.Add("AAPL");
            symbols.Add("GOOG");
            symbols.Add("FB");
        }

        private static void StartAll(ZmqPublisher pub)
        {
            foreach (var smb in symbols) {
                AddWorker(pub, smb);
            }
        }

        private static void StopAll()
        {
            foreach (var smb in symbols) {
                StopWorker(smb);
            }
        }

        private static void AddWorker(ZmqPublisher pub, string symbol)
        {
            if (workers.ContainsKey(symbol))
                return;

            Console.WriteLine("Starting {0} worker.", symbol);

            var cts = new CancellationTokenSource();

            var task = Task.Run(() =>
            {
                var serializer = new BinarySerializer();
                using (var conn = pub.GetConnection()) {
                    while (!cts.IsCancellationRequested) {
                        conn.Publish(symbol, serializer.Serialize(new Quote { Ask = 100, Bid = 99, Last = 99 }));
                        Thread.Sleep(100);
                    }
                }
            }, cts.Token);

            workers.Add(symbol, new Tuple<Task, CancellationTokenSource>(task, cts));
        }

        private static void StopWorker(string symbol)
        {
            if (!workers.ContainsKey(symbol))
                return;

            Console.WriteLine("Cancelling {0} worker.", symbol);

            var cts = workers[symbol].Item2;

            cts.Cancel();

            workers.Remove(symbol);
        }

        static void Main(string[] args)
        {
            FillSymbols();

            var pub = new ZmqPublisher("tcp://127.0.0.1:2345");
            pub.Subscription += (sender, eventArgs) =>
            {
                if (eventArgs.IsSubscription) {
                    if (eventArgs.Token == null) {
                        StartAll(pub);
                    } else {
                        AddWorker(pub, eventArgs.Token);
                    }
                } else {
                    if (eventArgs.Token == null) {
                        StopAll();
                    } else {
                        StopWorker(eventArgs.Token);
                    }
                }
            };

            Console.WriteLine("Press enter to stop.");
            Console.ReadLine();

            pub.Dispose();
        }
    }
}
