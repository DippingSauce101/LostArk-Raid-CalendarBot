using System;
using System.Text.RegularExpressions;

namespace DiscordLostArkBot.Utilities
{
    public static class StringExtensions
    {
        public static bool ParseToDateTime(this string str, out DateTime parsedDateTime)
        {
            if (DateTime.TryParse(str, out var parsed))
            {
                parsedDateTime = parsed;
                return true;
            }

            parsedDateTime = DateTime.Now.AddHours(1);
            return false;
        }
        
        /// <summary>
        ///     중괄호 안에 DateTime 형식의 스트링이 있다면 RegEx로 찾아내서 파싱해 <b>UTC</b> DateTime 리턴.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="parseResult"></param>
        /// <returns></returns>
        public static bool ParseParenthesisedDateTimeFromString(this string str, out DateTime parseResult)
        {
            var parenRegEx = new Regex(@"\(([^)]*)\)");
            var parsedDateTime = DateTime.Now.AddHours(1);
            var parseSuccessed = false;

            foreach (Match match in parenRegEx.Matches(str))
            {
                var dateTimeStr = match.Value.Substring(1, match.Value.Length - 2);
                if (dateTimeStr.ParseToDateTime(out parsedDateTime))
                {
                    parseSuccessed = true;
                    break;
                }
            }

            parseResult = parsedDateTime.KstToUtc();
            return parseSuccessed;
        }

        /// <summary>
        ///     중괄호 안에 DateTime 형식의 스트링이 있다면 제거하고 리턴.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ParseTitleWithoutDateTime(this string str)
        {
            var parenRegEx = new Regex(@"\(([^)]*)\)");
            var title = str;
            foreach (Match match in parenRegEx.Matches(str))
            {
                var dateTimeStr = match.Value.Substring(1, match.Value.Length - 2);
                if (dateTimeStr.ParseToDateTime(out var parsedDateTime))
                {
                    title = title.Replace(match.Value, "");
                    break;
                }
            }

            return title;
        }
    }
}