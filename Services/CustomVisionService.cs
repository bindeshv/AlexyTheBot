using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using SimpleEchoBot.Utils;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Cognitive.CustomVision.Prediction;
using System.IO;

namespace SimpleEchoBot.Services
{
    public static class CustomVisionService
    {

        public static async Task<string> GetPredictionsAsync(byte[] imageBytes, string intent)
        {

            Debug.WriteLine("inside GetPredictionsAsync ");
            string projectid, apikey;

            GetProjectBasedOnIntent(intent, out projectid, out apikey);

            var client = new HttpClient();

            // Request headers - replace this example key with your valid subscription key.
            client.DefaultRequestHeaders.Add("Prediction-Key",apikey);

            // Prediction URL - replace this example URL with your valid prediction URL.
            string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.1/Prediction/67b6a269-1648-4f9a-a79b-9b78b01dd38d/image?iterationId=9c142e9b-82f2-4b1a-aff6-c39d491334f8";

            HttpResponseMessage response;

            // Request body. Try this sample with a locally stored image.
           
                
            using (var content = new ByteArrayContent(imageBytes))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }


            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> GetPredictionsAsync2(byte[] imageBytes, string intent)
        {

            Debug.WriteLine("inside GetPredictionsAsync2 ");
            string projectid, apikey;

                GetProjectBasedOnIntent(intent, out projectid, out apikey);

            // Create a prediction endpoint, passing in the obtained prediction key
            PredictionEndpoint endpoint = new PredictionEndpoint() { ApiKey = apikey };

            // Make a prediction against the new project
            Debug.WriteLine("Making a prediction:");
            var result = await endpoint.PredictImageAsync(new Guid(projectid), new MemoryStream(imageBytes));

            var pn = result.Predictions.Where(e => e.Probability > 0.5).FirstOrDefault();

            if(pn != null)
            {
                Debug.Write($"responese Prediction tag {pn.Tag}");
                return pn.Tag;
            }else
            {
                Debug.Write("We didnt get any prediction");
            }
            

            return null;
        }


        private static void GetProjectBasedOnIntent(string intent, out string projectid, out string apikey )
        {
            switch(intent)
            {
                case "Insurance":

                    projectid = CustomVisionKeys.INSURANCE_PROJECT_ID;
                    apikey = CustomVisionKeys.INSURANCE_API_KEY;
                        
                    break;

                default:
                    projectid = "";
                    apikey = "";
                    break;
            }
        }

    }
}