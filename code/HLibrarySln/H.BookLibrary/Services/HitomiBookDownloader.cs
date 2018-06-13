using H.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace H.BookLibrary
{
    public class HitomiBookDownloader
    {
        #region fields
        private IHBook _book;
        private HttpClient _httpClient;
        #endregion

        public async void Download(string id, string savePath)
        {
            if (_book != null)
                throw new ApplicationException("One book is processing");

            //_book = new HBook(savePath, HBookMode.Create);
            _httpClient = new HttpClient();

            using (var previewRequest = CreateRequest($"galleries/{id}.html", HttpMethod.Get, null, null))
            {
                using (var response = await _httpClient.SendAsync(previewRequest))
                {
                    string content = await response.Content.ReadAsStringAsync();
                    StringBuilder xhtmlSB = new StringBuilder();
                    using (var htmlReader = new HtmlReader(content))
                    {
                        using (var htmlWriter = new HtmlWriter(xhtmlSB))
                        {
                            htmlReader.Read();
                            while (!htmlReader.EOF)
                            {
                                htmlWriter.WriteNode(htmlReader, true);
                            }
                        }
                    }
                    string xhtml = xhtmlSB.ToString();
                    XDocument xDoc = XDocument.Parse(xhtml);
                    var coverPath = (from item in xDoc.Descendants()
                                     where item.Name == "img" && item.HasAttributes && item.Attribute("src").Value.Contains("/bigtn/")
                                     select item.Attribute("src").Value).FirstOrDefault();

                    var bookName = (from item in xDoc.Descendants()
                                    where item.Name == "a" && !item.HasElements && item.HasAttributes && item.Attribute("href").Value.Contains("/reader/")
                                    select item.Value).FirstOrDefault();

                    var artist = ((from item in xDoc.Descendants()
                                   where item.Name == "a" && item.HasAttributes && item.Attribute("href").Value.Contains("/artist/")
                                   select item.Value).FirstOrDefault() ?? string.Empty).Split(',');

                    var headerInfo = (from item in xDoc.Descendants()
                                      where item.Name == "div" && item.HasAttributes && item.Attribute("class") != null && item.Attribute("class").Value.Contains("gallery-info")
                                      select item).FirstOrDefault();

                    var headerFields = from item in headerInfo.Descendants()
                                       where item.Name == "tr" && item.HasElements
                                       select item;

                    foreach (var tr in headerFields)
                    {
                        var cells = tr.Elements().ToArray();
                        if (cells.Length != 2) continue;

                        string key = cells[0].Value.Trim();
                        string value = cells[1].Value.Trim();
                        if (string.Equals(value, "N/A", StringComparison.OrdinalIgnoreCase)) continue;

                        switch (key)
                        {
                            case "Group":
                                break;
                            case "Type":
                                break;
                            case "Language":
                                break;
                            case "Series":
                                break;
                            case "Characters":
                                break;
                            case "Tags":
                                break;
                            default:
                                break;
                        }
                    }

                    Regex regex = new Regex(@"var thumbnails = \[(?<thumbnails>[\s\S]+?)\]");
                    var mh = regex.Match(xhtml);
                    var thumbnails = mh.Groups["thumbnails"].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim(new[] { '\'', ' ', '\n', '\r' }));
                    thumbnails = thumbnails.Where(s => !string.IsNullOrEmpty(s));
                }
            }
        }

        private HttpRequestMessage CreateRequest(string path, HttpMethod method, Dictionary<string, object> urlParams, Dictionary<string, object> bodyParams)
        {
            UriBuilder uriBd;
            if (path.StartsWith("//"))
                uriBd = new UriBuilder(path);
            else
            {
                uriBd = new UriBuilder("https", "hitomi.la");
                uriBd.Path = path;
            }

            if (urlParams != null)
                uriBd.Query = CreateHttpParameters(urlParams, true);

            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = uriBd.Uri;
            request.Method = method;

            if (bodyParams != null)
                request.Content = new StringContent(CreateHttpParameters(bodyParams, true), Encoding.UTF8, "application/x-www-form-urlencoded");

            request.Headers.UserAgent.ParseAdd("HBookLibrary");

            return request;
        }

        private static string CreateHttpParameters(IEnumerable<KeyValuePair<string, object>> parameters, bool isEnableUrlEncode)
        {
            StringBuilder httpParam = new StringBuilder();
            string value;
            foreach (var p in parameters)
            {
                httpParam.Append(p.Key);
                httpParam.Append('=');

                if (p.Value != null)
                    value = p.Value is string ? p.Value as string : p.Value.ToString();
                else
                    value = string.Empty;

                if (isEnableUrlEncode)
                    value = UrlEncode(value);

                httpParam.Append(value);
                httpParam.Append('&');
            }
            // Remove last '&' character
            httpParam.Remove(httpParam.Length - 1, 1);

            return httpParam.ToString();
        }

        private static string UrlEncode(string s)
        {
            const int maxLength = 32766;
            if (s == null)
                throw new ArgumentNullException("input");

            if (s.Length <= maxLength)
                return Uri.EscapeDataString(s);

            StringBuilder sb = new StringBuilder(s.Length * 2);
            int index = 0;
            while (index < s.Length)
            {
                int length = Math.Min(s.Length - index, maxLength);
                string subString = s.Substring(index, length);
                sb.Append(Uri.EscapeDataString(subString));
                index += subString.Length;
            }

            return sb.ToString();
        }
    }
}
