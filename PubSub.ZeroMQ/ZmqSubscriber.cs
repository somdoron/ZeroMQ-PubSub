using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NetMQ;

namespace PubSub.ZeroMQ
{
    public class ZmqSubscriber : ISubscriber, IDisposable
    {
        private bool _disposed = false;
        private readonly int _creatorId;
        private readonly NetMQContext _context = null;
        private readonly NetMQSocket _socket = null;

        public ZmqSubscriber(string address)
        {
            // I hope, that CLR makes full memory barrier if it switchs native thread while execution.
            _creatorId = Thread.CurrentThread.ManagedThreadId;

            _context = NetMQContext.Create();
            _socket = _context.CreateSubscriberSocket();
            _socket.Connect(address);
        }

        public void SubscribeAll()
        {
            Subscribe("");
        }

        public void Subscribe(string key)
        {
            if (Thread.CurrentThread.ManagedThreadId != _creatorId)
                throw new InvalidOperationException("Object must not be transfered through thread border!");

            //var data = new List<byte> { 1 };
            //data.AddRange(Encoding.ASCII.GetBytes(key));
            //_socket.Send(data.ToArray());
            _socket.Subscribe(key);
        }

        public void Receive(out string key, out byte[] data)
        {
            if (Thread.CurrentThread.ManagedThreadId != _creatorId)
                throw new InvalidOperationException("Object must not be transfered through thread border!");

            var msg = new NetMQMessage();
            _socket.ReceiveMessage(msg);
            key = Encoding.ASCII.GetString(msg[0].ToByteArray());
            data = msg[1].ToByteArray();
        }

        public bool Receive(out string key, out byte[] data, TimeSpan timeout)
        {
            if (Thread.CurrentThread.ManagedThreadId != _creatorId)
                throw new InvalidOperationException("Object must not be transfered through thread border!");

            var msg = _socket.ReceiveMessage(timeout);
            if (msg != null) {
                key = Encoding.ASCII.GetString(msg[0].ToByteArray());
                data = msg[1].ToByteArray();
                return true;
            }

            key = null;
            data = null;
            return false;
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

        ~ZmqSubscriber()
        {
            Dispose(false);
        }
    }
}
