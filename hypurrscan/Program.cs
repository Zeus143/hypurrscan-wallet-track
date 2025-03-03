using Leaf.xNet;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hypurrscan
{
    internal class Program
    {
        static string old;
        static string guncel;
        // Telegram bot configuration
        static string telegramBotToken = "<BOT:TOKEN>"; // Replace with your bot token
        static string telegramChatId = "<ChatID>"; // Replace with your chat ID

        static void Main(string[] args)
        {
            //SendTelegramMessage("test");
            Console.Write("Telegram Bot Token: ");
            telegramBotToken = Console.ReadLine();
            Console.Write("Chat ID: ");
            telegramChatId = Console.ReadLine();
            Console.WriteLine("Starting HypurrScan monitor with Telegram notifications...");
            old = Req().Result;
            Console.WriteLine("Initial coin: " + old);

            while (true)
            {
                try
                {
                    guncel = Req().Result;
                    if (old != guncel)
                    {
                        string message = "New Coin Detected: " + guncel + " (Previous: " + old + ")";
                        Console.WriteLine(message);

                        // Send Telegram notification
                        SendTelegramMessage(message).Wait();

                        old = guncel;
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in main loop: " + ex.Message);
                    Thread.Sleep(5000); // Wait a bit longer if there's an error
                }
            }
        }

        public static async Task<string> Req()
        {
            try
            {
                using (HttpRequest req = new HttpRequest())
                {
                    req.AddHeader("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                    req.AddHeader("accept-language", "en,tr-TR;q=0.9,tr;q=0.8,en-US;q=0.7");
                    req.AddHeader("cache-control", "max-age=0");
                    //req.AddHeader("if-modified-since", "Mon, 24 Feb 2025 17:04:45 GMT");
                    //req.AddHeader("if-none-match", "W/\"67bca6ad-245\"");
                    req.AddHeader("priority", "u=0, i");
                    req.AddHeader("sec-ch-ua", "\"Not(A:Brand\";v=\"99\", \"Google Chrome\";v=\"133\", \"Chromium\";v=\"133\"");
                    req.AddHeader("sec-ch-ua-mobile", "?0");
                    req.AddHeader("sec-ch-ua-platform", "\"Windows\"");
                    req.AddHeader("sec-fetch-dest", "document");
                    req.AddHeader("sec-fetch-mode", "navigate");
                    req.AddHeader("sec-fetch-site", "same-origin");
                    req.AddHeader("sec-fetch-user", "?1");
                    req.AddHeader("upgrade-insecure-requests", "1");
                    req.IgnoreProtocolErrors = true;
                    req.AddHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Safari/537.36");
                    var response = req.Post("https://api.hyperliquid.xyz/info", "{\"type\":\"clearinghouseState\",\"user\":\"0xe4d31c2541a9ce596419879b1a46ffc7cd202c62\"}", "application/json").ToString();
                    //Console.WriteLine(response);
                    return response.Substring("coin\":\"", "\"");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in request: " + ex.Message);
                Thread.Sleep(3000); // Wait before retrying
                return Req().Result;
            }
        }

        public static async Task SendTelegramMessage(string message)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"https://api.telegram.org/bot{telegramBotToken}/sendMessage";

                    var content = new System.Net.Http.StringContent(
                        $"{{\"chat_id\":\"{telegramChatId}\",\"text\":\"{message}\",\"parse_mode\":\"HTML\"}}",
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await client.PostAsync(url, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to send Telegram message: {responseString}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending Telegram message: {ex.Message}");
            }
        }
    }

    // Extension method for string substring between two strings
    public static class StringExtensions
    {
        public static string Substring(this string source, string from, string to)
        {
            int fromIndex = source.IndexOf(from);
            if (fromIndex == -1)
                return string.Empty;

            fromIndex += from.Length;

            int toIndex = source.IndexOf(to, fromIndex);
            if (toIndex == -1)
                return string.Empty;

            return source.Substring(fromIndex, toIndex - fromIndex);
        }
    }
}