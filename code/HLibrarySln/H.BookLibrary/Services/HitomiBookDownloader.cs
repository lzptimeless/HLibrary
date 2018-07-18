using H.Book;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace H.BookLibrary
{
    public class HitomiBookDownloader
    {
        #region fields
        private AsyncOneManyLock _lock;
        private CancellationTokenSource _cts;
        private string _id;
        private string _savePath;
        private IHBook _book;
        private HttpClient _httpClient;
        private HBookHeaderSetting _headerSetting;
        private string _coverUrl;
        private string[] _thumbnailUrls;
        private string[] _pageUrls;
        #endregion

        public HitomiBookDownloader(string id, string savePath)
        {
            _id = id;
            _savePath = savePath;
            _lock = new AsyncOneManyLock();
        }

        #region events
        #region ProgressChanged
        /// <summary>
        /// 下载进度改变
        /// </summary>
        public event EventHandler<ProgressEventArgs> ProgressChanged;

        private void OnProgressChanged(ProgressEventArgs e)
        {
            Volatile.Read(ref ProgressChanged)?.Invoke(this, e);
        }
        #endregion

        #region BookCreated
        /// <summary>
        /// 已经创建下载文件
        /// </summary>
        public event EventHandler<BookEventArgs> BookCreated;

        private void OnBookCreated(BookEventArgs e)
        {
            Volatile.Read(ref BookCreated)?.Invoke(this, e);
        }
        #endregion

        #region CoverDownloaded
        /// <summary>
        /// 封面下载完成
        /// </summary>
        public event EventHandler<CoverEventArgs> CoverDownloaded;

        private void OnCoverDownloaded(CoverEventArgs e)
        {
            Volatile.Read(ref CoverDownloaded)?.Invoke(this, e);
        }
        #endregion

        #region PageDownload
        /// <summary>
        /// 一个页面下载完成
        /// </summary>
        public event EventHandler<PageEventArgs> PageDownload;

        private void OnPageDownload(PageEventArgs e)
        {
            Volatile.Read(ref PageDownload)?.Invoke(this, e);
        }
        #endregion

        #region Completed
        /// <summary>
        /// 下载完成
        /// </summary>
        public event EventHandler<BookEventArgs> Completed;

        private void OnCompleted(BookEventArgs e)
        {
            Volatile.Read(ref Completed)?.Invoke(this, e);
        }
        #endregion

        #region Failed
        /// <summary>
        /// 下载失败
        /// </summary>
        public event EventHandler<FailedEventArgs> Failed;

        private void OnFailed(FailedEventArgs e)
        {
            Volatile.Read(ref Failed)?.Invoke(this, e);
        }
        #endregion
        #endregion

        public async Task<IHBook> DownloadAsync()
        {
            await _lock.WaitAsync(true);
            try
            {
                if (_httpClient != null)
                    throw new ApplicationException("One book is processing");

                _httpClient = new HttpClient();
                _cts = new CancellationTokenSource();

                Output.Print($"Download book gallery: {_id}");
                string galleryHtml = await Retry(3, _cts.Token, () => DownloadHtml($"galleries/{_id}.html", null, _cts.Token));
                await Task.Run(() => ParseGallery(galleryHtml));
                _cts.Token.ThrowIfCancellationRequested();

                Output.Print($"Download book page html: {_id}");
                string pageHtml = await Retry(3, _cts.Token, () => DownloadHtml($"reader/{_id}.html", "1", _cts.Token));
                await Task.Run(() => ParsePage(pageHtml));
                _cts.Token.ThrowIfCancellationRequested();

                string dir = Path.GetDirectoryName(_savePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                int maxPageCount = 1024 * 255;
                if (_thumbnailUrls.Length > maxPageCount) throw new ApplicationException($"Page is to many:value={_thumbnailUrls.Length}, range=[0,{maxPageCount}]");

                byte pageHeaderListCount = (byte)Math.Ceiling((double)_thumbnailUrls.Length / 1024);
                _book = new HBook(_savePath, HBookMode.Create, HBookAccess.All, pageHeaderListCount);
                _cts.Token.ThrowIfCancellationRequested();
                var bookHeader = await _book.SetHeaderAsync(_headerSetting);
                _cts.Token.ThrowIfCancellationRequested();

                int progressMax = 1 + 1 + _thumbnailUrls.Length;// 头 + 封面 + 页面
                int progressValue = 1;
                OnProgressChanged(new ProgressEventArgs(progressMax, progressValue));
                OnBookCreated(new BookEventArgs(_book, bookHeader));

                await DownloadCoverAsync(_cts.Token);
                OnProgressChanged(new ProgressEventArgs(progressMax, ++progressValue));

                if (_thumbnailUrls != null)
                {
                    for (int i = 0; i < _thumbnailUrls.Length; i++)
                    {
                        string thumburl = _thumbnailUrls[i];
                        string pageurl = _pageUrls[i];
                        if (!string.IsNullOrEmpty(pageurl))
                        {
                            _cts.Token.ThrowIfCancellationRequested();
                            await DownloadPageAsync(thumburl, pageurl, i, _cts.Token);
                        }
                        else
                            Output.Print($"Page url is null, skip this page: index={i}");

                        OnProgressChanged(new ProgressEventArgs(progressMax, ++progressValue));
                    }
                }
                else Output.Print("Page url is empty.");

                if (progressMax != progressValue)
                    OnProgressChanged(new ProgressEventArgs(progressMax, progressMax));

                Output.Print("Download complete.");
                OnCompleted(new BookEventArgs(_book, bookHeader));
                return _book;
            }
            catch(Exception ex)
            {
                _book?.Dispose();
                OnFailed(new FailedEventArgs(ex));
                throw;
            }
            finally
            {
                _lock.Release();
                _cts?.Dispose();
                _httpClient?.Dispose();
            }
        }

        public async Task CancelAsync()
        {
            await _lock.WaitAsync(true);
            try
            {
                if (_cts != null && !_cts.IsCancellationRequested) _cts.Cancel();
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task DownloadPageAsync(string thumburl, string pageurl, int index, CancellationToken ct)
        {
            Output.Print("Download page thumbnail:" + index);
            BookImageShrinkResult thumbRes = null, contentRes = null;

            try
            {
                await Retry(6, _cts.Token, async () =>
                {
                    var s = await DownloadImage(thumburl, ct);
                    try
                    {
                        ct.ThrowIfCancellationRequested();
                        thumbRes = BookImageHelper.Shrink(s, 400, 400);
                        if (s != thumbRes.Stream) s.Dispose();
                    }
                    catch
                    {
                        s.Dispose();
                        if (thumbRes != null) thumbRes.Stream?.Dispose();
                        throw;
                    }
                });
                Output.Print("Download page:" + index);
                await Retry(6, _cts.Token, async () =>
                {

                    var s = await DownloadImage(pageurl, ct);
                    try
                    {
                        ct.ThrowIfCancellationRequested();
                        contentRes = BookImageHelper.Shrink(s, 1920, 1920);
                        if (s != contentRes.Stream) s.Dispose();
                    }
                    catch
                    {
                        s.Dispose();
                        if (contentRes != null) contentRes.Stream?.Dispose();
                        throw;
                    }
                });

                HPageHeaderSetting pageHeader = new HPageHeaderSetting();
                if (_headerSetting != null && _headerSetting.Artists != null && _headerSetting.Artists.Length == 1)
                {
                    pageHeader.Artist = _headerSetting.Artists[0];
                    pageHeader.Selected = pageHeader.Selected | HPageHeaderFieldSelections.Artist;
                }
                var pageHeaderRes = await _book.AddPageAsync(pageHeader, thumbRes.Stream, contentRes.Stream);
                OnPageDownload(new PageEventArgs(_book, pageHeaderRes, thumbRes.ImageSource, contentRes.ImageSource));
            }
            finally
            {
                if (thumbRes != null) thumbRes.Stream?.Dispose();
                if (contentRes != null) contentRes.Stream?.Dispose();
            }
        }

        private async Task DownloadCoverAsync(CancellationToken ct)
        {
            if (!string.IsNullOrEmpty(_coverUrl))
            {
                Output.Print("Download cover...");
                BookImageShrinkResult thumbRes = null, coverRes = null;
                try
                {
                    await Retry(6, ct, async () =>
                    {
                        var s = await DownloadImage(_coverUrl, ct);
                        try
                        {
                            ct.ThrowIfCancellationRequested();
                            coverRes = BookImageHelper.Shrink(s, 1920, 1920);
                            thumbRes = BookImageHelper.Shrink(s, 400, 400);

                            if (s != coverRes.Stream && s != thumbRes.Stream) s.Dispose();
                        }
                        catch
                        {
                            s.Dispose();
                            if (coverRes != null) coverRes.Stream?.Dispose();
                            if (thumbRes != null) thumbRes.Stream?.Dispose();
                            throw;
                        }
                    });

                    await _book.SetCoverAsync(thumbRes.Stream, coverRes.Stream);
                    OnCoverDownloaded(new CoverEventArgs(thumbRes.ImageSource, coverRes.ImageSource));
                }
                finally
                {
                    if (thumbRes != null) thumbRes.Stream?.Dispose();
                    if (coverRes != null) coverRes.Stream?.Dispose();
                }
            }
            else Output.Print("Cover url is empty.");
        }

        private async Task<string> DownloadHtml(string url, string fragment, CancellationToken ct)
        {
            using (var req = CreateRequest(url, fragment, HttpMethod.Get, null, null))
            {
                using (var res = await _httpClient.SendAsync(req, ct))
                {
                    string html = await res.Content.ReadAsStringAsync();
                    return html;
                }
            }
        }

        private async Task<Stream> DownloadImage(string url, CancellationToken ct)
        {
            using (var req = CreateRequest(url, null, HttpMethod.Get, null, null))
            {
                using (var res = await _httpClient.SendAsync(req, ct))
                {
                    MemoryStream ms = new MemoryStream();
                    try
                    {
                        using (var s = await res.Content.ReadAsStreamAsync())
                        {
                            await s.CopyToAsync(ms, 2048, ct);
                        }

                        ms.Seek(0, SeekOrigin.Begin);
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

            Regex galleryClassRegex = new Regex(@"gallery\s+[\w-]*gallery\s+");
            var xgallery = xcontent.Elements().Where(x => x.Name == "div" && x.Attribute("class") != null && galleryClassRegex.IsMatch(x.Attribute("class").Value)).First();

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

        private static async Task<T> Retry<T>(int retryCount, CancellationToken ct, Func<Task<T>> action)
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
                catch (Exception ex)
                {
                    if (retryCount <= 0 || ct.IsCancellationRequested) throw;

                    --retryCount;
                    isRetry = true;

                    Output.Print($"Retry for (remain count:{retryCount}):" + Environment.NewLine + ex.ToString());
                }
            }
            while (isRetry);

            return result;
        }

        private static async Task Retry(int retryCount, CancellationToken ct, Func<Task> action)
        {
            bool isRetry = false;
            do
            {
                isRetry = false;
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    if (retryCount <= 0 || ct.IsCancellationRequested) throw;

                    --retryCount;
                    isRetry = true;

                    Output.Print($"Retry for (remain count:{retryCount}):" + Environment.NewLine + ex.ToString());
                }
            }
            while (isRetry);
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
