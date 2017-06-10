using Gunucco.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GunuccoSharp
{
    public interface IGunuccoStreaming<out T> : IDisposable
    {
        /// <summary>
        /// Start receiving loop
        /// </summary>
        /// <returns></returns>
        Task ReceiveLoopAsync();

        /// <summary>
        /// Disconnect streaming
        /// </summary>
        void Disconnect();
    }

    public interface IStreamingReceiver<in T>
    {
        /// <summary>
        /// Called when next item come
        /// </summary>
        /// <param name="item">The next item</param>
        void OnNext(T item);
    }

    class StreamingReceiver<T> : IGunuccoStreaming<T>
    {
        private readonly Func<Task<Stream>> streaming;
        private Stream stream;
        private readonly IStreamingReceiver<T> receiver;
        private bool isConnecting;
        private bool isDisposed;
        private readonly Timer reconnectTimer;

        internal StreamingReceiver(Func<Task<Stream>> streaming, IStreamingReceiver<T> receiver)
        {
            this.streaming = streaming;
            this.receiver = receiver;

            this.reconnectTimer = new Timer(this.ReconnectTimer_Callback, null, Timeout.Infinite, Timeout.Infinite);
        }

        private async void ReconnectTimer_Callback(object obj)
        {
            this.Disconnect(false);
        }

        public async Task ReceiveLoopAsync()
        {
            this.CheckDisposing();
            await this.StartConnectionAsync();
        }

        private async Task StartConnectionAsync()
        {
            var newStream = await this.streaming();

            // disconnect old stream
            this.Disconnect();

            // start timer
            this.reconnectTimer.Change(1000 * 75, Timeout.Infinite);

            this.stream = newStream;
            this.isConnecting = true;

            using (var reader = new StreamReader(stream))
            {
                while (this.isConnecting)
                {
                    try
                    {
                        var line = await reader.ReadLineAsync();
                        if (line.StartsWith("data: "))
                        {
                            var json = line.Substring("data: ".Length);

                            // check error
                            var err = JsonConvert.DeserializeObject<ApiMessage>(json);
                            if (err.StatusCode != default(int))
                            {
                                throw new GunuccoErrorException(err);
                            }

                            else
                            {
                                // deserialize object
                                var obj = JsonConvert.DeserializeObject<T>(json);

                                // send object
                                this.receiver.OnNext(obj);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (this.isConnecting)
                        {
                            throw e;
                        }
                    }
                }
            }
        }

        public void Disconnect()
        {
            this.Disconnect(true);
        }

        public void Disconnect(bool isCheck)
        {
            if (isCheck)
            {
                this.CheckDisposing();
            }
            this.isConnecting = false;
            this.reconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);

            if (this.stream == null)
            {
                return;
            }

            this.stream.Dispose();
            this.stream = null;
        }

        public void Dispose()
        {
            this.CheckDisposing();
            this.Disconnect();
            this.isDisposed = true;
        }

        private void CheckDisposing()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("StreamingReceiver");
            }
        }
    }
}
