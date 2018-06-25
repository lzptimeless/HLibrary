using H.Book;
using System;
using System.Collections.Generic;
using System.IO;
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
        private HBookHeaderSetting _headerSetting;
        private string _coverUrl;
        private string[] _thumbnailUrls;
        #endregion

        public async void Download(string id, string savePath)
        {
            if (_httpClient != null)
                throw new ApplicationException("One book is processing");

            _httpClient = new HttpClient();

            string pageContent = await Retry(3, () => GetBookPage(id));
            await Task.Run(() => ProcessBookPage(pageContent));

            string dir = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            _book = new HBook(savePath, HBookMode.Create);
            await _book.InitAsync();
            await _book.SetHeaderAsync(_headerSetting);

            if (!string.IsNullOrEmpty(_coverUrl))
            {
                Console.WriteLine("Download cover...");
                using (var s = await DownloadImage(_coverUrl))
                {
                    if (s != null) await _book.SetCoverAsync(s, s);
                }
            }

            if (_thumbnailUrls != null)
            {
                int pageIndex = 0;
                foreach (string thumbUrl in _thumbnailUrls)
                {
                    Console.WriteLine("Download page thumbnail:" + pageIndex++);
                    using (var s = await DownloadImage(thumbUrl))
                    {
                        HPageHeaderSetting pageHeader = new HPageHeaderSetting();
                        if (_headerSetting != null && _headerSetting.Artists != null && _headerSetting.Artists.Length == 1)
                        {
                            pageHeader.Artist = _headerSetting.Artists[0];
                            pageHeader.Selected = pageHeader.Selected | HPageHeaderFieldSelections.Artist;
                        }
                        if (s != null) await _book.AddPageAsync(pageHeader, s, s);
                    }
                }
            }

            Console.WriteLine("Download complete.");
        }

        private async Task<Stream> DownloadImage(string url)
        {
            using (var req = CreateRequest(url, HttpMethod.Get, null, null))
            {
                using (var res = await _httpClient.SendAsync(req))
                {
                    MemoryStream ms = new MemoryStream();
                    try
                    {
                        using (var s = await res.Content.ReadAsStreamAsync())
                        {
                            await s.CopyToAsync(ms);
                        }
                        return ms;
                    }
                    catch (Exception ex)
                    {
                        ms.Dispose();
                        Console.WriteLine("DownloadImage failed:" + url + Environment.NewLine + ex.ToString());
                        throw ex;
                    }
                }
            }
        }

        private async Task<string> GetBookPage(string id)
        {
            using (var previewRequest = CreateRequest($"galleries/{id}.html", HttpMethod.Get, null, null))
            {
                using (var response = await _httpClient.SendAsync(previewRequest))
                {
                    string content = await response.Content.ReadAsStringAsync();
                    return content;
                }
            }
        }

        private void ProcessBookPage(string content)
        {
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

            HBookHeaderSetting headerSetting = new HBookHeaderSetting();
            string emptyValue = "N/A";
            string xhtml = xhtmlSB.ToString();
            XDocument xDoc = XDocument.Parse(xhtml);
            var coverPath = (from item in xDoc.Descendants()
                             where item.Name == "img" && item.HasAttributes && item.Attribute("src").Value.Contains("/bigtn/")
                             select item.Attribute("src").Value).FirstOrDefault();

            _coverUrl = coverPath;

            var bookNames = ((from item in xDoc.Descendants()
                              where item.Name == "a" && !item.HasElements && item.HasAttributes && item.Attribute("href").Value.Contains("/reader/")
                              select item.Value).FirstOrDefault() ?? string.Empty).Split(',');

            bookNames = bookNames.Where(n => !string.IsNullOrEmpty(n) && string.Equals(n, emptyValue, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (bookNames.Length > 0)
            {
                headerSetting.Names = bookNames;
                headerSetting.Selected = headerSetting.Selected | HBookHeaderFieldSelections.Names;
            }

            var artists = ((from item in xDoc.Descendants()
                            where item.Name == "a" && item.HasAttributes && item.Attribute("href").Value.Contains("/artist/")
                            select item.Value).FirstOrDefault() ?? string.Empty).Split(',');

            artists = artists.Where(a => !string.IsNullOrEmpty(a) && string.Equals(a, emptyValue, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (artists.Length > 0)
            {
                headerSetting.Artists = artists;
                headerSetting.Selected = headerSetting.Selected | HBookHeaderFieldSelections.Artists;
            }

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

                string key = cells[0].Value.Trim().ToLowerInvariant();
                string value = cells[1].Value.Trim();
                if (string.IsNullOrEmpty(value) || string.Equals(value, "N/A", StringComparison.OrdinalIgnoreCase)) continue;

                string[] values = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length == 0) continue;

                switch (key)
                {
                    case "group":
                        {
                            headerSetting.Groups = values;
                            headerSetting.Selected = headerSetting.Selected | HBookHeaderFieldSelections.Groups;
                        }
                        break;
                    case "type":
                        {
                            headerSetting.Categories = values;
                            headerSetting.Selected = headerSetting.Selected | HBookHeaderFieldSelections.Categories;
                        }
                        break;
                    case "language":
                        {
                            string ietf = LanguageHelper.LangToIETF(values[0]);
                            headerSetting.IetfLanguageTag = ietf ?? values[0];
                            headerSetting.Selected = headerSetting.Selected | HBookHeaderFieldSelections.Groups;
                        }
                        break;
                    case "series":
                        {
                            headerSetting.Series = values;
                            headerSetting.Selected = headerSetting.Selected | HBookHeaderFieldSelections.Series;
                        }
                        break;
                    case "characters":
                        {
                            headerSetting.Characters = values;
                            headerSetting.Selected = headerSetting.Selected | HBookHeaderFieldSelections.Characters;
                        }
                        break;
                    case "tags":
                        {
                            headerSetting.Tags = values;
                            headerSetting.Selected = headerSetting.Selected | HBookHeaderFieldSelections.Tags;
                        }
                        break;
                    default:
                        break;
                }
            }

            _headerSetting = headerSetting;

            Regex regex = new Regex(@"var thumbnails = \[(?<thumbnails>[\s\S]+?)\]");
            var mh = regex.Match(xhtml);
            var thumbnails = mh.Groups["thumbnails"].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim(new[] { '\'', ' ', '\n', '\r' }));
            _thumbnailUrls = thumbnails.Where(s => !string.IsNullOrEmpty(s)).ToArray();
        }

        private static HttpRequestMessage CreateRequest(string path, HttpMethod method, Dictionary<string, object> urlParams, Dictionary<string, object> bodyParams)
        {
            UriBuilder uriBd;
            if (path.StartsWith("https://"))
                uriBd = new UriBuilder(path);
            else if (path.StartsWith("//"))
                uriBd = new UriBuilder("https:" + path);
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

        private static async Task<T> Retry<T>(int retryCount, Func<Task<T>> action)
        {
            bool isRetry = false;
            T result = default(T);
            do
            {
                isRetry = false;
                try
                {
                    result = await action();
                }
                catch
                {
                    if (retryCount <= 0) throw;

                    --retryCount;
                    isRetry = true;
                }
            }
            while (isRetry);

            return result;
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
