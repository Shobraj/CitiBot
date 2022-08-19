// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.16.0

using CitiBot.CognitiveModels;
using CitiBot.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CitiBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly ILogger _logger;
        private readonly string BookAppointment = "BookAppointment";
        private readonly string ViewAppointment = "ViewAppointment";

        protected readonly UserState UserState;
        private readonly ConversationState conversationState;
        private readonly IApiHelper apiHelper;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(ILogger<MainDialog> logger, UserState userState, ConversationState conversationState, IApiHelper apiHelper)
            : base(nameof(MainDialog))
        {
            _logger = logger;
            UserState = userState;
            this.conversationState = conversationState;
            this.apiHelper = apiHelper;

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            var InitwaterfallSteps = new WaterfallStep[]
            {
                IntroStepAsync,
                DeciderStepAsync
            };

            var BookwaterfallSteps = new WaterfallStep[]
            {
                FirstStepAsync,
                SecondStepAsync,
                ThirdStepAsync,
                FourthStepAsync,
                FifthStepAsync,
                SixthSteAsyncc,
                SeventhSteAsyncc
            };

            var ViewwaterfallSteps = new WaterfallStep[]
            {
                //IntroStepAsync,
                //SecondStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), InitwaterfallSteps));
            AddDialog(new WaterfallDialog(this.BookAppointment, BookwaterfallSteps));
            AddDialog(new WaterfallDialog(this.ViewAppointment, ViewwaterfallSteps));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
            UserState = userState;
        }

       
        private Attachment CreateAdaptiveCardAttachment(string name)
        {
            var cardResourcePath = GetType().Assembly.GetManifestResourceNames().First(name => name.EndsWith("welcomeCard.json"));

            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    string adaptiveCard = reader.ReadToEnd();
                    adaptiveCard = adaptiveCard.Replace("__PatientNme__", name);
                    return new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = JsonConvert.DeserializeObject(adaptiveCard, new JsonSerializerSettings { MaxDepth = null }),
                    };
                }
            }
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userStateAccessor = UserState.CreateProperty<PatientDetails>(nameof(PatientDetails));
            var userData = await userStateAccessor.GetAsync(stepContext.Context, () => new PatientDetails());
            var welcomeCard = CreateAdaptiveCardAttachment(userData.name);
            var response = MessageFactory.Attachment(welcomeCard, ssml: "Welcome to Bot Framework!");
            await stepContext.Context.SendActivityAsync(response, cancellationToken);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
        private async Task<DialogTurnResult> DeciderStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            dynamic user_input = stepContext.Context.Activity.AsMessageActivity().Value;

            if(user_input != null)
            {
                if(user_input.value == "Book_Appointment")
                {
                    await stepContext.BeginDialogAsync(this.BookAppointment);
                }
                else if(user_input.value == "View_Appointment")
                {
                    await stepContext.Context.SendActivityAsync("Its not in the requirement.");
                    return await stepContext.EndDialogAsync();
                }
                else
                {
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog));
                }
            }
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> FirstStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var ConverstationStateAccessor = conversationState.CreateProperty<AppointmentDetails>(nameof(AppointmentDetails));
            var ConverstationData = await ConverstationStateAccessor.GetAsync(stepContext.Context, () => new AppointmentDetails());
            var apiresult = await apiHelper.Get_Previous_Appointment<PreviousAppointment>();
            if (apiresult == null)
            {
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog));
            }
            ConverstationData.DrName = apiresult.ProviderName;
            ConverstationData.FacilityName = apiresult.FacilityName;
            ConverstationData.FacilityId = apiresult.FacilityId;

            await stepContext.Context.SendActivityAsync("Ok lets get you new appointment. I just need you to answer a few questions.");
            string[] path = { "./Cards/", "FirstStepCard.json" };
            var str = File.ReadAllText(Path.Combine(path));
            var cardJsonObj = JObject.Parse(str);
            var attachment = new Attachment
            {
                Content = cardJsonObj,
                ContentType = "application/vnd.microsoft.card.adaptive"
            };
            var reply = MessageFactory.Attachment(attachment);
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
        private async Task<DialogTurnResult> SecondStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            dynamic user_input = stepContext.Context.Activity.AsMessageActivity().Value;
            if (user_input != null)
            {
                if (user_input.value == "yes")
                {
                    var ConverstationStateAccessor = conversationState.CreateProperty<AppointmentDetails>(nameof(AppointmentDetails));
                    var ConverstationData = await ConverstationStateAccessor.GetAsync(stepContext.Context, () => new AppointmentDetails());
                    string[] path = { "./Cards/", "SecondStepCard.json" };
                    var str = File.ReadAllText(Path.Combine(path));
                    str = str.Replace("__Dr_Name__", ConverstationData.DrName);
                    str = str.Replace("__Location__", ConverstationData.FacilityName);

                    var cardJsonObj = JObject.Parse(str);
                    var attachment = new Attachment
                    {
                        Content = cardJsonObj,
                        ContentType = "application/vnd.microsoft.card.adaptive"
                    };
                    var reply = MessageFactory.Attachment(attachment);
                    await stepContext.Context.SendActivityAsync(reply, cancellationToken);
                }
                else
                {
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog));
                }
            }
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
        private async Task<DialogTurnResult> ThirdStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            dynamic user_input = stepContext.Context.Activity.AsMessageActivity().Value;
            if (user_input != null)
            {
                if (user_input.value == "yes")
                {
                    var ConverstationStateAccessor = conversationState.CreateProperty<AppointmentDetails>(nameof(AppointmentDetails));
                    var ConverstationData = await ConverstationStateAccessor.GetAsync(stepContext.Context, () => new AppointmentDetails());

                    //API call to get dates
                    var datelist = await apiHelper.Get_Dates<List<Appointment_Dates>>();
                    if(datelist == null)
                    {
                        return await stepContext.ReplaceDialogAsync(nameof(MainDialog));
                    }

                    StringBuilder strb = new StringBuilder();
                    foreach (var item in datelist)
                    {
                        strb = strb.Append("{\"type\": \"Action.Submit\",\"title\": \"" + item.startDate + "\",\"data\": {\"value\": \"" + item.startDate + "\"}},");
                    }
                    string[] path = { "./Cards/", "ThirdStepCard.json" };
                    var str = File.ReadAllText(Path.Combine(path));
                    str = str.Replace("__dynamic_data__", strb.ToString());

                    var cardJsonObj = JObject.Parse(str);
                    var attachment = new Attachment
                    {
                        Content = cardJsonObj,
                        ContentType = "application/vnd.microsoft.card.adaptive"
                    };
                    var reply = MessageFactory.Attachment(attachment);
                    await stepContext.Context.SendActivityAsync(reply, cancellationToken);
                }
                else
                {
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog));
                }
            }
            
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
        private async Task<DialogTurnResult> FourthStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var ConverstationStateAccessor = conversationState.CreateProperty<AppointmentDetails>(nameof(AppointmentDetails));
            var ConverstationData = await ConverstationStateAccessor.GetAsync(stepContext.Context, () => new AppointmentDetails());

            //storing date selected
            dynamic dateSelected = stepContext.Context.Activity.AsMessageActivity().Value;
            ConverstationData.date = dateSelected.value;
            await ConverstationStateAccessor.SetAsync(stepContext.Context, ConverstationData);

            if (ConverstationData.date != "Go_to_Previous_Menu")
            {
                //Get Timing
                var timelist = await apiHelper.Get_Appointment_Slots<List<Appointment_Slots>>(ConverstationData.date);

                if (timelist == null)
                {
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog));
                }

                StringBuilder strb = new StringBuilder();
                foreach (var item in timelist)
                {
                    strb = strb.Append("{\"type\": \"Action.Submit\",\"title\": \"" + item.startTime + "\",\"data\": {\"value\": \"" + item.startTime + "\"}},");
                }
                string[] path = { "./Cards/", "ThirdStepCard.json" };
                var str = File.ReadAllText(Path.Combine(path));
                str = str.Replace("__dynamic_data__", strb.ToString());

                var cardJsonObj = JObject.Parse(str);
                var attachment = new Attachment
                {
                    Content = cardJsonObj,
                    ContentType = "application/vnd.microsoft.card.adaptive"
                };
                var reply = MessageFactory.Attachment(attachment);
                await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            }
            else
            {
                await stepContext.BeginDialogAsync(this.BookAppointment);
            }
            
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
        private async Task<DialogTurnResult> FifthStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var ConverstationStateAccessor = conversationState.CreateProperty<AppointmentDetails>(nameof(AppointmentDetails));
            var ConverstationData = await ConverstationStateAccessor.GetAsync(stepContext.Context, () => new AppointmentDetails());

            //storing time selected
            dynamic dateSelected = stepContext.Context.Activity.AsMessageActivity().Value;
            ConverstationData.time = dateSelected.value;
            await ConverstationStateAccessor.SetAsync(stepContext.Context, ConverstationData);

            if (ConverstationData.time != "Go_to_Previous_Menu")
            {
                string[] path = { "./Cards/", "FifthStepCard.json" };
                var str = File.ReadAllText(Path.Combine(path));
                str = str.Replace("__Provider_Name__", ConverstationData.DrName);
                str = str.Replace("__Facility_Address__", ConverstationData.FacilityName);
                str = str.Replace("__Date_and_Time__", ConverstationData.date + " " + ConverstationData.time);

                var cardJsonObj = JObject.Parse(str);
                var attachment = new Attachment
                {
                    Content = cardJsonObj,
                    ContentType = "application/vnd.microsoft.card.adaptive"
                };
                var reply = MessageFactory.Attachment(attachment);

                await stepContext.Context.SendActivityAsync("Done! Thank you for provinding me these details. Please wait for some time and let me compile your appointment.");
                await stepContext.Context.SendActivityAsync("Can you please review the appointment and clik on Book Appointment button to confirm the appointment.");
                await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            }
            else
            {
                await stepContext.BeginDialogAsync(this.BookAppointment);
            }
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
        private async Task<DialogTurnResult> SixthSteAsyncc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            dynamic user_input = stepContext.Context.Activity.AsMessageActivity().Value;

            if (user_input != null)
            {
                if (user_input.value == "Book_Appointment")
                {
                    var ConverstationStateAccessor = conversationState.CreateProperty<AppointmentDetails>(nameof(AppointmentDetails));
                    var ConverstationData = await ConverstationStateAccessor.GetAsync(stepContext.Context, () => new AppointmentDetails());

                    //Booking Appointment
                    var appointment = await apiHelper.Book_Appointment<BookAppointment>(ConverstationData);

                    if (appointment == null)
                    {
                        return await stepContext.ReplaceDialogAsync(nameof(MainDialog));
                    }

                    if (appointment.message.Contains("success"))
                    {
                        string[] path = { "./Cards/", "SixthStepCard.json" };
                        var str = File.ReadAllText(Path.Combine(path));
                        str = str.Replace("__Dr_Name__", ConverstationData.DrName);
                        str = str.Replace("__Date__", ConverstationData.date);
                        str = str.Replace("__Time__", ConverstationData.time);

                        var cardJsonObj = JObject.Parse(str);
                        var attachment = new Attachment
                        {
                            Content = cardJsonObj,
                            ContentType = "application/vnd.microsoft.card.adaptive"
                        };
                        var reply = MessageFactory.Attachment(attachment);
                        await stepContext.Context.SendActivityAsync(reply, cancellationToken);
                    }
                    else
                    {
                        return await stepContext.ReplaceDialogAsync(nameof(MainDialog));
                    }
                }
                else if (user_input.value == "another_Date_and_Slot")
                {
                    await stepContext.BeginDialogAsync(this.BookAppointment);
                }
                else if (user_input.value == "another_Provider_and_Location")
                {
                    await stepContext.BeginDialogAsync(this.BookAppointment);
                }
                else
                {
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog));
                }
            }
            
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }
        private async Task<DialogTurnResult> SeventhSteAsyncc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            dynamic user_input = stepContext.Context.Activity.AsMessageActivity().Value;

            if (user_input != null)
            {
                if (user_input.value == "Book_Another_Appointment")
                {
                    await stepContext.BeginDialogAsync(this.BookAppointment);
                }
                else if (user_input.value == "Main_Menu")
                {
                    await stepContext.CancelAllDialogsAsync();
                    return await stepContext.BeginDialogAsync(nameof(MainDialog));
                }
                else
                {
                    await stepContext.CancelAllDialogsAsync();
                    return await stepContext.BeginDialogAsync(nameof(MainDialog));
                }
            }
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

    }
}
