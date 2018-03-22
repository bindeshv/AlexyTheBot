using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SimpleEchoBot.Dialogs
{
    [Serializable]
    public class BookInsuranceDialog : IDialog<object>
    {

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageRecievedAsync);
        }

        public async Task AfterConfirm(IDialogContext context, IAwaitable<bool> argument)
        {
            var choice = await argument;

            if (choice)
            {
                await context.PostAsync("Good to know!");
            }
            else
            {
               await context.PostAsync("Thats fine!");
            }
        }


        public async Task MessageRecievedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument; 
            PromptDialog.Confirm(context,
                AfterConfirm,
                "Do you have a picture of the car?");
        }

    }
}