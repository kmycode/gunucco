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

        Task OnConnectionAutomaticClosedAsync();
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
            this.Disconnect();
            await this.receiver.OnConnectionAutomaticClosedAsync();
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

            Queue<string> lineBuffer = new Queue<string>();
            string lastLine = "";
            var buf = new char[4096];
            string lastJson;

            using (var reader = new StreamReader(stream))
            {
                var bufferReader = new StreamReaderBuffer
                {
                    Stream = reader,
                };
                bufferReader.Start();

                while (this.isConnecting)
                {
                    try
                    {
                        string allText = lastLine + bufferReader.Read();

                        lastLine = "";
                        var lines = allText.Split('\n');
                        for (int i = 0; i < lines.Length; i++)
                        {
                            var l = lines[i];
                            if (!string.IsNullOrEmpty(l))
                            {
                                if (l.EndsWith("}") && (allText.EndsWith("\n") || i < lines.Length - 1))
                                {
                                    lineBuffer.Enqueue(l);
                                }
                                else
                                {
                                    lastLine = l;
                                }
                            }
                            System.Diagnostics.Debug.WriteLine(l);
                        }

                        while (lineBuffer.Count > 0)
                        {
                            var line = lineBuffer.Dequeue();
                            if (line.StartsWith("data: "))
                            {
                                var json = line.Substring("data: ".Length);
                                lastJson = json;

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
                    }
                    catch (Exception e)
                    {
                        if (this.isConnecting)
                        {
                            throw e;
                        }
                    }
                }

                bufferReader.Exit();
            }
        }

        private class StreamReaderBuffer
        {
            public StreamReader Stream { get; set; }
            private char[] buffer = new char[4096];
            private int index = 0;
            private bool isStop = false;
            private bool isExit = false;

            public string Read()
            {
                this.isStop = true;

                var buf = this.buffer;
                this.buffer = new char[4096];
                var text = new string(buf).Substring(0, this.index);

                this.index = 0;
                this.isStop = false;

                return text;
            }

            public void Start()
            {
                Task.Run(() =>
                {
                    try
                    {
                        while (!this.isExit)
                        {
                            while (this.isStop) ;
                            var c = (char)this.Stream.Read();
                            if (c != 0)
                            {
                                this.buffer[this.index++] = c;
                            }
                        }
                    }
                    catch
                    {
                        this.Exit();
                    }
                });
            }

            public void Exit()
            {
                this.isExit = true;
            }
        }

        public void Disconnect()
        {
            this.CheckDisposing();
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
