using System;
using System.Threading;
using NetMQ;

namespace PubSub.ZeroMQ
{
    public class ZmqPublisher : IPublisher, IDisposable
    {
        private bool _disposed = false;
        private readonly int _creatorId;
        private readonly NetMQContext _context = null;
        private readonly NetMQSocket _socket = null;

        public ZmqPublisher(string address)
        {
            // I hope, that CLR makes full memory barrier if it switchs native thread while execution.
            _creatorId = Thread.CurrentThread.ManagedThreadId;

            _context = NetMQContext.Create();
            _socket = _context.CreateXPublisherSocket();
            _socket.Bind(address);
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
