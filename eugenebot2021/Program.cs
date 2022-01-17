using FluentScheduler;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace eugenebot2021
{
    class Program
    {
        static void Main()
        {
            // Fire everything up
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Loaded {Quest.Messages.Count} messages...");
            Bot.Initialize();
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Bot is initialized...");

            // Add scheduled messages to queue
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            var registry = new Registry();
            foreach (TgMessage message in Quest.Messages.Where(m => m.SendAt != null))
            {
                registry.Schedule(() => Bot.SendMessage(message)).ToRunOnceAt(message.SendAt.Hours, message.SendAt.Minutes);
            }
            JobManager.Initialize(registry);
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Scheduled {Quest.Messages.Where(m => m.SendAt != null).Count()} messages...");

            // Manual break
            Console.WriteLine("Press any key to stop the quest");
            Console.ReadKey();
        }
    }
}
