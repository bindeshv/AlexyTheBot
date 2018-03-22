using Microsoft.Azure.CognitiveServices.Search.ImageSearch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using SimpleEchoBot.Models;

namespace SimpleEchoBot.Services
{
    public class BingImageSearch
    {
        private static ImageSearchAPI imageSearchClient = new ImageSearchAPI(new
                ApiKeyServiceClientCredentials("6d3e3024da8d43f2b20bff7f83be3dcc"));
        //6d3e3024da8d43f2b20bff7f83be3dcc


        public static List<Image> SearchImages(string imageString)
        {
            var imageResults = imageSearchClient.Images.SearchAsync(query: imageString).Result;

            if( imageResults.Value.Count > 0)
            {
                Debug.WriteLine($"image search successful! count={imageResults.Value.Count}");

                List<Image> imageList = new List<Image>();
                //take 5 
                var images = imageResults.Value.Take(5).ToList();

                foreach(var img in images)
                {
                    imageList.Add(new Image() { Title = img.Name, Description = img.Description, Url = img.ThumbnailUrl, ContentUrl = img.ContentUrl });
                }

                return imageList;
            }

            return null; 
        }
    }
}