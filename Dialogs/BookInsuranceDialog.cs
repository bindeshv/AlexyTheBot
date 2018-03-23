using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class BookInsuranceDialog : IDialog<object>
    {
        private bool isUserUploadingPic = false;


        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageRecievedAsync);
        }

        public async Task AfterConfirm(IDialogContext context, IAwaitable<bool> argument)
        {
            var choice = await argument;

            if (choice)
            {
                isUserUploadingPic = true;
                await context.PostAsync("Please upload the pic");
            }
            else
            {
                await context.PostAsync("Thats fine!");
            }
        }

        public async Task MessageRecievedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            if (message.Attachments != null && message.Attachments.Any())
            {
                Debug.WriteLine("message has attachment");
                //read the images
                var attachmentUrl = message.Attachments[0].ContentUrl;
                var httpClient = new HttpClient();
                try
                {
                    var attachmentData = await httpClient.GetByteArrayAsync(attachmentUrl);
                    var resp = await CustomVisionService.GetPredictionsAsync2(attachmentData, "Insurance");

                    if (resp != null)
                    {
                        Debug.WriteLine($"prediction response {resp}");
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.InnerException);
                    Debug.WriteLine(ex.Source);
                    Debug.WriteLine(ex.StackTrace);
                }

                

            }

            //only show the prompt once 
            if (!isUserUploadingPic)
            {
                PromptDialog.Confirm(context,
               AfterConfirm,
               "Do you have a picture of the car?");
            }
        }


    }
}