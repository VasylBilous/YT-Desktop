using Google.Apis.YouTube.v3.Data;
using System;

namespace Youtube_Desktop
{
    [Serializable]
    public class Video
    {
        public string Adress { get; set; }
        public string ImageUrl { get; set; }
        public string VideoTitle { get; set; }
        public string ChannelTitle { get; set; }
        public string Desctiption { get; set; }
        public DateTime PostedDate { get; set; }
        public bool IsVideo { get; set; }
        public ResourceId ResourceId { get; set; }

        public Video() { }

        public Video(SearchResult searchResult)
        {
            Adress = string.Format("https://www.youtube.com/embed/{0}?rel=0&amp;showinfo=0", searchResult.Id.VideoId);
            ChannelTitle = searchResult.Snippet.ChannelTitle;
            ImageUrl = FindImageUrl(searchResult.Snippet.Thumbnails);
            VideoTitle = searchResult.Snippet.Title;
            PostedDate = (DateTime)searchResult.Snippet.PublishedAt;
            IsVideo = IsResultVideo(searchResult);
            Desctiption = searchResult.Snippet.Description;
            ResourceId = searchResult.Id;
        }

        public string FindImageUrl(ThumbnailDetails details)
        {
            string res = "";
            if (details.Standard != null)
            {
                res = details.Standard.Url;
                return res;
            }
            else if (details.Medium != null)
            {
                res = details.Medium.Url;
                return res;
            }
            else if (details.High != null)
            {
                res = details.High.Url;
                return res;
            }
            else if (details.Maxres != null)
            {
                res = details.Maxres.Url;
                return res;
            }
            return res;
        }
        public bool IsResultVideo(SearchResult searchResult)
        {
            if (searchResult.Id.Kind == "youtube#video")
                return true;
            return false;            
        }
    }
}
