namespace HeadNonSub {

    public static partial class Backblaze {

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
