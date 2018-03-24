using AdaptiveCards;
using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SimpleEchoBot.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class LookupRestaurantDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageRecievedAsync);
        }
        public async Task MessageRecievedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            Debug.WriteLine($"Looking up restaurants");
            //we got the image recognized from cog custom service 
            var webresult = await BingWebSearch.SearchWeb("find restaurants near me");
            //once the result is there build the card 
            var adaptiveCards = BuildCard(webresult);
            var reply = context.MakeMessage();
            reply.Attachments = adaptiveCards;
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            await context.PostAsync(reply);
            context.Done(true);
        }

        private List<Attachment> BuildCard(List<WebPage> resp)
        {
            List<Attachment> cardAttachments = new List<Attachment>();
            foreach (var p in resp)
            {
                try
                {
                    AdaptiveCard card = new AdaptiveCard();

                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = p.Name,
                        Size = AdaptiveTextSize.Large,
                        Wrap = false,
                        Separator = true


                    });

                    card.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = p.Snippet.Substring(0, 100),
                        Size = AdaptiveTextSize.Default,
                        Wrap = true

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
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.InnerException);
                    Debug.WriteLine(ex.Source);
                    Debug.WriteLine(ex.StackTrace);
                }
            }
            return cardAttachments;
        }

    }
}
