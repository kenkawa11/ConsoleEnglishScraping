using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using System.Text;
using System.Linq;
using System.IO;
using System;
using System.Windows;
using EnglishScraping.ViewModels.Notification;

namespace Utility
{
    public class FileStatus : NotificationObject //Raise the event when the members are changed
    {
        private string status;
        public string Status
        {
            get { return this.status; }
            set { SetProperty(ref this.status, value, "status"); }
        }
    }
    public class  CsvTreat
    {
        public FileStatus fileStatus = new FileStatus();

        private static string EncloseDoubleQuotesIfNeed(string field)
        {
            if (NeedEncloseDoubleQuotes(field))
            {
                return EncloseDoubleQuotes(field);
            }
            return field;
        }

        private static string EncloseDoubleQuotes(string field)
        {
            if (field.IndexOf('"') > -1)
            {
                //"を""とする
                field = field.Replace("\"", "\"\"");
            }
            return "\"" + field + "\"";
        }
        private static bool NeedEncloseDoubleQuotes(string field)
        {
            return field.IndexOf('"') > -1 ||
                field.IndexOf(',') > -1 ||
                field.IndexOf('\r') > -1 ||
                field.IndexOf('\n') > -1 ||
                field.StartsWith(" ") ||
                field.StartsWith("\t") ||
                field.EndsWith(" ") ||
                field.EndsWith("\t");
        }


        public static bool ReadDicionaryTypeData(string rfn, List<Dictionary<string, string>> fdata,string[] key)
        {
            
            var enumerable = Enumerable.Range(0,key.Length).ToArray();
            try
            {
                using TextFieldParser tfp = new TextFieldParser(rfn)
                {
                    //値がカンマで区切られているとする
                    TextFieldType = FieldType.Delimited,
                    Delimiters = new string[] { "," },

                    //値がダブルコーテーションで囲まれているか
                    HasFieldsEnclosedInQuotes = true,

                    //値をトリミングするか否か（お好みで）
                    TrimWhiteSpace = false
                };
                while (!tfp.EndOfData)
                {
                    List<string> stringList = tfp.ReadFields().ToList();
                    Dictionary<string, string> temp = enumerable.ToDictionary(x => key[x], x => stringList[x]);
                    //var temp2 = key.ToDictionary((key, index) => key, stringList[index]);
                    fdata.Add(temp);
                }
                return true;
            }
            catch(Exception)
            {
                return false;

            }

        }


        public static string WriteDicionaryTypeData(string rfn, List<Dictionary<string, string>> fdata, string[] key)
        {
            string line = null;
            try
            {


                using StreamWriter file = new StreamWriter(rfn, false, Encoding.UTF8);
                foreach (var v in fdata)
                {
                    /* 参考。順序不定になる可能性あり、不採用
                    foreach(var kvp in v)
                    {
                        line += CsvTreat.EncloseDoubleQuotesIfNeed(kvp.Value);
                        line += ",";
                    }
                    */
                    for (var i = 0; i < key.Length; i++)
                    {
                        line += CsvTreat.EncloseDoubleQuotesIfNeed(v[key[i]]);
                        line += ",";
                    }
                    line = line[0..^1];//Remove the tail comma
                    file.WriteLine(line);
                    line = null;
                }
                return "OK";
            }
            catch(Exception e)
            {
                var msg = $"{e.Message}\nIf you want to resume,close the file and press OK\nIf you want to cancel, press Cancel";

               /*
                var ret= MessageBox.Show(msg,"file is opened", MessageBoxButton.OKCancel);
                
                 if(ret==MessageBoxResult.OK)
                 {
                    WriteDicionaryTypeData(rfn, fdata, key);
                 }
               */

                return msg;

            }
        }
        public static void ReadData(string rfn, List<List<string>> fdata)
        {
            using TextFieldParser tfp = new TextFieldParser(rfn)
            {
                //値がカンマで区切られているとする
                TextFieldType = FieldType.Delimited,
                Delimiters = new string[] { "," },

                //値がダブルコーテーションで囲まれているか
                HasFieldsEnclosedInQuotes = true,

                //値をトリミングするか否か（お好みで）
                TrimWhiteSpace = false
            };
            while (!tfp.EndOfData)
            {
                List<string> stringList = tfp.ReadFields().ToList();
                fdata.Add(stringList);
            }
        }
        public static void WriteData(string rfn, List<List<string>> fdata)
        {
            string line = null;
            using StreamWriter file = new StreamWriter(rfn, false, Encoding.UTF8);
            foreach (var v in fdata)
            {
                foreach (var u in v)
                {
                    line += CsvTreat.EncloseDoubleQuotesIfNeed(u);
                    line += ",";
                }
                line = line[0..^1];//Remove the tail comma
                file.WriteLine(line);
                line = null;
            }
        }
    }
}
