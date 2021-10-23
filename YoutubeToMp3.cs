using MediaToolkit;
using MediaToolkit.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;

namespace ProjektDjAladar
{
    public static class YoutubeToMp3
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
