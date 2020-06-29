using System;
using System;
using EnglishScraping.Models;
using System.ComponentModel;
using System.Threading.Tasks;
using EnglishScraping.ViewModels.Notification;

namespace ConsoleEnglishScraping
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var basetargetdir = @"C:\Users\naobaby\Desktop\test\";
            var fn = @"C:\Users\naobaby\Desktop\test\test.csv";
            var dir = @"C:\Users\naobaby\Desktop\test\";
            while (true)
            {
                Console.WriteLine("ターゲットファイル名入力,止める場合はqを入力");
                var temp = Console.ReadLine();
                if (temp=="q")
                {
                    break;
                }
                if (temp != "")
                {
                    fn = basetargetdir + temp;
                }
                PronounceDownloader DownLoder = new PronounceDownloader();
                DownLoder.Ret.PropertyChanged += OnModelChanged;
                DownLoder.MsgFrmMdl.PropertyChanged += OnModelMessageChanged;
                var Mp3filename = "ID";
                var IsSentencemp3 = true;
                await DownLoder.TreatDataAsync(fn, dir, Mp3filename, IsSentencemp3);
                await Task.Delay(100);

            }


            static void OnModelChanged(object sender, PropertyChangedEventArgs e)
            {
                var dl = sender as ModelStatus;
                switch (e.PropertyName)
                {
                    case "progress":
                        var BarProgress = dl.Progress;
                        Console.WriteLine($"現在の進捗は{BarProgress}%");
                        break;
                    case "status":

                        var Status = dl.Status;
                        Console.WriteLine($"現在の状況は{Status}");

                        break;
                }
            }
            static void OnModelMessageChanged(object sender, PropertyChangedEventArgs e)
            {
                var MsgFrmMdl = sender as MessageFromModel;
                if ((MsgFrmMdl.Msg != null))
                {
                    Console.WriteLine("ターゲットファイルがオープンのままです。閉じてください。閉じたらreturnを押し,再度プログラムを起動してください");
                    var temp = Console.ReadLine();
                    
                    
                }
                MsgFrmMdl.Msg = null;
            }
        }

    }
}
