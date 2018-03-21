using Microsoft.Azure.CognitiveServices.Search.EntitySearch;
using Microsoft.Azure.CognitiveServices.Search.EntitySearch.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

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

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";
            await context.PostAsync(message);
            context.Wait(this.MessageReceived);
        }

        //[LuisIntent("Help")]
        //public async Task Help(IDialogContext context, LuisResult result)
        //{
        //    await context.PostAsync("Hi! Try asking me things like , 'search hotels near LAX airport','who is Selena Gomez?'or 'show me the reviews of The Bot Resort'");
        //    context.Wait(this.MessageReceived);
        //}

        [LuisIntent("People")]
        public async Task People(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            await context.PostAsync($"I am analyzing your query..be right back!");
            EntityRecommendation entity;

            if (result.TryFindEntity("People.Info",out entity))
            {
                //if we got the query for who is XYZ 
                var entities = result.Entities;
                string personName = entities.Where(e => e.Type == "People.Name").FirstOrDefault().Entity;

                if (personName != null)
                {
                    var resp = BingSearch.SearchForPerson(personName);
                    if (resp == null)
                    {
                        await context.PostAsync($"I couldn't find anything for {personName}");
                        Debug.WriteLine($"**** No result for query {result.Query}");
                    }
                    else
                    {
                        await context.PostAsync("Here is what i found...");
                        await context.PostAsync(resp);
                    }
                }

            }else
            {
                await context.PostAsync($"sorry couldn't understand that!");
            }

        }

        
    }
}