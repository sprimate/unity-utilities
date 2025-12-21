using System.Collections.Generic;
using HitTrax.CoreUtilities;
using UnityEngine;

namespace HitTrax.UnityUtilities
{
    public static class StringUtilities
    {

        private static string[] Tags => new[]{
            "<color=red>",
            "<color=green>",
            "<color=yellow>",
            "<color=blue>",
            "<color=magenta>",
            "<color=cyan>",
            "<color=orange>",
            "<color=purple>",
            "</color>",
            "<b>",
            "</b>"
        };

        public static string Todo() => "TODO:".B().Orange();

        public static string AsTodo(this string message) => $"{Todo()} {message}";

        // Move to logger
        public static string TodoLog(this string title, string message) => $"{Todo().B()} {title.B()} - {message}";

        public static string Yes() => "Yes".Green();

        public static string No() => "No".Red();

        [HideInCallstack]
        public static string AddActionTag(this string message) => $"{ActionTag()} {message}".LogThis();

        public static string ActionTag() => "Action:".B().Cyan();

        public static string Black(this string text) => $"<color=black>{text}</color>";

        public static string White(this string text) => $"<color=white>{text}</color>";

        public static string Red(this string text) => $"<color=red>{text}</color>";

        public static string Green(this string text) => $"<color=green>{text}</color>";

        public static string Blue(this string text) => $"<color=blue>{text}</color>";

        public static string Yellow(this string text) => $"<color=yellow>{text}</color>";

        public static string Purple(this string text) => $"<color=purple>{text}</color>";

        public static string Orange(this string text) => $"<color=orange>{text}</color>";

        public static string Cyan(this string text) => $"<color=cyan>{text}</color>";

        public static string Magenta(this string text) => $"<color=magenta>{text}</color>";

        public static string B(this string text) => $"<b>{text}</b>";

        public static string I(this string text) => $"<i>{text}</i>";

        public static string Space(this string message) => message + " ";

        public static string NL(this string message) => message + "\n";

        public static string Append(this string message, string append) => message + append;

        public static string Size(this string text, float size) => $"<size={size}>{text}</size>";

        public static string ListStrings(params string[] items)
            => items.ListStrings();

        public static string ListStrings(this IEnumerable<string> items)
        {
            string result = "";
            foreach (var item in items)
            {
                result += item.NL();
            }

            return result;
        }

        public static string ListLastStrings(this List<string> items, int total)
        {

            total = total > items.Count ? items.Count : total;
            int startIndex = items.Count - total;
            startIndex = startIndex >= items.Count ? items.Count - 1 : startIndex;
            if (startIndex < 0)
            {
                return "";
            }

            string result = "";

            for (int i = startIndex; i < items.Count; i++)
            {
                result += items[i].NL();
            }

            return result;
        }

        public static string StripTags(this string str)
        {
            foreach (var tag in Tags)
            {
                str = str.Replace(tag, "");
            }

            return str;
        }
    }
}
