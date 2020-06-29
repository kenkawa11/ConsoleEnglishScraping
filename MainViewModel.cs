using System;
using EnglishScraping.Models;
using System.ComponentModel;
using System.Threading.Tasks;
using EnglishScraping.ViewModels.Notification;


namespace EnglishScraping.ViewModels
{
    public class MainViewModel : NotificationObject
    {


        //コンストラクター
        private readonly PronounceDownloader DownLoder = new PronounceDownloader();
        public MainViewModel()
        {
            dir = @"C:\Users\naobaby\Desktop\test\";
            //dir = @"C:\Users\naoak\AppData\Roaming\Anki2\ユーザー 1\collection.media\";
            isActiveDone = true;
            DownLoder.Ret.PropertyChanged += OnModelChanged;
            DownLoder.MsgFrmMdl.PropertyChanged += OnModelMessageChanged;
            Mp3filename = "ID";
            DownLoder.Ret.Status = "Waiting for loading data";
        }

        //ファイルオープンメニュー
        private string fn;
        public string Fn
        {
            get { return this.fn; }
            set { SetProperty(ref this.fn, value); }
        }
        private readonly TextMsgColor fileInputMessage = new TextMsgColor();
        public TextMsgColor FileInputMessage
        {
            get { return fileInputMessage; }
        }

        private DelegateCommand _openFileCommand;
        public DelegateCommand OpenFileCommand
        {
            get
            {
                return this._openFileCommand ?? (_openFileCommand = new DelegateCommand(
                    _ =>
                    {
                        FileInputMessage.Msg = "";
                        FileInputMessage.TextBorderColor = "Gray";
                        this.DialogCallback = OnDialogCallback;
                    }));
            }
        }

        private Action<bool, string> _dialogCallback;
        public Action<bool, string> DialogCallback
        {
            get { return this._dialogCallback; }
            private set { SetProperty(ref this._dialogCallback, value); }
        }

        private void OnDialogCallback(bool isOk, string filePath)
        {
            this.DialogCallback = null;

            Fn = filePath;

            if (string.IsNullOrWhiteSpace(Fn))
            {
                return;
            }
            DownLoder.Ret.Status = "Ready";
        }

        //mp3ディレクトリー選択メニュー
        private string dir;
        public string Dir
        {
            get { return this.dir; }
            set { SetProperty(ref this.dir, value); }
        }

        private DelegateCommand _selectDirectory;
        public DelegateCommand SelectDirectory
        {
            get
            {
                return this._selectDirectory ?? (_selectDirectory = new DelegateCommand(
                    _ =>
                    {
                        this.DialogDirCallback = OnDirDialogCallback;

                    }));
            }
        }
        private Action<bool, string> _dialogDirCallback;
        public Action<bool, string> DialogDirCallback
        {
            get { return this._dialogDirCallback; }
            private set { SetProperty(ref this._dialogDirCallback, value); }
        }
        private void OnDirDialogCallback(bool isOk, string filePath)
        {
            this.DialogDirCallback = null;
            Dir = filePath;
        }

        //終了関係

        private async Task<bool> OnExit()
        {
            //await Task.Delay(2000);
            //App.Current.Shutdown();
            return true;
        }

        //WindowClosing

        public Func<Task<bool>> ClosingCallback
        {
            get
            {
                return OnExit;
            }
        }

        private DelegateCommand _exitApplication;
        public DelegateCommand ExitApplication
        {
            get
            {
                return this._exitApplication ??= new DelegateCommand
                    (_ => {
                        OnExitWindow();
                    });
            }
        }

        private async void OnExitWindow()
        {
            if (Status == "Completed" || Status == "Canceled" || Status == "Waiting for loading data" || Status == "Ready" || Status == "Successfully Canceled")
            {
                //App.Current.Shutdown();

            }
            else
            {
                DownLoder.Ret.Status = "Canceled";

                while (true)
                {
                    if (DownLoder.Ret.Status == "Successfully Canceled") break;
                    await Task.Delay(50);
                }
            }
            //App.Current.Shutdown();
        }


        //ヘルプメニュー関係

        private DelegateCommand _helpDialogCommand;
        public DelegateCommand HelpDialogCommand => this._helpDialogCommand ??= new
                    DelegateCommand(_ =>
                    {
                        this.HelpDialogCallback = OnHelpDialog;
                    });
        private Action<bool> _helpDialogCallback;
        public Action<bool> HelpDialogCallback
        {
            get { return this._helpDialogCallback; }
            private set { SetProperty(ref this._helpDialogCallback, value); }
        }
        private void OnHelpDialog(bool result)
        {
            this.HelpDialogCallback = null;
        }


        //Program Infoメニュー
        private DelegateCommand _programInfoDialogCommand;
        public DelegateCommand ProgramInfoDialogCommand => this._programInfoDialogCommand ??= new
                    DelegateCommand(_ =>
                    {
                        this.ProgramInfoDialogCallback = OnProgramInfoDialog;
                    });

        private Action<bool> _programInfoDialogCallback;

        public Action<bool> ProgramInfoDialogCallback
        {
            get { return this._programInfoDialogCallback; }
            private set { SetProperty(ref this._programInfoDialogCallback, value); }
        }

        private void OnProgramInfoDialog(bool result)
        {
            this.ProgramInfoDialogCallback = null;
            System.Diagnostics.Debug.WriteLine(result);
        }

        //チェックボックス例文音声ダウンロード選択
        public bool IsSentencemp3 { get; set; }


