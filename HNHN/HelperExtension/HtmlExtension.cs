using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace HNHB.HelperExtension
{
    public static class HtmlExtension
    {
        public static string GetSummary(
        this HtmlHelper htmlHelper, string html, int max)
        {
            string summaryHtml = string.Empty;

            // load our html document
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            int wordCount = 0;

            foreach (var element in htmlDoc.DocumentNode.ChildNodes)
            {
                if (element.Name == "img")
                {
                    continue;
                }
                // inner text will strip out all html, and give us plain text
                string elementText = element.InnerText;

                // we split by space to get all the words in this element
                string[] elementWords = elementText.Split(new char[] { ' ' });

                int countTmp = 0;
                foreach (var t in elementWords)
                {
                    countTmp += t.Length;
                }

                if (countTmp > max)
                {
                    element.InnerHtml = elementText.Substring(0, max);
                }

                // and if we haven't used too many words ...
                if (wordCount <= max)
                {
                    // add the *outer* HTML (which will have proper 
                    // html formatting for this fragment) to the summary
                    summaryHtml += element.OuterHtml;

                    wordCount += countTmp;
                }
                else
                {
                    break;
                }
            }

            return summaryHtml;
        }

        public static string GetImg(this HtmlHelper htmlHelper, string htmlContent)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            var img = htmlDoc.DocumentNode.ChildNodes.FindFirst("img");
            if (img != null)
            {
                img.Attributes.Remove("style");
                img.Attributes.Remove("height");
                img.Attributes.Remove("width");
                img.Attributes.Add("class", "img-responsive");
                return img.OuterHtml;
            }
            return null;
        }

        public static string GetImgData(this HtmlHelper htmlHelper, string htmlContent)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            var img = htmlDoc.DocumentNode.ChildNodes.FindFirst("img");
            if (img != null)
            {             
                return img.GetAttributeValue("src", null);
            }
            return null;
        }
    }
}