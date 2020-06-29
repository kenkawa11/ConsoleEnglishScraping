using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnglishScraping.ViewModels.Notification;
using Utility;
using System.IO;

namespace EnglishScraping.Models
{
    public class MessageFromModel:NotificationObject
    {
        private string msg;
        private Action _done;
        public string Msg
        {
            get { return this.msg; }
            set { SetProperty(ref this.msg, value, "msg"); }
        }

        public Action Done
        {
            get { return this._done; }
            set { SetProperty(ref this._done, value, "_done"); }
        }

    }

    public class ModelStatus: NotificationObject //Raise the event when the members are changed
    {
        private string status; 
        private int progress;
        private string rpt; //Indicator for the number of download


        public string Status
        {
            get { return this.status; }
            set { SetProperty(ref this.status, value,"status"); }
        }
        public int Progress
        {
            get { return this.progress; }
            set { SetProperty(ref this.progress, value,"progress"); }
        }
        public string Rpt
        {
            get { return this.rpt; }
            set { SetProperty(ref this.rpt, value,"rpt"); }
        }



    }
    public class PronounceDownloader
    {
        private readonly string[] key = { "ID", "Jap", "EngWord", "Sound", "Symbol", "EngSentence", "JapSentence", "SntncSound" };
        private List<Dictionary<string, string>> fdata = new List<Dictionary<string, string>>();
        private string dir;
        private string rfn;
        public ModelStatus Ret = new ModelStatus();
        public MessageFromModel MsgFrmMdl = new MessageFromModel();
        public PronounceDownloader()
        {
            Ret.Progress = 0;
            Ret.Status = " ";
            Ret.Rpt = " ";
        }
        //プログラムのメインパス

        public void MessageOK()
        {

        }
        public async Task<bool> TreatDataAsync(string filename, string mp3Dir,string c="ID", bool isSentenceMp3=false)
        {
           
            if (IsFileLocked(filename))
            {
                MsgFrmMdl.Done = MessageOK;
                MsgFrmMdl.Msg = "The target file is opened. If you want to continue, close the file";


                return false;

            }
           
            
            fdata.Clear();
            //List<Dictionary<string, string>> fdata = new List<Dictionary<string, string>>();
            rfn = filename;
            dir = mp3Dir;
            
            if (!CsvTreat.ReadDicionaryTypeData(rfn,fdata, key))
            {
                MsgFrmMdl.Msg = "Target file is opened. If you want to run, close the file.";
                MsgFrmMdl.Done = MessageOK;
                return false;
            }

            
            Ret.Status = "Reading";
            int count = fdata.Count;
            Ret.Status = "Processing";
            int treatNum = 0;
            int downloadNum = 0;
            int targetNum = 0;
            int targetsymbolNum = 0;
            int symbolNum = 0;
            int targetSentenceNum = 0;
            int SentenceNum = 0;
            var oxford = new Ox();
            var longman = new Ldo();
            var weblio = new Webl();
            var eijiro = new Eiji();
            var coubuild = new Collins();
            foreach (var values in fdata)
            {
                var dl = false;
                var target_word = values["EngWord"];
                target_word = target_word.Trim().Replace(" ", "+");

                var ID = values["ID"];

                switch (c)
                {
                    case "Word":
                        ID = dir + values["EngWord"];
                        break;
                    case "TRK-Word":
                        ID = dir + "TRK-" + values["EngWord"];
                        break;
                }

                if (values["Sound"] == "n")
                {
                    var outputmp3 = dir + ID + ".mp3";
                    targetNum++;
                    BaseDic[] UseDicForWordmp3 = { oxford,weblio,longman,weblio,coubuild};
                    foreach (var dic in UseDicForWordmp3)
                    {
                        if (await dic.DownLoadMp3Async(target_word, outputmp3))
                        {
                            downloadNum++;
                           values["Sound"] = "A";
                            break;
                        }
                    }
                    dl = true;
                }

                if(values["Symbol"] =="n")
                {
                    targetsymbolNum++;
                    values["Symbol"]  = await eijiro.DownLoadSymbolAsync(target_word);
                    //values["Symbol"] = await oxford.AngleGetSymbolAsync(target_word);
                    if (values["Symbol"] != "n") symbolNum++;
                    dl = true;
                }
                if (values["EngSentence"] == "n")
                {
                    targetSentenceNum++;
                    var filemp3 = "SC_" + target_word + ".mp3";
                    var outputmp3 = dir + filemp3;
                    (var Sntnc,var isMp3) = await longman.GetEaxampleSentenceAsync(target_word, outputmp3, isSentenceMp3);
                    values["EngSentence"] = Sntnc;
                    if (isMp3) values["SntncSound"] = filemp3;
                    if (Sntnc == "n")
                    {
                        (Sntnc, isMp3) = await coubuild.GetEaxampleSentenceAsync(target_word, outputmp3);
                    }
                    if (values["EngSentence"] != "n") SentenceNum++;
                    dl = true;
                }
                treatNum++;
                Ret.Progress = treatNum * 100 / count;

                Ret.Rpt = $"mp3:{downloadNum}/{targetNum}\nSymbol:{symbolNum}/{targetsymbolNum}\nSentence:{SentenceNum}/{targetSentenceNum}";
                if (Ret.Status == "Canceled")
                {
                    Ret.Status ="Successfully Canceled";
                    break;
                }
                if(dl)
                {
                    await Task.Delay(1000);
                }
            }
            
            
            
            var temp=CsvTreat.WriteDicionaryTypeData(rfn, fdata, key);


            var mp3 = $"{downloadNum} mp3 files in {targetNum} were downloaded\n";
            var sym = $"{symbolNum} symbols in {targetsymbolNum} were gotten\n";
            var sntnc = $"{SentenceNum} sentence in {targetSentenceNum} were gotten";

            if (downloadNum < 2)
            {
                mp3 = $"{downloadNum} mp3 file in {targetNum} was downloaded \n"; ;
            }
            if (symbolNum < 2)
            {
                sym = $"{symbolNum} symbol in {targetsymbolNum} was gotten\n";
            }

            if (SentenceNum < 2)
            {
                sntnc = $"{SentenceNum} sentence in {SentenceNum} was gotten";
            }

            Ret.Rpt = mp3 + sym + sntnc;

            if (Ret.Status != "Successfully Canceled")
            {
                Ret.Status = "Completed";
            }
            return true;
        }

        private bool IsFileLocked(string filename)
        {
            FileStream stream = null;

            try
            {
                stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch
            {
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            return false;
        }

    }
}
