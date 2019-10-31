using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using B2Net;
using B2Net.Models;
using HeadNonSub.Settings;

namespace HeadNonSub {

    public static partial class Backblaze {

        /// <summary>
        /// Temporary files bucket. Files will be hidden after 7 days and deleted after 10.
        /// </summary>
        private static readonly B2Client _TempBucketClient = new B2Client(new B2Options() {
            AccountId = SettingsManager.Configuration.BackblazeTempBucket.KeyId,
            ApplicationKey = SettingsManager.Configuration.BackblazeTempBucket.ApplicationKey,
            KeyId = SettingsManager.Configuration.BackblazeTempBucket.KeyId,
            BucketId = SettingsManager.Configuration.BackblazeTempBucket.BucketId,
            PersistBucket = true
        });

        /// <summary>
        /// Avatar files bucket.
        /// </summary>
        private static readonly B2Client _AvatarBucketClient = new B2Client(new B2Options() {
            AccountId = SettingsManager.Configuration.BackblazeAvatarBucket.KeyId,
            ApplicationKey = SettingsManager.Configuration.BackblazeAvatarBucket.ApplicationKey,
            KeyId = SettingsManager.Configuration.BackblazeAvatarBucket.KeyId,
            BucketId = SettingsManager.Configuration.BackblazeAvatarBucket.BucketId,
            PersistBucket = true
        });

        /// <summary>
        /// Authorize with the Backblaze api.
        /// </summary>
        public static async Task Authorize() {
            await _TempBucketClient.Authorize();
            await _AvatarBucketClient.Authorize();
        }

        /// <summary>
        /// Uploads a temporary file.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="fileName">Name of the file.</param>
        public static async Task<File> UploadTemporaryFileAsync(string content, string fileName) => await UploadTemporaryFileAsync(Encoding.UTF8.GetBytes(content), fileName);

        /// <summary>
        /// Uploads a temporary file.
        /// </summary>
        /// <param name="fileData">The file data.</param>
        /// <param name="fileName">Name of the file.</param>
        public static async Task<File> UploadTemporaryFileAsync(byte[] fileData, string fileName) {
            await Authorize();

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

        /// <summary>
        /// Uploads a temporary file.
        /// </summary>
        /// <param name="memoryStream">The memory stream.</param>
        /// <param name="fileName">Name of the file.</param>
        public static async Task<File> UploadTemporaryFileAsync(MemoryStream memoryStream, string fileName) {
            await Authorize();

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

        /// <summary>
        /// Uploads an avatar.
        /// </summary>
        /// <param name="memoryStream">The memory stream.</param>
        /// <param name="fileName">Name of the file.</param>
        public static async Task<File> UploadAvatarAsync(MemoryStream memoryStream, string fileName) {
            await Authorize();

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

        /// <summary>
        /// ISO 8601 timestamp usable as a file name.
        /// </summary>
        /// <param name="extension">Extension of the file.</param>
        public static string ISOFileNameDate(string extension) => $"{DateTime.UtcNow.ToString("yyyyMMddTHHmmss")}Z.{extension}";

    }

}
