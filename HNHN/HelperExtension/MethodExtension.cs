using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace HNHB.HelperExtension
{
    public static class MethodExtension
    {
        public static string ToUnsign(this string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
        public static bool SearchByWord(this string s, string keyword)
        {
            return string.Concat(' ', s, ' ').Contains(string.Concat(' ', keyword, ' '));
        }

        public static string ConvertToUnSign(this string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            temp = regex.Replace(temp, String.Empty).Replace('\u0111', 'd')
                .Replace('\u0110', 'D').Replace(" ", "-").Replace(",", "").Replace("?", "").Replace(":", "-");
            while (temp.EndsWith("-"))
                temp = temp.Substring(0, temp.Length - 1);

            while (temp.IndexOf("--") >= 0)
                temp = temp.Replace("--", "-");

            temp = temp.Replace("\"", string.Empty).Replace("'", string.Empty).Replace(".", string.Empty);

            return temp;
        }

        public static string ToPhoneNumber(this string s)
        {
            if (s.Length == 10)
            {
                s = s.Substring(0, 4) + " " + s.Substring(4, 3) + " " + s.Substring(7, 3);
            }
            else if (s.Length == 11)
            {
                s = s.Substring(0, 4) + " " + s.Substring(4, 3) + " " + s.Substring(7, 4);
            }
            else
            {
                s = "Đang cập nhật";
            }
            return s;
        }

        public static string ToTimeAgo(this DateTime dt)
        {
            TimeSpan span = DateTime.Now - dt;

            if (span.Days > 365)
            {
                int years = (span.Days / 365);
                if (span.Days % 365 != 0)
                    years += 1;
                return String.Format("{0} năm trước", years);
            }

            if (span.Days > 30)
            {
                int months = (span.Days / 30);
                if (span.Days % 31 != 0)
                    months += 1;
                return String.Format("{0} tháng trước", months);
            }

            if (span.Days > 0)
                return String.Format("{0} ngày trước", span.Days);

            if (span.Hours > 0)
                return String.Format("{0} giờ trước", span.Hours);

            if (span.Minutes > 0)
                return String.Format("{0} phút trước", span.Minutes);

            if (span.Seconds > 5)
                return String.Format("{0} giây trước", span.Seconds);

            if (span.Seconds <= 5)
                return "Mới đây";

            return string.Empty;
        }
    }
}