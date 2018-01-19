using System;
using System.Linq;
using System.Web.Script.Serialization;

namespace Mining_Station
{
    public class JsonConverter
    {
        public static string ConvertToJson(object obj)
        {
            var jss = new JavaScriptSerializer();
            jss.RecursionLimit = 255;
            var json = jss.Serialize(obj);
            return json;
        }

        public static object ConvertFromJson(string json)
        {
            var jss = new JavaScriptSerializer();
            jss.RecursionLimit = 255;
            object obj = null;
            try { obj = jss.Deserialize<object>(json); }
            catch
            {
                return null;
            }
            return obj;
        }

        public static T ConvertFromJson<T>(string json, bool showError = true)
        {
            T obj = default(T);
            var jss = new JavaScriptSerializer();
            jss.RecursionLimit = 255;
            try { obj = jss.Deserialize<T>(json); }
            catch (Exception e)
            {
                if (showError)
                    Helpers.ShowErrorMessage(e.Message, "JSON conversion error");
            }
            return obj;
        }

        private const string INDENT_STRING = "    ";
        public static string FormatJson(string json)
        {

            int indentation = 0;
            int quoteCount = 0;
            var result =
                from ch in json
                let quotes = ch == '"' ? quoteCount++ : quoteCount
                let lineBreak = ch == ',' && quotes % 2 == 0 ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, indentation)) : null
                let openChar = ch == '{' || ch == '[' ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, ++indentation)) : ch.ToString()
                let closeChar = ch == '}' || ch == ']' ? Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, --indentation)) + ch : ch.ToString()
                select lineBreak == null
                            ? openChar.Length > 1
                                ? openChar
                                : closeChar
                            : lineBreak;

            return String.Concat(result);
        }
    }
}
