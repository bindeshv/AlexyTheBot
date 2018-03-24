using AdaptiveCards;
using Microsoft.Azure.CognitiveServices.Search.EntitySearch;
using Microsoft.Azure.CognitiveServices.Search.EntitySearch.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using SimpleEchoBot.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SimpleEchoBot.Services;
using System.Threading;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {

        public RootLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"]
            )))
        {
            Debug.WriteLine("initialized root dialog!!");
        }

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'";
            await context.PostAsync(message);
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Greeting")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            EntityRecommendation entity; 
            if(result.TryFindEntity("Greeting.Hello", out entity))
            {
                await context.PostAsync("Hi there!");
            }

            if(result.TryFindEntity("Greeting.WhoAmI", out entity))
            {
                await context.PostAsync("I am Alexy..");
                await context.PostAsync("An Intelligent Bot powered by Microsoft Cognitive Services!");
            }

            if(result.TryFindEntity("Greeting.Question", out entity))
            {
                await context.PostAsync("I am doing very good. Thanks for asking!");
            }
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Insurance")]
        public async Task Insure(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            await context.PostAsync("ok..");
            EntityRecommendation entity;

            if (result.TryFindEntity("Insure.Car", out entity))
            {
                Debug.WriteLine("starting car insurance dialog");
                await context.Forward(new BookInsuranceDialog(), AfterConfirm, new Activity { Text = result.Query}, CancellationToken.None);
            }

        }


        [LuisIntent("Restaurants")]
        public async Task Restaurant(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            await context.PostAsync($"looking up..");
            EntityRecommendation entity;
            if(result.TryFindEntity("Restaurants.Nearby", out entity))
            {
                Debug.WriteLine("starting restaurant lookup dialog");
                await context.Forward(new LookupRestaurantDialog(), AfterConfirm, new Activity { Text = result.Query }, CancellationToken.None);

            }else
            {
                Debug.WriteLine("restaurants no entity found");
            }
        }
        

        [LuisIntent("People")]
        public async Task People(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            await context.PostAsync($"looking up..");
            EntityRecommendation entity;

            if (result.TryFindEntity("People.Info", out entity))
            {
                //if we got the query for who is XYZ 
                var entities = result.Entities;
                string personName = entities.Where(e => e.Type == "People.Name").FirstOrDefault().Entity;

                if (personName != null)
                {
                    var resp = BingEntitySearch.SearchForPerson(personName);
                    if (resp == null)
                    {
                        await context.PostAsync($"I couldn't find anything for {personName}");
                        Debug.WriteLine($"**** No result for query {result.Query}");
                    }
                    else
                    {
                        await context.PostAsync("Here is what i found...");
                        AdaptiveCard card = BuildAdaptivePersonCard(resp);
                        var convMessage = context.MakeMessage();
                        convMessage.Attachments = new List<Attachment>();
                        Attachment attachment = new Attachment()
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = card
                        };

                        convMessage.Attachments.Add(attachment);
                        await context.PostAsync(convMessage);

                    }
                }

            }
            else if (result.TryFindEntity("People.Images", out entity))
            {
                string imageName = result.Entities.Where(e => e.Type == "People.Name").FirstOrDefault().Entity;

                if (imageName != null)
                {
                    var imgList = BingImageSearch.SearchImages(imageName);
                    if (imgList != null)
                    {
                        await context.PostAsync("Here is what i found...");
                        //take 5 images                      

                        var convMessage = context.MakeMessage();
                        convMessage.Attachments = BuildImageListCard(imgList, imageName);
                        convMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                        await context.PostAsync(convMessage);

                    }
                    else
                    {
                        await context.PostAsync($"sorry didn't find any image for- {imageName}");
                    }

                }
                else
                {
                    await context.PostAsync("I didn't get the image that you're interested in");
                    await context.PostAsync("Try something like 'find pics of Ed Sheeran'");
                }
            }


            else
            {
                await context.PostAsync($"sorry couldn't understand that!");
            }

        }


        private async Task AfterConfirm(IDialogContext context, IAwaitable<object> item )
        {
            context.Wait(this.MessageReceived);
        }

        private List<Attachment> BuildImageListCard(List<Image> images, string imageQuery)
        {
            List<Attachment> attachments = new List<Attachment>();

            foreach (var img in images)
            {
                HeroCard plCard = new HeroCard();
                List<CardImage> cardImages = new List<CardImage>();
                cardImages.Add(new CardImage(url: img.Url));
                plCard.Title = img.Title;
                plCard.Images = cardImages;
                attachments.Add(plCard.ToAttachment());

            }



            return attachments;
        }

        private AdaptiveCard BuildAdaptivePersonCard(Person person)
        {
            AdaptiveCard card = new AdaptiveCard();
            card.Body.Add(new AdaptiveImage()
            {
                Url = new Uri(person.ImageUrl)
            });
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = person.Description,
                Size = AdaptiveTextSize.Medium,
                Wrap = true

            });

            card.Actions.Add(new AdaptiveOpenUrlAction()
            {
                Url = new Uri(person.Url),
                Title = "More"
            });



           

            return card;
        }
    }
}