        //リストボックス単語音声ファイルのファイル名の付け方
        private readonly string[] mp3fnlist = new string[]
{
            "ID","Word","TRK-Word"
};
        public string[] Mp3Fnlist
        {
            get { return mp3fnlist; }
        }

        private string mp3filename;
        public string Mp3filename
        {
            get { return this.mp3filename; }
            set { SetProperty(ref this.mp3filename, value); }
        }

        //実行ボタン関係
        private bool isActiveDone;
        public bool IsActiveDone
        {
            get { return this.isActiveDone; }
            set { SetProperty(ref this.isActiveDone, value); }
        }
        private DelegateCommand _done;
        public DelegateCommand Done
        {
            get
            {
                if (this._done == null)
                {
                    this._done = new DelegateCommand(_ =>
                    {
                        if (!string.IsNullOrWhiteSpace(fn))
                        {
                            Status = "Initialization";
                            IsActiveDone = false;
                            /*
                            Task task = Task.Run(() => {
                                DownLoder.TreatDataAsync(fn, dir, Mp3filename);
                            });
                           */
                            Asyncfntreat();
                            //DownLoder.TreatDataAsync(fn, dir, Mp3filename);
                        }
                        else
                        {
                            FileInputMessage.Msg = "Select a target file !!!";
                            FileInputMessage.Color = "Red";
                            FileInputMessage.TextBorderColor = "Red";
                            //this.MessageDialogCallback = OnMessageDialog;
                        }
                    });
                }
                return this._done;
            }
        }
        private async void Asyncfntreat()
        {
            await DownLoder.TreatDataAsync(fn, dir, Mp3filename, IsSentencemp3);
        }

        //キャンセル関係
        private DelegateCommand _cancel;
        public DelegateCommand Cancel
        {
            get
            {
                if (this._cancel == null)
                {
                    this._cancel = new DelegateCommand(_ =>
                    {
                        DownLoder.Ret.Status = "Canceled";
                    });
                }
                return this._cancel;
            }
        }

        //Model進捗通知関連
        private void OnModelChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "progress":
                    BarProgress = DownLoder.Ret.Progress;
                    Progress = $"{BarProgress}%";
                    break;
                case "rpt":
                    Rpt = DownLoder.Ret.Rpt;
                    break;
                case "status":
                    Status = DownLoder.Ret.Status;
                    if (Status == "Completed" || Status == "Successfully Canceled")
                    {
                        this.IsActiveDone = true;
                    }
                    break;
            }
        }

        //プログレスバー
        private int barProgress;
        public int BarProgress
        {
            get { return this.barProgress; }
            set { SetProperty(ref this.barProgress, value); }
        }

        //プログレス数字
        private string progress;
        public string Progress
        {
            get { return this.progress; }
            set { SetProperty(ref this.progress, value); }
        }

        //ダウンロード状況
        private string rpt;
        public string Rpt
        {
            get { return this.rpt; }
            set { SetProperty(ref this.rpt, value); }
        }

        //プロセス状況
        //Status Waiting for loading data
        //Ready
        //Reading Processing Writing Canceled Completed
        private string status;
        public string Status
        {
            get { return this.status; }
            set { SetProperty(ref this.status, value); }
        }

        //メッセージウインドウ関係
        private void OnModelMessageChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((DownLoder.MsgFrmMdl.Msg != null))
            {
                Msg = DownLoder.MsgFrmMdl.Msg;
                MessageBoxAction = DownLoder.MsgFrmMdl.Done;
                MessageDialogCallback = OnMessageDialog;
                IsActiveDone = true;
            }
            DownLoder.MsgFrmMdl.Msg = null;
        }

        private string msg;
        public string Msg
        {
            get { return this.msg; }
            set { SetProperty(ref this.msg, value); }
        }
        private Action _messageBoxAction;
        public Action MessageBoxAction
        {
            get { return this._messageBoxAction; }
            set { SetProperty(ref this._messageBoxAction, value); }
        }

        //MessageWindowViewのトリガー
        private Action<bool> _messageDialogCallback;
        public Action<bool> MessageDialogCallback
        {
            get { return this._messageDialogCallback; }
            private set { SetProperty(ref this._messageDialogCallback, value); }
        }

        //MessageWindow ボタン処理
        private void OnMessageDialog(bool result)
        {
            this.MessageDialogCallback = null;
            //System.Diagnostics.Debug.WriteLine(result);
        }
        private DelegateCommand _messageOK;
        public DelegateCommand MessageOK
        {
            get
            {
                if (_messageOK == null)
                {
                    _messageOK = new DelegateCommand(_ =>
                    {
                        MessageBoxAction();

                    });
                }
                return _messageOK;
            }
        }

        //ステータスバー
        private System.Timers.Timer _timer;
        private DateTime _currentTime;
        public DateTime CurrentTime
        {
            get
            {
                if (this._timer == null)
                {
                    this._currentTime = DateTime.Now;
                    this._timer = new System.Timers.Timer(100000); 
                    this._timer.Elapsed += (_, __) => { this.CurrentTime = DateTime.Now; }; this._timer.Start();
                }
                return this._currentTime;
            }
            private set { SetProperty(ref this._currentTime, value); }
        }

    }
    public class TextMsgColor : NotificationObject
    {
        private string msg;
        private string color;
        private string textBorderColor;

        public string TextBorderColor
        {
            get { return this.textBorderColor; }
            set { SetProperty(ref this.textBorderColor, value, "textBorderColor"); }
        }
        public string Msg
        {
            get { return this.msg; }
            set { SetProperty(ref this.msg, value, "msg"); }
        }
        public string Color
        {
            get { return this.color; }
            set { SetProperty(ref this.color, value, "color"); }
        }
    }
}