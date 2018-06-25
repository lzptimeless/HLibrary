using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.BookLibrary
{
    public static class LanguageHelper
    {
        private static Dictionary<string, string> _langtoietf = new Dictionary<string, string>();
        private static Dictionary<string, string> _ietftozh = new Dictionary<string, string>();

        static LanguageHelper()
        {
            _ietftozh.Add("id", "印度");// indonesian,印度
            _ietftozh.Add("ca", "加泰罗尼亚语");// catalan,valencian,加泰罗尼亚语,瓦伦西亚语
            _ietftozh.Add("cs", "捷克语");// czech,捷克语
            _ietftozh.Add("da", "丹麦语");// danish,丹麦语
            _ietftozh.Add("de", "德语");// german,德语
            _ietftozh.Add("et", "爱沙尼亚");//estonian,爱沙尼亚
            _ietftozh.Add("en", "英语");// 英语
            _ietftozh.Add("es", "西班牙语");// spanish,castilian,西班牙语
            _ietftozh.Add("eo", "世界语");// 世界语
            _ietftozh.Add("fr", "法语");// french,法语
            _ietftozh.Add("it", "意大利语");// italian,意大利语
            _ietftozh.Add("la", "拉丁语");// latin,拉丁语 
            _ietftozh.Add("hu", "匈牙利语");// hungarian,匈牙利语
            _ietftozh.Add("nl", "荷兰语");// dutch,flemish,荷兰语
            _ietftozh.Add("no", "挪威语");// norwegian, 挪威语
            _ietftozh.Add("pl", "波兰语");// polish,波兰语
            _ietftozh.Add("pt", "葡萄牙语");// portuguese,葡萄牙语
            _ietftozh.Add("ro", "罗马尼亚语");//romanian,moldavian,moldovan,罗马尼亚语
            _ietftozh.Add("sq", "阿尔巴尼亚语");//albanian,阿尔巴尼亚语
            _ietftozh.Add("sk", "斯洛伐克语");//slovak,斯洛伐克语
            _ietftozh.Add("fi", "芬兰语");//finnish,芬兰语
            _ietftozh.Add("sv", "瑞典语");//swedish,瑞典语
            _ietftozh.Add("tl", "塔加路族人");//tagalog,塔加路族人
            _ietftozh.Add("vi", "越南语");//vietnamese,越南语
            _ietftozh.Add("tr", "土耳其语");//turkish 土耳其语
            _ietftozh.Add("el", "希腊语");//greek,希腊语
            _ietftozh.Add("ru", "俄语");//russian,俄语
            _ietftozh.Add("uk", "乌克兰语");//ukrainian,乌克兰语
            _ietftozh.Add("he", "希伯来语");//hebrew,希伯来语
            _ietftozh.Add("ar", "阿拉伯语");//arabic,阿拉伯语
            _ietftozh.Add("th", "泰语");//thai,泰语
            _ietftozh.Add("ko", "韩语");//korean,韩语
            _ietftozh.Add("zh", "中文");//chinese
            _ietftozh.Add("ja", "日语");//japanese,日语

            _langtoietf.Add("bahasa indonesia", "id");// indonesian,印度
            _langtoietf.Add("indonesian", "id");// indonesian,印度
            _langtoietf.Add("català", "ca");// catalan,valencian,加泰罗尼亚语,瓦伦西亚语
            _langtoietf.Add("catalan", "ca");// catalan,valencian,加泰罗尼亚语,瓦伦西亚语
            _langtoietf.Add("valencian", "ca");// catalan,valencian,加泰罗尼亚语,瓦伦西亚语
            _langtoietf.Add("čeština", "cs");// czech,捷克语
            _langtoietf.Add("czech", "cs");// czech,捷克语
            _langtoietf.Add("dansk", "da");// danish,丹麦语
            _langtoietf.Add("danish", "da");// danish,丹麦语
            _langtoietf.Add("deutsch", "de");// german,德语
            _langtoietf.Add("german", "de");// german,德语
            _langtoietf.Add("eesti", "et");//estonian,爱沙尼亚
            _langtoietf.Add("estonian", "et");//estonian,爱沙尼亚
            _langtoietf.Add("english", "en");// 英语
            _langtoietf.Add("español", "es");// spanish,castilian,西班牙语
            _langtoietf.Add("spanish", "es");// spanish,castilian,西班牙语
            _langtoietf.Add("castilian", "es");// spanish,castilian,西班牙语
            _langtoietf.Add("esperanto", "eo");// 世界语
            _langtoietf.Add("français", "fr");// french,法语
            _langtoietf.Add("french", "fr");// french,法语
            _langtoietf.Add("italiano", "it");// italian,意大利语
            _langtoietf.Add("italian", "it");// italian,意大利语
            _langtoietf.Add("latina", "la");// latin,拉丁语 
            _langtoietf.Add("latin", "la");// latin,拉丁语 
            _langtoietf.Add("magyar", "hu");// hungarian,匈牙利语
            _langtoietf.Add("hungarian", "hu");// hungarian,匈牙利语
            _langtoietf.Add("nederlands", "nl");// dutch,flemish,荷兰语
            _langtoietf.Add("dutch", "nl");// dutch,flemish,荷兰语
            _langtoietf.Add("flemish", "nl");// dutch,flemish,荷兰语
            _langtoietf.Add("norsk", "no");// norwegian, 挪威语
            _langtoietf.Add("norwegian", "no");// norwegian, 挪威语
            _langtoietf.Add("polski", "pl");// polish,波兰语
            _langtoietf.Add("polish", "pl");// polish,波兰语
            _langtoietf.Add("português", "pt");// portuguese,葡萄牙语
            _langtoietf.Add("portuguese", "pt");// portuguese,葡萄牙语
            _langtoietf.Add("română", "ro");//romanian,moldavian,moldovan,罗马尼亚语
            _langtoietf.Add("romanian", "ro");//romanian,moldavian,moldovan,罗马尼亚语
            _langtoietf.Add("moldavian", "ro");//romanian,moldavian,moldovan,罗马尼亚语
            _langtoietf.Add("moldovan", "ro");//romanian,moldavian,moldovan,罗马尼亚语
            _langtoietf.Add("shqip", "sq");//albanian,阿尔巴尼亚语
            _langtoietf.Add("albanian", "sq");//albanian,阿尔巴尼亚语
            _langtoietf.Add("slovenčina", "sk");//slovak,斯洛伐克语
            _langtoietf.Add("slovak", "sk");//slovak,斯洛伐克语
            _langtoietf.Add("suomi", "fi");//finnish,芬兰语
            _langtoietf.Add("finnish", "fi");//finnish,芬兰语
            _langtoietf.Add("svenska", "sv");//swedish,瑞典语
            _langtoietf.Add("swedish", "sv");//swedish,瑞典语
            _langtoietf.Add("tagalog", "tl");//tagalog,塔加路族人
            _langtoietf.Add("tiếng việt", "vi");//vietnamese,越南语
            _langtoietf.Add("vietnamese", "vi");//vietnamese,越南语
            _langtoietf.Add("türkçe", "tr");//turkish 土耳其语
            _langtoietf.Add("turkish", "tr");//turkish 土耳其语
            _langtoietf.Add("ελληνικά", "el");//greek,希腊语
            _langtoietf.Add("greek", "el");//greek,希腊语
            _langtoietf.Add("русский", "ru");//russian,俄语
            _langtoietf.Add("russian", "ru");//russian,俄语
            _langtoietf.Add("українська", "uk");//ukrainian,乌克兰语
            _langtoietf.Add("ukrainian", "uk");//ukrainian,乌克兰语
            _langtoietf.Add("עברית", "he");//hebrew,希伯来语
            _langtoietf.Add("hebrew", "he");//hebrew,希伯来语
            _langtoietf.Add("العربية", "ar");//arabic,阿拉伯语
            _langtoietf.Add("arabic", "ar");//arabic,阿拉伯语
            _langtoietf.Add("ไทย", "th");//thai,泰语
            _langtoietf.Add("thai", "th");//thai,泰语
            _langtoietf.Add("한국어", "ko");//korean,韩语
            _langtoietf.Add("korean", "ko");//korean,韩语
            _langtoietf.Add("中文", "zh");//chinese
            _langtoietf.Add("chinese", "zh");//chinese
            _langtoietf.Add("日本語", "ja");//japanese,日语
            _langtoietf.Add("japanese", "ja");//japanese,日语
        }

        public static string LangToIETF(string lang)
        {
            string l = lang.ToLowerInvariant();
            if (_langtoietf.ContainsKey(l))
                return _langtoietf[l];
            else
                return null;
        }

        public static string IETFToZh(string ietfTag)
        {
            string l = ietfTag.ToLowerInvariant();
            if (_ietftozh.ContainsKey(l))
                return _ietftozh[l];
            else
                return null;
        }

        public static string LangToZh(string lang)
        {
            string l = lang.ToLowerInvariant();
            string ietf = LangToIETF(lang);
            if (ietf == null) return null;

            return IETFToZh(ietf);
        }
    }
}
