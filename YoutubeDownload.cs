using MediaToolkit.Model;
using System.IO;
using VideoLibrary;

namespace ProjektDjAladar
{
    public static class YoutubeDownload
    {
        public static string DownloadVideo(string url)
        {
            var source = @"E:\bot\";
            var youtube = YouTube.Default;
            var vid = youtube.GetVideo(url);
            File.WriteAllBytes(source + vid.FullName, vid.GetBytes());

            var file = new MediaFile { Filename = source + vid.FullName };

            return file.Filename.ToString();
        }
    }
}
