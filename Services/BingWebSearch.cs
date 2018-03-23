using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;
using Microsoft.Azure.CognitiveServices.Search.WebSearch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SimpleEchoBot.Services
{
    public static class BingWebSearch
    {
       private static WebSearchAPI client = new WebSearchAPI(new ApiKeyServiceClientCredentials("18ff0190f93842a39a7635c1f803cac0"));

        public async static Task<List<WebPage>> SearchWeb(string searchQuery)
        {

            var webData = await client.Web.SearchAsync(query: searchQuery);
            if (webData?.WebPages?.Value?.Count > 0)
            {
                Debug.WriteLine($"Bing search response recvd {webData.WebPages.Value.Count}");
                //take 5 
                List<WebPage> resp =
                    webData.WebPages.Value.Take(5).ToList(); 
                return resp;
            }
            else
            {
                Debug.WriteLine("Bing search response recvd no values!");
                return null;
            }

        }
    }
}