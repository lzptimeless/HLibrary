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
        private string _id;
        private IHBook _book;
        private HttpClient _httpClient;
        private HBookHeaderSetting _headerSetting;
        private string _coverUrl;
        private string[] _thumbnailUrls;
        private string[] _pageUrls;
        #endregion

        public async Task<IHBook> DownloadAsync(string id, string savePath)
        {
            if (_httpClient != null)
                throw new ApplicationException("One book is processing");

            _id = id;
            _httpClient = new HttpClient();

            Output.Print($"Download book gallery: {id}");
            string galleryHtml = await Retry(3, () => DownloadHtml($"galleries/{id}.html", null));
            await Task.Run(() => ParseGallery(galleryHtml));

            Output.Print($"Download book page html: {id}");
            string pageHtml = await Retry(3, () => DownloadHtml($"reader/{_id}.html", "1"));
            await Task.Run(() => ParsePage(pageHtml));

            string dir = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            _book = new HBook(savePath, HBookMode.Create);
            await _book.InitAsync();
            await _book.SetHeaderAsync(_headerSetting);

            if (!string.IsNullOrEmpty(_coverUrl))
            {
                Output.Print("Download cover...");
                using (var s = await Retry(3, () => DownloadImage(_coverUrl)))
                {
                    if (s != null) await _book.SetCoverAsync(s, s);
                }
            }
            else Output.Print("Cover url is empty.");

            if (_thumbnailUrls != null)
            {
                for (int i = 0; i < _thumbnailUrls.Length; i++)
                {
                    string thumburl = _thumbnailUrls[i];
                    string pageurl = _pageUrls[i];
                    Output.Print("Download page thumbnail:" + i);
                    using (var thumbS = await Retry(3, () => DownloadImage(thumburl)))
                    {
                        Stream pageS = null;
                        if (!string.IsNullOrEmpty(pageurl))
                        {
                            Output.Print("Download page:" + i);
                            pageS = await Retry(3, () => DownloadImage(pageurl));
                        }
                        else Output.Print($"Page url is null: {i}");

                        try
                        {
                            HPageHeaderSetting pageHeader = new HPageHeaderSetting();
                            if (_headerSetting != null && _headerSetting.Artists != null && _headerSetting.Artists.Length == 1)
                            {
                                pageHeader.Artist = _headerSetting.Artists[0];
                                pageHeader.Selected = pageHeader.Selected | HPageHeaderFieldSelections.Artist;
                            }
                            await _book.AddPageAsync(pageHeader, thumbS, pageS);
                        }
                        finally
                        {
                            if (pageS != null) pageS.Dispose();
                        }
                    }
                }
            }
            else Output.Print("Page url is empty.");

            Output.Print("Download complete.");
            return _book;
        }

        private async Task<string> DownloadHtml(string url, string fragment)
        {
            using (var req = CreateRequest(url, fragment, HttpMethod.Get, null, null))
            {
                using (var res = await _httpClient.SendAsync(req))
                {
                    string html = await res.Content.ReadAsStringAsync();
                    return html;
                }
            }
        }

        private async Task<Stream> DownloadImage(string url)
        {
            using (var req = CreateRequest(url, null, HttpMethod.Get, null, null))
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

        private void ParsePage(string content)
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

            string xhtml = xhtmlSB.ToString();
            XDocument xDoc = XDocument.Parse(xhtml);
            var xbody = xDoc.Element("html").Element("body");
            string[] gpageurls = xbody.Elements().Where(x => x.Name == "div" && x.Attribute("class") != null && x.Attribute("class").Value == "img-url").Select(x => x.Value.Trim()).ToArray();

            _pageUrls = new string[_thumbnailUrls.Length];
            for (int i = 0; i < _thumbnailUrls.Length; i++)
            {
                string thumburl = _thumbnailUrls[i];
                string thumbname = thumburl.Substring(thumburl.LastIndexOf('/') + 1);
                thumbname = thumbname.Substring(0, thumbname.IndexOf('.'));// 因为有些文件名的后缀重复，这里过滤掉后缀
                string gpageurl = gpageurls.FirstOrDefault(s => s.Contains($"/{thumbname}."));
                _pageUrls[i] = !string.IsNullOrEmpty(gpageurl) ? UrlFromUrl(gpageurl, null) : null;
            }
        }

        private string UrlFromUrl(string url, string basedomain)
        {
            Regex r = new Regex(@"//..?\.hitomi\.la/");
            return r.Replace(url, $"//{SubdomainFromUrl(url, basedomain)}.hitomi.la/");
        }

        private string SubdomainFromUrl(string url, string basedomain)
        {
            string ret = "a";
            if (!string.IsNullOrEmpty(basedomain)) ret = basedomain;

            Regex r = new Regex(@"/\d*(?<gid>\d)/");
            var m = r.Match(url);
            if (!m.Success) return ret;

            int gid;
            if (!int.TryParse(m.Groups["gid"].Value, out gid)) return ret;

            if (gid == 1) gid = 0;

            return SubdomainFromGalleryID(gid) + ret;
        }

        private string SubdomainFromGalleryID(int id)
        {
            bool adapose = false;
            int number_of_frontends = 2;

            if (adapose) return "0";

            int o = id % number_of_frontends;
            return char.ConvertFromUtf32(97 + o);
        }

        private void ParseGallery(string content)
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
            string xhtml = xhtmlSB.ToString();
            XDocument xDoc = XDocument.Parse(xhtml);
            var xbody = xDoc.Descendants().Where(x => x.Name == "body").First();
            var xcontainer = xbody.Elements().Where(x => x.Name == "div" && x.Attribute("class").Value == "container").First();
            var xcontent = xcontainer.Elements().Where(x => x.Name == "div" && x.Attribute("class").Value == "content").First();

            var xcovercloumn = xcontent.Elements().Where(x => x.Name == "div" && x.Attribute("class").Value == "cover-column").First();
            var xcover = xcovercloumn.Elements().Where(x => x.Name == "div" && x.Attribute("class").Value == "cover").First();
            _coverUrl = xcover.Descendants().Where(x => x.Name == "img").Select(i => i.Attribute("src").Value).First().Trim();

            var xgallery = xcontent.Elements().Where(x => x.Name == "div" && x.Attribute("class").Value.Contains("manga-gallery")).First();

            string[] booknames = xgallery.Element("h1").Element("a").Value.Split(new[] { " | " }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
            if (booknames.Length > 0)
            {
                headerSetting.Names = booknames;
                headerSetting.Selected = headerSetting.Selected | HBookHeaderFieldSelections.Names;
            }

            var artists = xgallery.Element("h2").Descendants().Where(x => x.Name == "a").Select(a => a.Value.Trim()).ToArray();
            if (artists.Length > 0)
            {
                headerSetting.Artists = artists;
                headerSetting.Selected = headerSetting.Selected | HBookHeaderFieldSelections.Artists;
            }

            var xgalleryinfo = xgallery.Elements().Where(x => x.Name == "div" && x.Attribute("class").Value == "gallery-info").First();
            var xtrs = xgalleryinfo.Descendants().Where(x => x.Name == "tr");

            foreach (var xtr in xtrs)
            {
                var xtds = xtr.Elements().ToArray();
                if (xtds.Length != 2) continue;

                string key = xtds[0].Value.Trim().ToLowerInvariant();
                string[] values = xtds[1].Descendants().Where(x => x.Name == "a").Select(a => a.Value.Trim().Trim(new[] { '♀', '♂' }).Trim()).ToArray();

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
                            headerSetting.Selected = headerSetting.Selected | HBookHeaderFieldSelections.IetfLanguageTag;
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

        private static HttpRequestMessage CreateRequest(string path, string fragment, HttpMethod method, Dictionary<string, object> urlParams, Dictionary<string, object> bodyParams)
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

            if (!string.IsNullOrEmpty(fragment)) uriBd.Fragment = fragment;

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
