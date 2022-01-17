using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot.Types.ReplyMarkups;

namespace eugenebot2021
{
    class Quest
    {
        public static readonly List<TgMessage> Messages;
        public static int MainQuestProgress;
        public static Dictionary<string, int[]> SideSideQuestsProgress;

        static Quest()
        {
            // Load messages from json
#if DEBUG
            string jsonString = File.ReadAllText("messages_test.json");
#else
            string jsonString = File.ReadAllText("messages.json");
#endif
            JsonSerializerOptions options = new();
            options.PropertyNameCaseInsensitive = true;
            Messages = JsonSerializer.Deserialize<List<TgMessage>>(jsonString, options);

            // Setup quests progress
            MainQuestProgress = 0;
            SideSideQuestsProgress = new Dictionary<string, int[]>
            {
                ["ua_artist"] = new int[2] { 0, 1 },
                ["alice_cooper"] = new int[2] { 0, 1 },
                ["burp"] = new int[2] { 0, 2 },
                ["memes"] = new int[2] { 0, 5 },
                ["motocycles"] = new int[2] { 0, 7 },
                ["welldone"] = new int[2] { 0, 1 }
            };
        }
    }

    enum TgMessageType
    {
        Text,
        Image,
        Sticker,
        Voice,
        Geotag,
        Video,
        Quiz,
        Buttons
    }

    class Button
    {
        public string Text { get; set; }
        public string CallbackData { get; set; }
    }
    class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        [JsonIgnore]
        public GeoCoordinate GetGeoCoordinate
        {
            get
            {
                return new GeoCoordinate(Latitude, Longitude);
            }
        }
    }

    class Time
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
    }

    class Quiz
    {
        public string Question { get; set; }
        public string[] Options { get; set; }
        public int CorrectOptionId { get; set; }
        public string ReplyName { get; set; }
        [JsonIgnore]
        public string PollId { get; set; } = null;
    }

    class TgMessage
    {
        public string Name { get; set; }
        public Time SendAt { get; set; } = null;
        public Location SendAtLocation { get; set; } = null;
        public string SendOnText { get; set; } = null;
        public int? NextAfter { get; set; } = null;
        public string NextName { get; set; } = null;
        public string Text { get; set; } = null;
        public string Image { get; set; } = null;
        public string Sticker { get; set; } = null;
        public string Voice { get; set; } = null;
        public Location Geotag { get; set; } = null;
        public string Video { get; set; } = null;
        public Quiz Quiz { get; set; } = null;
        public string[] Keyboard { get; set; } = null;
        public Button[] Buttons { get; set; } = null;
        [JsonIgnore]
        public InlineKeyboardMarkup KeyboardMarkup
        {
            get
            {
                if (Keyboard == null)
                {
                    return null;
                }
                var buttons = new List<InlineKeyboardButton[]>();
                for (int i = 0; i < Keyboard.Length; i++)
                {
                    string callbackData = $"{Name}_{i}";
                    buttons.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(Keyboard[i], callbackData) });
                }
                return new InlineKeyboardMarkup(buttons);
            }
        }
        [JsonIgnore]
        public InlineKeyboardMarkup InlineKeyboard
        {
            get
            {
                if (Buttons == null)
                {
                    return null;
                }

                var buttons = new List<InlineKeyboardButton[]>();

                foreach (Button button in Buttons)
                {
                    buttons.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(button.Text, button.CallbackData) });
                }
                return new InlineKeyboardMarkup(buttons);
            }
        }
        [JsonIgnore]
        public TgMessageType Type
        {
            get
            {
                if (Geotag != null)
                {
                    return TgMessageType.Geotag;
                }
                if (Image != null)
                {
                    return TgMessageType.Image;
                }
                if (Sticker != null)
                {
                    return TgMessageType.Sticker;
                }
                if (Voice != null)
                {
                    return TgMessageType.Voice;
                }
                if (Video != null)
                {
                    return TgMessageType.Video;
                }
                if (Quiz != null)
                {
                    return TgMessageType.Quiz;
                }
                if (Buttons != null)
                {
                    return TgMessageType.Buttons;
                }
                return TgMessageType.Text;
            }
        }
        [JsonIgnore]
        public TgMessage NextMessage
        {
            get
            {
                if (NextName == null)
                {
                    return null;
                }

                return Quest.Messages.Single(m => m.Name == NextName);
            }
        }
    }
}
