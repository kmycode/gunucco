using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace GunuccoSharp.CommandModels
{
    public class CommandBase
    {
        protected HttpClientBase Client { get; }

        internal CommandBase(HttpClientBase client)
        {
            this.Client = client;
        }
    }

    internal class CommandInfo
    {
        internal string Route { get; set; }

        internal Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();

        internal Collection<Tuple<string, Stream, string>> Media { get; set; } = new Collection<Tuple<string, Stream, string>>();

        internal HttpMethod Method { get; set; } = HttpMethod.Get;

        internal bool IsSendAsJson { get; set; } = false;
    }

    internal enum HttpMethod
    {
        Get,
        Post,
        PostMedia,
        Put,
        Patch,
        Delete,
    }
}
