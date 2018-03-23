using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using SimpleEchoBot.Services;
using AdaptiveCards;
using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;

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
                        //we got the image recognized from cog custom service 
                        var webresult = await BingWebSearch.SearchWeb(resp + " insurance bangalore");
                        //once the result is there build the card 
                        var adaptiveCards = BuildCard(webresult);
                        var reply = context.MakeMessage();
                        reply.Attachments = adaptiveCards;
                        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        await context.PostAsync(reply);


                    }else
                    {
                        await context.PostAsync("Sorry, I didn't recognize the image");
                       
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


        private List<Attachment> BuildCard(List<WebPage> resp)
        {
            List<Attachment> cardAttachments = new List<Attachment>();
            foreach(var p in resp)
            {
                AdaptiveCard card = new AdaptiveCard();
                //card.Body.Add(new AdaptiveImage()
                //{
                //    Url = new Uri(p.Image.ThumbnailUrl)
                //});

                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = p.Name,
                    Size = AdaptiveTextSize.Large,
                    Wrap = false,
                    Separator = true


                });

                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = p.Snippet.Substring(0, 50),
                    Size = AdaptiveTextSize.Default,
                    Wrap = false

                });

               
                card.Actions.Add(new AdaptiveOpenUrlAction()
                {
                    Url = new Uri(p.Url),
                    Title = "More"
                }
                );

                Attachment attachment = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                };
                cardAttachments.Add(attachment);
            }
            return cardAttachments;
        }

    }
}