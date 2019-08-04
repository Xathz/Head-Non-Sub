using System;
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
        /// <param name="input">File to upload.</param>
        /// <param name="fileExtension">Extension of the file to upload.</param>
        /// <returns>Url of the file.</returns>
        public static async Task<string> UploadFileToCDN(byte[] byteArray, string fileExtension) {
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            using (StreamContent streamContent = new StreamContent(new MemoryStream(byteArray)))
            using (ByteArrayContent fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync())) {
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                form.Add(new StringContent(SettingsManager.Configuration.UploadKey), "key");
                form.Add(fileContent, "file", $"{Guid.NewGuid()}.{fileExtension}");

                using (HttpResponseMessage response = await Client.PostAsync(Constants.CDNAPI, form)) {
                    if (response.IsSuccessStatusCode) {
                        using (HttpContent content = response.Content) {
                            string fileName = await content.ReadAsStringAsync();

                            if (!string.IsNullOrWhiteSpace(fileName)) {
                                return $"{Constants.CDNUploads}{fileName}";
                            } else {
                                throw new ArgumentException("File name is empty", nameof(fileName));
                            }
                        }
                    } else {
                        throw new HttpRequestException($"There was an error uploading the file. {(int)response.StatusCode}; {response.ReasonPhrase}");
                    }
                }
            }
        }

        /// <summary>
        /// Send a string to be stored on the CDN.
        /// </summary>
        /// <param name="data">String to upload.</param>
        /// <returns>Url of the file.</returns>
        public static async Task<string> UploadStringToCDN(string data) {
            using (MultipartFormDataContent form = new MultipartFormDataContent()) {
                form.Add(new StringContent(SettingsManager.Configuration.UploadKey), "key");
                form.Add(new StringContent(data, Encoding.UTF8), "string");

                using (HttpResponseMessage response = await Client.PostAsync(Constants.CDNAPI, form)) {
                    if (response.IsSuccessStatusCode) {
                        using (HttpContent content = response.Content) {
                            string fileName = await content.ReadAsStringAsync();

                            if (!string.IsNullOrWhiteSpace(fileName)) {
                                return $"{Constants.CDNUploads}{fileName}";
                            } else {
                                throw new ArgumentException("File name is empty", nameof(fileName));
                            }
                        }
                    } else {
                        throw new HttpRequestException($"There was an error uploading the string. {(int)response.StatusCode}; {response.ReasonPhrase}");
                    }
                }
            }
        }

        /// <summary>
        /// Send a url to the CDN to be downloaded and stored.
        /// </summary>
        /// <param name="url">Url to be downloaded on the CDN.</param>
        /// <returns>Url of the file.</returns>
        public static async Task<string> UploadUrlToCDN(string url) {
            using (MultipartFormDataContent form = new MultipartFormDataContent()) {
                form.Add(new StringContent(SettingsManager.Configuration.UploadKey), "key");
                form.Add(new StringContent(url), "url");

                using (HttpResponseMessage response = await Client.PostAsync(Constants.CDNAPI, form)) {
                    if (response.IsSuccessStatusCode) {
                        using (HttpContent content = response.Content) {
                            string fileName = await content.ReadAsStringAsync();

                            if (!string.IsNullOrWhiteSpace(fileName)) {
                                return $"{Constants.CDNUploads}{fileName}";
                            } else {
                                throw new ArgumentException("File name is empty", nameof(fileName));
                            }
                        }
                    } else {
                        throw new HttpRequestException($"There was an error uploading the url. {(int)response.StatusCode}; {response.ReasonPhrase}");
                    }
                }
            }
        }

    }

}

