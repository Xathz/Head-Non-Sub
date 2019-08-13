using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using B2Net;
using B2Net.Models;
using HeadNonSub.Settings;

namespace HeadNonSub {

    public static class Backblaze {

        private static readonly B2Client _TempBucketClient = new B2Client(new B2Options() {
            AccountId = SettingsManager.Configuration.BackblazeTempBucket.KeyId,
            ApplicationKey = SettingsManager.Configuration.BackblazeTempBucket.ApplicationKey,
            KeyId = SettingsManager.Configuration.BackblazeTempBucket.KeyId,
            BucketId = SettingsManager.Configuration.BackblazeTempBucket.BucketId,
            PersistBucket = true
        });

        private static readonly B2Client _AvatarBucketClient = new B2Client(new B2Options() {
            AccountId = SettingsManager.Configuration.BackblazeAvatarBucket.KeyId,
            ApplicationKey = SettingsManager.Configuration.BackblazeAvatarBucket.ApplicationKey,
            KeyId = SettingsManager.Configuration.BackblazeAvatarBucket.KeyId,
            BucketId = SettingsManager.Configuration.BackblazeAvatarBucket.BucketId,
            PersistBucket = true
        });

        public static async Task Authorize() {
            await _TempBucketClient.Authorize();
            await _AvatarBucketClient.Authorize();
        }

        public static async Task<File> UploadTemporaryFileAsync(string content, string fileName) => await UploadTemporaryFileAsync(Encoding.UTF8.GetBytes(content), fileName);

        public static async Task<File> UploadTemporaryFileAsync(byte[] fileData, string fileName) {
            B2UploadUrl uploadUrl = await _TempBucketClient.Files.GetUploadUrl();
            string sha1Hash = Utilities.GetSHA1Hash(fileData);

            B2File uploadedFile = await _TempBucketClient.Files.Upload(fileData, fileName, uploadUrl);

            if (sha1Hash == uploadedFile.ContentSHA1) {
                string fullUrl = $"{Constants.BackblazeCDN}/file/{SettingsManager.Configuration.BackblazeTempBucket.BucketName}/{uploadedFile.FileName}";
                string shortUrl = await Http.ShortenUrl(fullUrl);

                return new File(SettingsManager.Configuration.BackblazeTempBucket.BucketName, uploadedFile.FileName, fullUrl, shortUrl);
            }

            return null;
        }

        public static async Task<File> UploadTemporaryFileAsync(MemoryStream memoryStream, string fileName) {
            B2UploadUrl uploadUrl = await _TempBucketClient.Files.GetUploadUrl();

            byte[] fileData = memoryStream.ToArray();
            memoryStream.Close();

            string sha1Hash = Utilities.GetSHA1Hash(fileData);

            B2File uploadedFile = await _TempBucketClient.Files.Upload(fileData, fileName, uploadUrl);

            if (sha1Hash == uploadedFile.ContentSHA1) {
                string fullUrl = $"{Constants.BackblazeCDN}/file/{SettingsManager.Configuration.BackblazeTempBucket.BucketName}/{uploadedFile.FileName}";
                string shortUrl = await Http.ShortenUrl(fullUrl);

                return new File(SettingsManager.Configuration.BackblazeTempBucket.BucketName, uploadedFile.FileName, fullUrl, shortUrl);
            }

            return null;
        }

        public static async Task<File> UploadAvatarAsync(MemoryStream memoryStream, string fileName) {
            B2UploadUrl uploadUrl = await _AvatarBucketClient.Files.GetUploadUrl();

            byte[] fileData = memoryStream.ToArray();
            memoryStream.Close();

            string sha1Hash = Utilities.GetSHA1Hash(fileData);

            B2File uploadedFile = await _AvatarBucketClient.Files.Upload(fileData, fileName, uploadUrl);

            if (sha1Hash == uploadedFile.ContentSHA1) {
                string fullUrl = $"{Constants.BackblazeCDN}/file/{SettingsManager.Configuration.BackblazeAvatarBucket.BucketName}/{uploadedFile.FileName}";
                string shortUrl = await Http.ShortenUrl(fullUrl);

                return new File(SettingsManager.Configuration.BackblazeAvatarBucket.BucketName, uploadedFile.FileName, fullUrl, shortUrl);
            }

            return null;
        }

        public static string ISOFileNameDate(string extension) => $"{DateTime.UtcNow.ToString("yyyyMMddTHHmmss")}Z.{extension}";

        public class File {

            private string _ShortUrl;

            public File(string bucketName, string fileName, string fullUrl, string shortUrl) {
                BucketName = bucketName;
                FileName = fileName;
                FullUrl = fullUrl;
                ShortUrl = shortUrl;
            }

            public string BucketName { get; private set; }

            public string FileName { get; private set; }

            public string FullUrl { get; private set; }

            public string ShortUrl {
                get {
                    if (string.IsNullOrWhiteSpace(_ShortUrl)) {
                        return FullUrl;
                    } else {
                        return _ShortUrl;
                    }
                }
                set => _ShortUrl = value;
            }

        }

    }

}
