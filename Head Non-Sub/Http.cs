using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HeadNonSub.Settings;

namespace HeadNonSub {

    public static class Http {

        /// <summary>
        /// Static http client.
        /// </summary>
        public static readonly HttpClient Client = new HttpClient();

        /// <summary>
        /// Send a file to be stored on the CDN.
        /// </summary>
        /// <param name="byteArray">File to upload.</param>
        /// <param name="fileExtension">Extension of the file to upload.</param>
        /// <returns>Url of the file.</returns>
        public static async Task<string> UploadFileToCDNAsync(byte[] byteArray, string fileExtension) {
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            using (StreamContent streamContent = new StreamContent(new MemoryStream(byteArray)))
            using (ByteArrayContent fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync())) {
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                form.Add(new StringContent(SettingsManager.Configuration.CDNKey), "key");
                form.Add(fileContent, "file", $"{Guid.NewGuid()}.{fileExtension}");

                using (HttpResponseMessage response = await Client.PostAsync(Constants.CDNAPI, form)) {
                    if (response.IsSuccessStatusCode) {
                        using (HttpContent content = response.Content) {
                            string fileName = await content.ReadAsStringAsync();

                            if (!string.IsNullOrWhiteSpace(fileName)) {
                                return $"{Constants.CDNUploads}{fileName}";
                            } else {
                                throw new ArgumentException("File name is empty after upload", nameof(fileName));
                            }
                        }
                    } else {
                        throw new HttpRequestException($"There was an error; ({(int)response.StatusCode}) {response.ReasonPhrase}");
                    }
                }
            }
        }

        /// <summary>
        /// Post data to the CDN based on <see cref="PostType"/>.
        /// </summary>
        /// <param name="data">Data to upload.</param>
        /// <param name="type">Type of post to make.</param>
        /// <returns>Url of the file.</returns>
        public static async Task<string> PostToCDNAsync(string data, PostType type) {
            using (MultipartFormDataContent form = new MultipartFormDataContent()) {
                form.Add(new StringContent(SettingsManager.Configuration.CDNKey), "key");
                form.Add(new StringContent(data, Encoding.UTF8), type.ToString().ToLower());

                using (HttpResponseMessage response = await Client.PostAsync(Constants.CDNAPI, form)) {
                    if (response.IsSuccessStatusCode) {
                        using (HttpContent content = response.Content) {
                            string fileName = await content.ReadAsStringAsync();

                            if (!string.IsNullOrWhiteSpace(fileName)) {
                                return $"{Constants.CDNUploads}{fileName}";
                            } else {
                                throw new ArgumentException("File name is empty after upload", nameof(fileName));
                            }
                        }
                    } else {
                        throw new HttpRequestException($"There was an error; ({(int)response.StatusCode}) {response.ReasonPhrase}");
                    }
                }
            }
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

            using (HttpResponseMessage response = await Client.SendAsync(request)) {
                if (response.IsSuccessStatusCode) {
                    using (HttpContent content = response.Content) {
                        return await content.ReadAsStringAsync();
                    }
                } else {
                    throw new HttpRequestException($"There was an error; ({(int)response.StatusCode}) {response.ReasonPhrase}");
                }
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

            using (HttpResponseMessage response = await Client.SendAsync(request)) {
                if (response.IsSuccessStatusCode) {
                    using (HttpContent content = response.Content) {
                        using (Stream stream = await content.ReadAsStreamAsync()) {
                            MemoryStream copyStream = new MemoryStream(256);
                            stream.CopyTo(copyStream);
                            copyStream.Seek(0, SeekOrigin.Begin);

                            return copyStream;
                        }
                    }
                } else {
                    throw new HttpRequestException($"There was an error; ({(int)response.StatusCode}) {response.ReasonPhrase}");
                }
            }
        }

        /// <summary>
        /// CDN post type.
        /// </summary>
        public enum PostType {
            String,
            Url
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

