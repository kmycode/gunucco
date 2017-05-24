using Gunucco.Entities;
using GunuccoSharp.CommandModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GunuccoSharp
{
    public class HttpClientBase
    {
        /// <summary>
        /// Service path without last '/'
        /// </summary>
        /// <example>
        /// client.ServicePath = "http://gunucco.net";
        /// </example>
        public string ServicePath { get; set; }

        /// <summary>
        /// Authentication token
        /// </summary>
        public AuthenticationToken AuthToken { get; set; }

        /// <summary>
        /// allow self-signed SSL certificate
        /// </summary>
        [Obsolete("This property is for debug or test. NOT for production.")]
        public bool IsAllowInvalidSSLCert { get; set; } = false;

        private void AddAuthenticationHeader(HttpClient client)
        {
            if (this.AuthToken != null)
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + this.AuthToken.AccessToken);
            }
        }

        internal async Task<T> Command<T>(CommandInfo command)
        {
            var route = command.Route;
            var data = command.Data;
            var isJson = command.IsSendAsJson;
            switch (command.Method)
            {
                case CommandModels.HttpMethod.Get:
                    return await this.Get<T>(route, data);
                case CommandModels.HttpMethod.Post:
                    return await this.Post<T>(route, data, isJson);
                case CommandModels.HttpMethod.PostMedia:
                    return await this.PostMedia<T>(route, data, command.Media);
                case CommandModels.HttpMethod.Put:
                    return await this.Put<T>(route, data, isJson);
                case CommandModels.HttpMethod.Delete:
                    return await this.Delete<T>(route, data, isJson);
                case CommandModels.HttpMethod.Patch:
                    return await this.Patch<T>(route, data, isJson);
            }

            throw new Exception();
        }

        protected async Task<string> Get(string route, IEnumerable<KeyValuePair<string, string>> data = null)
        {
            var rp = await this.RequestHttpAsync(route, async (client, url) =>
            {
                if (data != null)
                {
                    var querystring = "?" + string.Join("&", data.Select(kvp => kvp.Key + "=" + kvp.Value));
                    url += querystring;
                }

                return await client.GetAsync(url);
            });

            return rp;
        }

        protected async Task<T> Get<T>(string route, IEnumerable<KeyValuePair<string, string>> data = null)
        {
            var json = await this.Get(route, data);
            return this.JsonDeserialize<T>(json);
        }

        private async Task<string> Post(string route, IEnumerable<KeyValuePair<string, string>> data = null, string methodName = "POST", bool isSendAsJson = false)
        {
            var rp = await this.RequestHttpAsync(route, async (client, url) =>
            {
                var method = new System.Net.Http.HttpMethod(methodName);
                
                var content = !isSendAsJson ? (HttpContent)new FormUrlEncodedContent(data ?? Enumerable.Empty<KeyValuePair<string, string>>()) :
                                              (HttpContent)new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var message = new HttpRequestMessage
                {
                    Method = method,
                    RequestUri = new Uri(url),
                    Content = content,
                };

                return await client.SendAsync(message);
            });

            return rp;
        }

        private async Task<string> PostMedia(string route, IEnumerable<KeyValuePair<string, string>> data = null, IEnumerable<Tuple<string, Stream, string>> media = null, string methodName = "POST")
        {
            var rp = await this.RequestHttpAsync(route, async (client, url) =>
            {
                var method = new System.Net.Http.HttpMethod(methodName);

                var content = new MultipartFormDataContent();

                foreach (var tuple in media)
                {
                    content.Add(new StreamContent(tuple.Item2), tuple.Item1, tuple.Item3);
                }
                if (data != null)
                {
                    foreach (var pair in data)
                    {
                        content.Add(new StringContent(pair.Value), pair.Key);
                    }
                }

                var message = new HttpRequestMessage
                {
                    Method = method,
                    RequestUri = new Uri(url),
                    Content = content,
                };

                return await client.SendAsync(message);
            });

            return rp;
        }

        private async Task<T> PostMedia<T>(string route, IEnumerable<KeyValuePair<string, string>> data = null, IEnumerable<Tuple<string, Stream, string>> media = null)
        {
            var json = await this.PostMedia(route, data, media);
            return this.JsonDeserialize<T>(json);
        }

        protected async Task<T> Post<T>(string route, IEnumerable<KeyValuePair<string, string>> data = null, bool isSendAsJson = false)
        {
            var json = await this.Post(route, data, "POST", isSendAsJson);
            return this.JsonDeserialize<T>(json);
        }

        protected async Task<T> Patch<T>(string route, IEnumerable<KeyValuePair<string, string>> data = null, bool isSendAsJson = false)
        {
            var json = await this.Post(route, data, "PATCH", isSendAsJson);
            return this.JsonDeserialize<T>(json);
        }

        protected async Task<T> Put<T>(string route, IEnumerable<KeyValuePair<string, string>> data = null, bool isSendAsJson = false)
        {
            var json = await this.Post(route, data, "PUT", isSendAsJson);
            return this.JsonDeserialize<T>(json);
        }

        protected async Task<T> Delete<T>(string route, IEnumerable<KeyValuePair<string, string>> data = null, bool isSendAsJson = false)
        {
            var json = await this.Post(route, data, "DELETE", isSendAsJson);
            return this.JsonDeserialize<T>(json);
        }

        private async Task<string> RequestHttpAsync(string route, Func<HttpClient, string, Task<HttpResponseMessage>> action)
        {
            HttpClient client;
            if (this.IsAllowInvalidSSLCert)
            {
                // allow self-signed SSL certificate
                var httpClientHandler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = delegate { return true; },
                };
                client = new HttpClient(httpClientHandler);
            }
            else
            {
                client = new HttpClient();
            }
            string url = this.ServicePath + "/api/v1/" + route;
            this.AddAuthenticationHeader(client);

            HttpResponseMessage response = null;

            try
            {
                response = await action(client, url);
            }
            catch (WebException e)
            {
                try
                {
                    var stream = e.Response.GetResponseStream();
                    var result = new byte[stream.Length];
                    stream.Seek(0, SeekOrigin.Begin);
                    await stream.ReadAsync(result, 0, (int)stream.Length);
                    var content = Encoding.UTF8.GetString(result);
                    ApiMessage error = null;
                    try
                    {
                        error = this.JsonDeserialize<ApiMessage>(content);
                    }
                    catch { }
                    if (!string.IsNullOrEmpty(error?.Message))
                    {
                        throw new GunuccoErrorException("Api failed. message: '" + error.Message + "'", error, (int?)((e.Response as HttpWebResponse)?.StatusCode) ?? -1);
                    }
                    else
                    {
                        throw new GunuccoException("http connection failed.", e);
                    }
                }
                catch (Exception ex)
                {
                    throw new GunuccoException("http connection and get error detail failed.", ex);
                }
            }
            catch (Exception e)
            {
                throw new GunuccoException("http connection failed.", e);
            }

            var contents = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(contents))
            {
                throw new GunuccoException("server returns empty result.");
            }

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotModified && contents[0] == '{')
            {
                var error = this.JsonDeserialize<ApiMessage>(contents);
                if (!string.IsNullOrEmpty(error?.Message))
                {
                    throw new GunuccoErrorException("Api failed. message: '" + error.Message + "'", error, (int)response.StatusCode);
                }
            }

            return contents;
        }

        private T JsonDeserialize<T>(string json)
        {
            T jobj;

            try
            {
                jobj = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                throw new GunuccoException("invalid http response.", e);
            }

            return jobj;
        }
    }
}
