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
        /// Replaces mentions with their canonical representation
        /// </summary>
        public static string ReplaceMentions(string input)
        {
            var res = mentions.Replace(input, (m) => m.Value[2..m.Value.IndexOf('>')]);
            return res;
        }
        private static readonly Regex mentions = new Regex(@"<.\d+?>", RegexOptions.Compiled);

        /// <summary>
        /// Turns a <see cref="TimeSpan"/> into an imprecise readable string
        /// </summary>
        public static string TimeSpanToText(TimeSpan span)
        {
            if (span.TotalDays >= 1826250)
                return "an unreasonably long time";

            //acceptable estimates
            const int daysInYear = 365;
            const int daysInMonth = 30;
            const int daysInWeek = 7;

            if (span.TotalDays >= daysInYear - 30)
            {
                int years = (int)Math.Ceiling(span.TotalDays / daysInYear);
                return years + ((years == 1) ? " year" : " years");
            }

            if (span.TotalDays >= daysInMonth - 5)
            {
                int months = (int)Math.Ceiling(span.TotalDays / daysInMonth);
                return months + ((months == 1) ? " month" : " months");
            }

            if (span.TotalDays >= daysInWeek - 1)
            {
                int weeks = (int)Math.Ceiling(span.TotalDays / daysInWeek);
                return weeks + ((weeks == 1) ? " week" : " weeks");
            }

            if (span.TotalDays >= 0.9f)
            {
                int days = (int)Math.Ceiling(span.TotalDays);
                return days + ((days == 1) ? " day" : " days");
            }

            if (span.TotalHours >= 0.9f)
            {
                int hours = (int)Math.Ceiling(span.TotalHours);
                return hours + "h";
            }

            if (span.TotalMinutes >= 1)
            {
                int mins = (int)Math.Ceiling(span.TotalMinutes);
                return mins + "m";
            }

            if (span.TotalSeconds >= 1)
            {
                int secs = (int)Math.Ceiling(span.TotalSeconds);
                return secs + "s";
            }

            return span.Milliseconds + " ms";
        }

        /// <summary>
        /// Get a <see cref="TimeSpan"/> from a string. Supports any whole number followed by second, minute, hour, day, week, or year
        /// </summary>
        /// <returns>The parsed time duration, or null when none is found</returns>
        public static TimeSpan? ParseTimeFromText(string text)
        {
            var duration = TimeSpan.Zero;

            if (Regex.Match(text, @"((forever)|(indefinitely)|(infinite))").Success)
                return TimeSpan.MaxValue;

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
