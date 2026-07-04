using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HeadNonSub.Settings;
using Microsoft.Extensions.Options;

namespace HeadNonSub {

    public static class Http {

        /// <summary>
        /// Static http client.
        /// </summary>
        public static readonly HttpClient Client = new HttpClient();

        /// <summary>
        /// Optional IOptions configuration provider (for dependency injection).
        /// </summary>
        private static IOptions<Configuration> _ConfigurationOptions;

        /// <summary>
        /// Set the configuration options (called during startup from DI container).
        /// </summary>
        public static void SetConfigurationOptions(IOptions<Configuration> configurationOptions) {
            _ConfigurationOptions = configurationOptions;
        }

        /// <summary>
        /// Get the current configuration, preferring injected options over static SettingsManager.
        /// </summary>
        private static Configuration GetConfiguration() {
            if (_ConfigurationOptions != null) {
                return _ConfigurationOptions.Value;
            }
            return SettingsManager.Configuration;
        }

        /// <summary>
        /// Send a http request.
        /// </summary>
        /// <param name="url">Url to download.</param>
        /// <param name="headers">Headers for the request. (name, value)</param>
        /// <param name="parameters">Parameters for the request. (key, value)</param>
        /// <param name="method">Http method for the request.</param>
        /// <returns>Response content as a string. This may be empty based on <see cref="Method"/>.</returns>
        public static async Task<string> SendRequestAsync(string url, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null, Method method = Method.Get) {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method.ToString()), url);

            if (headers != null) {
                foreach (KeyValuePair<string, string> header in headers) {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (parameters != null) {
                request.Content = new FormUrlEncodedContent(parameters);
            }

            using HttpResponseMessage response = await Client.SendAsync(request);
            if (response.IsSuccessStatusCode) {
                using HttpContent content = response.Content;
                return await content.ReadAsStringAsync();
            } else {
                throw new HttpRequestException($"There was an error; ({(int)response.StatusCode}) {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// Download data as a stream.
        /// </summary>
        /// <param name="url">Url to download.</param>
        /// <param name="headers">Headers for the request. (name, value)</param>
        /// <param name="method">Http method for the request.</param>
        /// <returns>The requested data as a <see cref="MemoryStream"/></returns>
        public static async Task<MemoryStream> GetStreamAsync(string url, Dictionary<string, string> headers = null, Method method = Method.Get) {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method.ToString()), url);

            if (headers != null) {
                foreach (KeyValuePair<string, string> header in headers) {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            using HttpResponseMessage response = await Client.SendAsync(request);
            if (response.IsSuccessStatusCode) {
                using HttpContent content = response.Content;
                using Stream stream = await content.ReadAsStreamAsync();

                MemoryStream copyStream = new MemoryStream(256);
                stream.CopyTo(copyStream);
                copyStream.Seek(0, SeekOrigin.Begin);

                return copyStream;
            } else {
                throw new HttpRequestException($"There was an error; ({(int)response.StatusCode}) {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// Shorten a url.
        /// </summary>
        /// <param name="url">Url to shorten.</param>
        public static async Task<string> ShortenUrl(string url) {
            Task<string> shortenRequest = SendRequestAsync(Constants.UrlShortener, parameters: new Dictionary<string, string> { { "key", GetConfiguration().UrlShortenerKey }, { "url", url } }, method: Method.Post);
            string shortenedUrl = await shortenRequest;

            if (shortenRequest.IsCompletedSuccessfully) {
                return shortenedUrl;
            } else {
                LoggingManager.Log.Error(shortenRequest.Exception);
                return string.Empty;
            }
        }

        /// <summary>
        /// Deserializes a json string to a type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">Json to deserialize.</param>
        public static T DeserializeJson<T>(string json) {
            try {
                JsonSerializerOptions options = new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                return JsonSerializer.Deserialize<T>(json, options);
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return default;
            }
        }

        /// <summary>
        /// Http request method.
        /// </summary>
        public enum Method {
            Delete,
            Get,
            Head,
            Options,
            Patch,
            Post,
            Put
        }

    }

}

