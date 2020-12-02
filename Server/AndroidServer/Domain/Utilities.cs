using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Domain
{
    /// <summary>
    /// Utilities specifically useful to Android
    /// </summary>
    public struct Utilities
    {
        private static readonly Random rand = new();

        /// <summary>
        /// Send a text file as a Discord message
        /// </summary>
        public static async Task SendTextAsFile(IMessageChannel channel, string data, string filename = "message.txt")
        {
            Encoding encoder = new UTF8Encoding();
            var bytes = encoder.GetBytes(data);
            MemoryStream stream = new MemoryStream(bytes);
            await channel.SendFileAsync(stream, filename);
        }

        /// <summary>
        /// Simple HTTP get request
        /// </summary>
        public static async Task<T> HttpGet<T>(string uri)
        {
            using HttpClient http = new HttpClient();
            var body = await http.GetStringAsync(uri);
            var tree = JsonConvert.DeserializeObject<T>(body);
            return tree;
        }

        /// <summary>
        /// Pick random entry from an <see cref="IList{T}"/>
        /// </summary>
        public static T PickRandom<T>(IList<T> collection)
        {
            return collection[rand.Next(0, collection.Count)];
        }

        /// <summary>
        /// Get a <see cref="TimeSpan"/> from a string. Supports any whole number followed by second, minute, hour, day, week, or year
        /// </summary>
        /// <returns>The parsed time duration, or null when none is found</returns>
        public static TimeSpan? ParseTimeFromText(string text)
        {
            var duration = TimeSpan.Zero;

            var match = Regex.Match(text, @"(\d+)\s?((second)|(minute)|(hour)|(day)|(week)|(year))s?");
            if (match.Success)
            {
                try
                {
                    int parsedNumber = int.Parse(new string(match.Value.Where(c => char.IsDigit(c)).ToArray()));

                    if (match.Value.Contains("second"))
                        duration = TimeSpan.FromSeconds(parsedNumber);
                    else if (match.Value.Contains("minute"))
                        duration = TimeSpan.FromMinutes(parsedNumber);
                    else if (match.Value.Contains("hour"))
                        duration = TimeSpan.FromHours(parsedNumber);
                    else if (match.Value.Contains("day"))
                        duration = TimeSpan.FromDays(parsedNumber);
                    else if (match.Value.Contains("week"))
                        duration = TimeSpan.FromDays(parsedNumber * 7);
                    else if (match.Value.Contains("year"))
                        duration = TimeSpan.FromDays(parsedNumber * 365);
                    else
                        duration = TimeSpan.FromMinutes(parsedNumber);

                    return duration;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else return null;
        }
    }
}
