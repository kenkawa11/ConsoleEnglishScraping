using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using AngleSharp.Html.Parser;

namespace EnglishScraping.Models
{
    public abstract class BaseDic
    {
        public static HttpClient client = new HttpClient();
        public string Url { get; set; }
        public string Ptn { get; set; }
        protected Wdhtml DataHtml;
        protected string SymbolQuery;
        protected string SentenceQuery;
        protected string SentenceMp3Query;
        protected string SentenceMp3Attributor;
        protected string Divider;
        protected class Wdhtml
        {
            private string word;
            private string html;
            private readonly string url;
            public Wdhtml(string u)
            {
                url = u;
            }
            public async Task<string> GethtmlofWordAsync(string w, string d)
            {
                w=w.Replace("+", d);
                if (word == null || word != w)
                {
                    word = w;
                    var targeturl = url + w;
                    html = await GetHtmlAsync(targeturl);
                }
                return html;
            }
            private static async Task<string> GetHtmlAsync(string url)
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                    return await client.GetStringAsync(url);
                }
                catch (HttpRequestException)
                {
                    return "n";
                }
            }
        }
        public virtual async Task<bool> DownLoadMp3Async(string w, string outpath)
        {
            var (success, targeturl) = await TargeturlgetAsync(w);
            var url_mp3 = targeturl;
            if (!success)
            {
                return false;
            }
            GetMp3Async(url_mp3, outpath);
            return true;
        }
        protected async Task<(bool success, string targeturl)> TargeturlgetAsync(string w)
        {
            var reg = new Regex(Ptn);
            var html = await DataHtml.GethtmlofWordAsync(w,Divider);
            var m = reg.Match(html);
            return m.Success ? (true, m.Groups[0].Value) : (false, "");
        }
        protected static async void GetMp3Async(string url_mp3, string outputmp3)
        {
            HttpResponseMessage res = await client.GetAsync(url_mp3);
            using var fileStream = File.Create(outputmp3);
            using var httpStream = await res.Content.ReadAsStreamAsync();
            httpStream.CopyTo(fileStream);
            fileStream.Flush();
        }
        public async Task<string> DownLoadSymbolAsync(string w)
        {
            var reg = new Regex(Ptn);
            var m = reg.Match(await DataHtml.GethtmlofWordAsync(w,Divider));
            if (!m.Success)
            {
                return "n" ;
            }
            var symbol = m.Groups[0].Value;
            var pos = symbol.IndexOf("】");
            return symbol.Substring(pos + 1, symbol.Length - pos - 5);
        }
        public virtual async Task<string> AngleGetSymbolAsync(string w)
        {
            var parser = new HtmlParser();
            var doc = parser.ParseDocument(await DataHtml.GethtmlofWordAsync(w,Divider));
            var element = doc.QuerySelector(SymbolQuery);
            return element == null ? "n" : element.TextContent.Trim();
        }

        public virtual async Task<(string sntnc,bool isMp3)> GetEaxampleSentenceAsync(string w, string outputmp3,bool isSentenceMp3=false)
        {
            var parser = new HtmlParser();
            var doc = parser.ParseDocument(await DataHtml.GethtmlofWordAsync(w,Divider));
            var element = doc.QuerySelector(SentenceQuery);
            if (element == null)
            {
                return ("n",false);
            }

            var Sntnc = element.TextContent.Trim();

            if (SentenceMp3Query == "n")
            {
                return (Sntnc, false);
            }

            if (SentenceQuery != SentenceMp3Query)
            {
                element = doc.QuerySelector(SentenceMp3Query);
                if (element == null)
                {
                    return (Sntnc,false);
                }
            }

            if (element.ChildElementCount == 0)
            {
                return (Sntnc, false);

            }
            
            if(isSentenceMp3)
            {
                var mp3File = element.FirstElementChild.GetAttribute(SentenceMp3Attributor);

                if (mp3File == null)
                {
                    return (Sntnc, false);
                }

                GetMp3Async(mp3File, outputmp3);
                return (Sntnc, true);

            }
            else
            {
                return (Sntnc, false);
            }
        }
    }
    public class Ox : BaseDic
    {
        public Ox()
        {
            Url = "https://www.oxfordlearnersdictionaries.com/definition/english/";
            Ptn = "https://www.oxfordlearnersdictionaries.com/media/english/us_pron/.+?mp3";
            DataHtml = new Wdhtml(Url);
            SymbolQuery = ".phons_n_am";
            SentenceQuery = "ul.examples>li:nth-child(1)>span";
            SentenceMp3Query = "n";
            SentenceMp3Attributor = "n";
            Divider = "+";
        }
    }
    public class Ldo : BaseDic
    {
        public Ldo()
        {
            Url = "https://www.ldoceonline.com/jp/dictionary/";
            //Ptn = "https://d27ucmmhxk51xv.cloudfront.net/media/english/exaProns/.+?mp3";

            Ptn = "https://.+/ameProns/.+?mp3";
            DataHtml = new Wdhtml(Url);
            SymbolQuery = ".PronCodes";
            SentenceQuery = ".EXAMPLE";
            SentenceMp3Query= ".EXAMPLE";
            SentenceMp3Attributor = "data-src-mp3";
            Divider = "-";
    }

        public Ldo(string ptn1)
        {
            Url = "https://www.ldoceonline.com/jp/dictionary/";
            Ptn = ptn1;
            //Ptn = "https://d27ucmmhxk51xv.cloudfront.net/media/english/exaProns/.+?mp3";
            DataHtml = new Wdhtml(Url);
        }       

    }
    public class Webl : BaseDic
    {
        public Webl()
        {
            Url = "https://ejje.weblio.jp/content/";
            Ptn = "https://weblio.hs.llnwd.net/.+?mp3\"";
            DataHtml = new Wdhtml(Url);
            SymbolQuery = "#phoneticEjjeNavi";
            SentenceQuery = "n";
            SentenceMp3Query = "n";
            SentenceMp3Attributor = "n";
            Divider = "+";
        }

        public override async Task<bool> DownLoadMp3Async(string w, string outpath)
        {
            var (success, targeturl) = await TargeturlgetAsync(w);
            if (!success)
            {
                return false;
            }
            var url_mp3 = targeturl;
            url_mp3 = url_mp3[0..^1];
            //url_mp3 = url_mp3.Substring(0, url_mp3.Length - 1);
            GetMp3Async(url_mp3, outpath);
            return true;
        }
    }
    public class Eiji : BaseDic
    {
        public Eiji()
        {
            Url = "https://eow.alc.co.jp/";
            Ptn = "【発音】.+?【カナ】|【発音.+?】.+?【カナ】";
            DataHtml = new Wdhtml(Url);
            SymbolQuery = ".pron";
            SentenceQuery = "n";
            SentenceMp3Query = "n";
            SentenceMp3Attributor = "n";
            Divider = "+";
        }
    }
    public class Collins:BaseDic
    {
        public Collins()
        {
            Url = "https://www.collinsdictionary.com/dictionary/english/";
            Ptn = "https://www.collinsdictionary.com/sounds/.+?mp3";
            DataHtml = new Wdhtml(Url);
            SymbolQuery = "span.pron.type-";
            SentenceQuery = "span.quote";
            SentenceMp3Query = "span.ptr.exa_sound.type-exa_sound";
            SentenceMp3Attributor = "data-src-mp3";
            Divider = "-";
        }
    }
}
