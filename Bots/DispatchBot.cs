// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public class DispatchBot : ActivityHandler
    {
        private ILogger<DispatchBot> _logger;
        private IBotServices _botServices;

        public DispatchBot(IBotServices botServices, ILogger<DispatchBot> logger)
        {
            _logger = logger;
            _botServices = botServices;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                // _logger.LogInformation($"Dispatch unrecognized intent: {turnContext}.");
                var dc = new DialogContext(new DialogSet(), turnContext, new DialogState());
                // Top intent tell us which cognitive service to use.
                // var allScores = await _botServices.Dispatch.RecognizeAsync(dc, (Activity)turnContext.Activity, cancellationToken);
                // var topIntent = allScores.Intents.First().Key;

                // Next, we call the dispatcher with the top intent.
                await DispatchToTopIntentAsync(turnContext, null, cancellationToken);
            }
            catch (Exception error)
            {
                _logger.LogInformation($"Dispatch error received: {error}.");
            }

        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            const string WelcomeText = "Type a greeting, or a question about the weather to get started.";

            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome to Dispatch bot {member.Name}. {WelcomeText}"), cancellationToken);
                }
            }
        }

        private async Task DispatchToTopIntentAsync(ITurnContext<IMessageActivity> turnContext, string intent, CancellationToken cancellationToken)
        {

            await ProcessHomeAutomationAsync(turnContext, cancellationToken);
            //   await ProcessSampleQnAAsync(turnContext, cancellationToken);
            // switch (intent)
            // {
            //    //  case "HomeAutomation":
            //    //      await ProcessHomeAutomationAsync(turnContext, cancellationToken);
            //    //      break;
            //    //  case "Weather":
            //    //      await ProcessWeatherAsync(turnContext, cancellationToken);
            //    //      break;
            //     case "QnAMaker":
            //         await ProcessSampleQnAAsync(turnContext, cancellationToken);
            //         break;
            //     default:
            //         _logger.LogInformation($"Dispatch unrecognized intent.");
            //         await turnContext.SendActivityAsync(MessageFactory.Text($"Dispatch unrecognized intent."), cancellationToken);
            //         break;
            // }
        }

        private async Task ProcessHomeAutomationAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProcessHomeAutomationAsync");

            // Retrieve LUIS result for HomeAutomation.
            var recognizerResult = await _botServices.LuisHomeAutomationRecognizer.RecognizeAsync(turnContext, cancellationToken);
            var result = recognizerResult.Properties["luisResult"] as LuisResult;
            _logger.LogInformation($"Recognized intent {result.ToString()}");

            var topIntent = result.TopScoringIntent.Intent;
            var score = result.TopScoringIntent.Score;

            if (topIntent == "Greeting" && score >= 0.70)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Hi, My name is Deepak-Naggaro Bot, I can help you with Pizza Information and Shop timing"), cancellationToken);

            } else if(topIntent == "GoodBye" && score >= 0.70)   {
						await turnContext.SendActivityAsync(MessageFactory.Text($"Thank you! If you need any other help I am available 24*7.\n\nHave a great day!"), cancellationToken);
				}      
            else
            {
                _logger.LogInformation($"Unrecognized intent");
                await ProcessSampleQnAAsync(turnContext, cancellationToken);
            }

            // switch (topIntent)
            // {
            //     case "Greeting":
            // 		  await turnContext.SendActivityAsync(MessageFactory.Text($"Hi, My name is Deepak-Naggaro Bot, I can help you with Pizza Information and Shop timing"), cancellationToken);
            //         break;
            //     case "None":
            //         await ProcessSampleQnAAsync(turnContext, cancellationToken);
            //         break;
            //     default:
            //         _logger.LogInformation($"Unrecognized intent");
            //         await turnContext.SendActivityAsync(MessageFactory.Text($"Sorry didn't get you. You can try with below examples\n'What you can offer'\n'How slices in personal pizza'\n'Where I can find you'."), cancellationToken);
            //         break;
            // }

            // await turnContext.SendActivityAsync(MessageFactory.Text($"HomeAutomation top intent {topIntent}."), cancellationToken);
            // await turnContext.SendActivityAsync(MessageFactory.Text($"HomeAutomation intents detected:\n\n{string.Join("\n\n", result.Intents.Select(i => i.Intent))}"), cancellationToken);
            // if (result.Entities.Count > 0)
            // {
            //     await turnContext.SendActivityAsync(MessageFactory.Text($"HomeAutomation entities were found in the message:\n\n{string.Join("\n\n", result.Entities.Select(i => i.Entity))}"), cancellationToken);
            // }
        }

         //  private async Task ProcessWeatherAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
         //  {
         //      _logger.LogInformation("ProcessWeatherAsync");

         //      // Retrieve LUIS result for Weather.
         //      var recognizerResult = await _botServices.LuisWeatherRecognizer.RecognizeAsync(turnContext, cancellationToken);
         //      var result = recognizerResult.Properties["luisResult"] as LuisResult;
         //      var topIntent = result.TopScoringIntent.Intent;

         //      await turnContext.SendActivityAsync(MessageFactory.Text($"ProcessWeather top intent {topIntent}."), cancellationToken);
         //      await turnContext.SendActivityAsync(MessageFactory.Text($"ProcessWeather Intents detected::\n\n{string.Join("\n\n", result.Intents.Select(i => i.Intent))}"), cancellationToken);
         //      if (result.Entities.Count > 0)
         //      {
         //          await turnContext.SendActivityAsync(MessageFactory.Text($"ProcessWeather entities were found in the message:\n\n{string.Join("\n\n", result.Entities.Select(i => i.Entity))}"), cancellationToken);
         //      }
         //  }

        private async Task ProcessSampleQnAAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProcessSampleQnAAsync");

            var results = await _botServices.SampleQnA.GetAnswersAsync(turnContext);
            if (results.Any())
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(results.First().Answer), cancellationToken);
            }
            else
            {
                await ProcessShopQnAAsync(turnContext, cancellationToken);
            }
        }

		  private async Task ProcessShopQnAAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProcessShopQnAAsync");

            var results = await _botServices.ShopQnA.GetAnswersAsync(turnContext);
            if (results.Any())
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(results.First().Answer), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Sorry didn't get you. You can try with below examples\n\n'What you can offer'\n\n'How slices in personal pizza'\n\n'Where I can find you'."), cancellationToken);
            }
        }
    }
}
