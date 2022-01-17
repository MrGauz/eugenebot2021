using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace eugenebot2021
{
    class Bot
    {
        private static ITelegramBotClient BotClient;

        // Admins' Telegram IDs
        public static readonly long[] Admins = {
             1, // GAUZ_ID
             2, // DANI_ID
        };

        // B-Day princess's Telegram ID
        private const long EUGENE_ID = 0;

        public static void Initialize()
        {
            BotClient = new TelegramBotClient("<token>");

            using var cancellationTokenSource = new CancellationTokenSource();

            GreetAdmins();

            BotClient.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                               cancellationTokenSource.Token);
        }

        #region Sending
        public static void GreetAdmins()
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Greeting admins...");
            foreach (long admin in Admins)
            {
                SendAdminUsage(admin);

                // Send keyboard
                var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                    new KeyboardButton[][]
                    {
                        new KeyboardButton[] { "квест ua_artist", "квест alice_cooper", "квест burp" },
                        new KeyboardButton[] { "квест memes", "квест motocycles", "квест welldone" },
                        new KeyboardButton[] { "муз квиз раунд 1", "муз квиз раунд 2", "муз квиз раунд 3" },
                        new KeyboardButton[] { "Как пользоваться админкой?" },
                    })
                {
                    ResizeKeyboard = true
                };

                BotClient.SendTextMessageAsync(
                    chatId: admin,
                    text: "Здарова, админ нашей залупы",
                    replyMarkup: replyKeyboardMarkup);
            }
        }

        public static void SendAdminUsage(long admin)
        {
            var usage = @"
Usage:
  <code>/send message_name_as_in_json</code>
  <code>/forward text you want to send to Eugene</code>
  <code>/progress 42</code>

Keyboard:
  First 2 rows for increasing side-side quests completeness
  Next button starts the music quiz's rounds
  Last button is for showing this message
                ";

            SendMessage(usage, admin);
        }

        public static void UpdateQuestsStatusAsync()
        {
            string progress_filled = "\u25a0";
            string progress_empty = "\u25a1";
            string cross = "\u2716\ufe0f";
            string star = "\u2b50\ufe0f";
            var message = Quest.Messages.Single(m => m.Name == "quests_progress_status").Text;

            message = message.Replace($"[main_progress]", Quest.MainQuestProgress.ToString());
            string progress = string.Concat(Enumerable.Repeat(progress_filled, Quest.MainQuestProgress / 4)) +
                    string.Concat(Enumerable.Repeat(progress_empty, 25 - Quest.MainQuestProgress / 4));
            message = message.Replace($"[main_progress_bar]", progress);

            foreach (KeyValuePair<string, int[]> sideSideQuest in Quest.SideSideQuestsProgress)
            {
                string status = string.Concat(Enumerable.Repeat(star, sideSideQuest.Value[0])) +
                    string.Concat(Enumerable.Repeat(cross, sideSideQuest.Value[1] - sideSideQuest.Value[0]));
                message = message.Replace($"[{sideSideQuest.Key}]", status);
            }

            SendMessage(message: message, pin: true);
        }

        public static async void SendMessage(TgMessage message, long to = EUGENE_ID, bool pin = false)
        {
            Message sentMessage = new();

            switch (message.Type)
            {
                case TgMessageType.Geotag:
                    await BotClient.SendChatActionAsync(to, ChatAction.FindLocation);
                    sentMessage = await BotClient.SendLocationAsync(
                        chatId: to,
                        latitude: (float)message.Geotag.Latitude,
                        longitude: (float)message.Geotag.Longitude,
                        replyMarkup: message.KeyboardMarkup
                    );
                    break;
                case TgMessageType.Image:
                    await BotClient.SendChatActionAsync(to, ChatAction.UploadPhoto);
                    using (var stream = System.IO.File.OpenRead(message.Image))
                    {
                        sentMessage = await BotClient.SendPhotoAsync(
                        chatId: to,
                        photo: stream,
                        caption: message.Text,
                        replyMarkup: message.KeyboardMarkup
                        );
                    }
                    break;
                case TgMessageType.Sticker:
                    await BotClient.SendChatActionAsync(to, ChatAction.Typing);
                    using (var stream = System.IO.File.OpenRead(message.Sticker))
                    {
                        sentMessage = await BotClient.SendStickerAsync(
                        chatId: to,
                        sticker: stream,
                        replyMarkup: message.KeyboardMarkup
                        );
                    }
                    break;
                case TgMessageType.Voice:
                    await BotClient.SendChatActionAsync(to, ChatAction.RecordVoice);
                    using (var stream = System.IO.File.OpenRead(message.Voice))
                    {
                        sentMessage = await BotClient.SendVoiceAsync(
                        chatId: to,
                        voice: stream,
                        replyMarkup: message.KeyboardMarkup
                        );
                    }
                    break;
                case TgMessageType.Video:
                    await BotClient.SendChatActionAsync(to, ChatAction.UploadVideo);
                    using (var stream = System.IO.File.OpenRead(message.Video))
                    {
                        sentMessage = await BotClient.SendVideoAsync(
                            chatId: EUGENE_ID,
                            video: stream,
                            caption: message.Text,
                            supportsStreaming: true,
                            replyMarkup: message.KeyboardMarkup
                        );
                    }
                    break;
                case TgMessageType.Quiz:
                    await BotClient.SendChatActionAsync(to, ChatAction.Typing);
                    sentMessage = await BotClient.SendPollAsync(
                        chatId: to,
                        question: message.Quiz.Question,
                        options: message.Quiz.Options,
                        type: PollType.Quiz,
                        correctOptionId: message.Quiz.CorrectOptionId,
                        replyMarkup: message.KeyboardMarkup
                    );
                    message.Quiz.PollId = sentMessage.Poll.Id;
                    break;
                case TgMessageType.Buttons:
                    await BotClient.SendChatActionAsync(to, ChatAction.Typing);
                    sentMessage = await BotClient.SendTextMessageAsync(
                        chatId: to,
                        text: message.Text,
                        parseMode: ParseMode.Html,
                        replyMarkup: message.InlineKeyboard);
                    break;
                case TgMessageType.Text:
                    await BotClient.SendChatActionAsync(to, ChatAction.Typing);
                    sentMessage = await BotClient.SendTextMessageAsync(
                        chatId: to,
                        text: message.Text,
                        parseMode: ParseMode.Html,
                        replyMarkup: message.KeyboardMarkup
                    );
                    break;
            }
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Sent \"{message.Name}\"");

            // Pinning messages
            if (pin)
            {
                await BotClient.UnpinAllChatMessages(EUGENE_ID);
                await BotClient.PinChatMessageAsync(EUGENE_ID, sentMessage.MessageId, true);
            }

            // Wiretap a.k.a. "Babysitting"
            if (to == EUGENE_ID)
            {
                foreach (long admin in Admins)
                {
                    await BotClient.ForwardMessageAsync(
                        chatId: admin,
                        fromChatId: sentMessage.Chat.Id,
                        messageId: sentMessage.MessageId
                        );
                }
            }

            // Sending message sequences
            if (message.NextAfter != null)
            {
                Thread.Sleep((int)message.NextAfter * 1000);
                SendMessage(message.NextMessage);
            }
        }

        public static void SendMessage(string message, long to = EUGENE_ID, bool pin = false)
        {
            TgMessage tgMessage = new();
            tgMessage.Name = "manual_message";
            tgMessage.Text = message;
            SendMessage(tgMessage, to, pin);
        }

        public static void SendMessageManually(string messageName)
        {
            SendMessage(Quest.Messages.Single(m => m.Name == messageName));
        }
        #endregion

        #region Receiving
        private static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - {ErrorMessage}");
            return Task.CompletedTask;
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                    case UpdateType.EditedMessage:
                        BotOnMessageReceived(update.Message);
                        break;
                    case UpdateType.CallbackQuery:
                        BotOnCallbackQueryReceived(update.CallbackQuery);
                        break;
                    case UpdateType.Poll:
                        BotOnPollAnswerReceived(update.Poll);
                        break;
                }
            }
            catch (Exception e)
            {
                await HandleErrorAsync(BotClient, e, cancellationToken);
            }
        }

        private static void BotOnMessageReceived(Message message)
        {
            if (message == null) return;

            // Another wiretap
            if (message.From.Id == EUGENE_ID)
            {
                foreach (long admin in Admins)
                {
                    BotClient.ForwardMessageAsync(
                        chatId: admin,
                        fromChatId: message.Chat.Id,
                        messageId: message.MessageId
                        );
                }
            }

            switch (message.Type)
            {
                case MessageType.Text:
                    BotOnTextReceived(message);
                    break;
                case MessageType.Location:
                    BotOnLocationReceived(message);
                    break;
                default:
                    return;
            }
        }

        private static void BotOnTextReceived(Message message)
        {
            if (new List<long>(Admins).Contains(message.From.Id))
            {
                if (message.Text.StartsWith("/send"))
                {
                    SendMessage(Quest.Messages.Single(m => m.Name == message.Text[6..]));
                }
                else if (message.Text.StartsWith("/forward"))
                {
                    SendMessage(message.Text[9..]);
                }
                else if (message.Text.StartsWith("/progress"))
                {
                    Quest.MainQuestProgress = int.Parse(message.Text[10..]);
                    UpdateQuestsStatusAsync();
                }
                else if (message.Text.StartsWith("квест"))
                {
                    Quest.SideSideQuestsProgress[message.Text[6..]][0]++;
                    UpdateQuestsStatusAsync();
                }
                else if (message.Text.StartsWith("муз квиз раунд 1"))
                {
                    SendMessage(Quest.Messages.Single(m => m.Name == "quiz_1_welcome"));
                }
                else if (message.Text.StartsWith("муз квиз раунд 2"))
                {
                    SendMessage(Quest.Messages.Single(m => m.Name == "quiz_16_round_2_welcome"));
                }
                else if (message.Text.StartsWith("муз квиз раунд 3"))
                {
                    SendMessage(Quest.Messages.Single(m => m.Name == "quiz_37_round_3_welcome"));
                }
                else if (message.Text.StartsWith("Как пользоваться админкой?"))
                {
                    SendAdminUsage(message.From.Id);
                }
            }

            // Handling codewords
            if (Quest.Messages.Where(m => m.SendOnText == message.Text).Any())
            {
                SendMessage(Quest.Messages.Single(m => m.SendOnText == message.Text));
            }
        }

        private static void BotOnLocationReceived(Message message)
        {
            var eugeneCoordinate = new GeoCoordinate(message.Location.Latitude, message.Location.Longitude);
            foreach (TgMessage tgMessage in Quest.Messages.Where(m => m.SendAtLocation != null))
            {
                var distanceInMeters = (int)eugeneCoordinate.GetDistanceTo(tgMessage.SendAtLocation.GetGeoCoordinate);
                if (distanceInMeters < 100)
                {
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Location proximity {distanceInMeters} meters");
                    SendMessage(tgMessage);
                    return;
                }
            }
        }

        private static void BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            SendMessage(Quest.Messages.Single(m => m.Name == callbackQuery.Data));
        }

        private static void BotOnPollAnswerReceived(Poll poll)
        {
            foreach (TgMessage message in Quest.Messages.Where(m => m.Quiz != null && m.Quiz.PollId == poll.Id))
            {
                SendMessage(Quest.Messages.Single(m => m.Name == message.Quiz.ReplyName));
            }
        }
        #endregion
    }
}
