using System;

namespace HeadNonSub.Entities.Twitch {

    public class Clip {

        public Clip(DateTime createdAt, string title, int viewCount, string url) {
            CreatedAt = createdAt;
            Title = title;
            ViewCount = viewCount;
            Url = url;
        }

        public DateTime CreatedAt { get; private set; }

        public string Title { get; private set; }

        public int ViewCount { get; private set; }

        public string Url { get; private set; }

    }

}
