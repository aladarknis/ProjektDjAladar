using DotNetTools.SharpGrabber;
using DotNetTools.SharpGrabber.Converter;
using DotNetTools.SharpGrabber.Grabbed;
using MediaBrowser.Model.Entities;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;

namespace ProjektDjAladar
{
    public static class Grabber
    {
        private static readonly HttpClient Client = new HttpClient();
        private static readonly HashSet<string> TempFiles = new HashSet<string>();
        private static GrabbedMedia ChooseMonoMedia(GrabResult result, MediaChannels channel)
        {
            var resources = result.Resources<GrabbedMedia>()
                .Where(m => m.Channels == channel)
                .ToList();

            if (resources.Count == 0)
                return null;

            for (var i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];
                Console.WriteLine($"{i}. {resource.Title ?? resource.FormatTitle ?? resource.Resolution}");
            }

            while (true)
            {
                Console.Write($"Choose the {channel} file: ");
                var choiceStr = Console.ReadLine();
                if (!int.TryParse(choiceStr, out var choice))
                {
                    Console.WriteLine("Number expected.");
                    continue;
                }

                if (choice < 0 || choice >= resources.Count)
                {
                    Console.WriteLine("Invalid number.");
                    continue;
                }

                return resources[choice];
            }
        }

        private static async Task<string> DownloadMedia(GrabbedMedia media, IGrabResult grabResult)
        {
            Console.WriteLine("Downloading {0}...", media.Title ?? media.FormatTitle ?? media.Resolution);
            using var response = await Client.GetAsync(media.ResourceUri);
            response.EnsureSuccessStatusCode();
            using var downloadStream = await response.Content.ReadAsStreamAsync();
            using var resourceStream = await grabResult.WrapStreamAsync(downloadStream);
            var path = Path.GetTempFileName();

            using var fileStream = new FileStream(path, FileMode.Create);
            TempFiles.Add(path);
            await resourceStream.CopyToAsync(fileStream);
            return path;
        }

        private static void GenerateOutputFile(string audioPath, GrabbedMedia videoStream)
        {
          
            var outputPath = @"E:\bot\";
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new Exception("No output path is specified.");
            var merger = new MediaMerger(outputPath);
            merger.AddStreamSource(audioPath, DotNetTools.SharpGrabber.Converter.MediaStreamType.Audio);
            merger.OutputMimeType = videoStream.Format.Mime;
            merger.OutputShortName = videoStream.Format.Extension;
            merger.Build();
            Console.WriteLine($"Output file successfully created.");
        }

        public static async Task<string> GrabYouTube(string url)
        {
            var grabber = GrabberBuilder.New().AddYouTube().Build();
            var grabResult = await grabber.GrabAsync(new Uri(url));

            var audioStream = ChooseMonoMedia(grabResult, MediaChannels.Audio);
            if (audioStream == null)
                throw new InvalidOperationException("No audio stream detected.");
            try
            {
                var audioPath = await DownloadMedia(audioStream, grabResult);

                GenerateOutputFile(audioPath, audioStream);

                return audioPath;
            }
            finally
            {
                foreach (var tempFile in TempFiles)
                {
                    try
                    {
                        File.Delete(tempFile);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

    }
}


