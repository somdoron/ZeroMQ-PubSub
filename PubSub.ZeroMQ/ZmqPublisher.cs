using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;

namespace PubSub.ZeroMQ
{
    public class ZmqPublisher : IPublisher, IDisposable
    {
        private bool _disposed = false;
        private readonly int _creatorId;
        private readonly NetMQContext _context = null;
        private readonly NetMQSocket _frontend = null;
        private readonly NetMQSocket _backend = null;
        private Task _task = null;
        private Poller _poller = null;
        private const string _endpoint = "inproc://backend";

        class ZmqPublishConnection : IPublishConnection
        {
            private bool _disposed = false;
            private readonly int _creatorId;
            private readonly NetMQContext _context = null;
            private readonly NetMQSocket _socket = null;

            public ZmqPublishConnection(NetMQContext context)
            {
                // I hope, that CLR makes full memory barrier if it switchs native thread while execution.
                _creatorId = Thread.CurrentThread.ManagedThreadId;

                _context = context;//NetMQContext.Create();
                _socket = _context.CreatePushSocket();
                _socket.Connect(_endpoint);
            }

            public void Publish(string key, byte[] data)
            {
                if (Thread.CurrentThread.ManagedThreadId != _creatorId)
                    throw new InvalidOperationException("Object must not be transfered through thread border!");

                _socket.SendMore(key);
                _socket.Send(data);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected void Dispose(bool disposing)
            {
                if (_disposed)
                    return;

                if (disposing) {

                    if (_socket != null) {
                        _socket.Close();
                    }
                }

                _disposed = true;
            }

            ~ZmqPublishConnection()
            {
                Dispose(false);
            }
        }

        public ZmqPublisher(string address)
        {
            _creatorId = Thread.CurrentThread.ManagedThreadId;

            _context = NetMQContext.Create();
            _frontend = _context.CreateXPublisherSocket();
            _frontend.Bind(address);
            _backend = _context.CreatePullSocket();
            _backend.Bind(_endpoint);

            _frontend.ReceiveReady += (s, a) =>
            {
                NetMQMessage msg = null;
                do {
                    msg = _frontend.ReceiveMessage(dontWait: true);
                    if (msg != null) {
                        var data = msg[0].ToByteArray();
                        bool isSubscription = data[0] == 1;
                        string token = data.Length > 1 ? Encoding.ASCII.GetString(data.Skip(1).ToArray()) : null;
                        OnSubscription(this, new SubscriptionEventArgs {IsSubscription = isSubscription, Token = token});
                    }
                } while (msg != null);
            };

            _backend.ReceiveReady += (s, a) => {
                var msg = _backend.ReceiveMessage(dontWait: true);
                if (msg != null) {
                    _frontend.SendMessage(msg);
                }
            };

            _poller = new Poller();
            _poller.AddSocket(_frontend);
            _poller.AddSocket(_backend);

            _task = Task.Factory.StartNew(_poller.Start);
        }

        public event EventHandler<SubscriptionEventArgs> OnSubscription;

        public IPublishConnection GetConnection()
        {
            var conn = new ZmqPublishConnection(_context);
            return conn;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _poller.Stop(true);
            _task.Wait();

            if (disposing) {
                if (_backend != null) {
                    _backend.Close();
                }

                if (_frontend != null) {
                    _frontend.Close();
                }

                if (_context != null) {
                    _context.Dispose();
                }
            }

            _disposed = true;
        }

        ~ZmqPublisher()
        {
            Dispose(false);
        }
    }
}
