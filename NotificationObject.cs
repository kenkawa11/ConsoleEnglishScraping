
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace EnglishScraping.ViewModels.Notification
{


    /// <summary>
    /// INotifyPropertyChanged インターフェースを実装したプロパティ値変更通知機能を備えた抽象クラス
    /// </summary>
    public abstract class NotificationObject : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged のメンバ
        /// <summary>
        /// プロパティ変更時に発生します。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion  // INotifyPropertyChanged のメンバ

        /// <summary>
        /// PropertyChanged イベントを発行します。
        /// </summary>
        /// <param name="propertyName">プロパティ名を指定します。省略した場合、所有するすべてのプロパティに対して変更通知をおこないます。</param>
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// プロパティ値に変更があった場合、プロパティ値を変更し、 PropertyChanged イベントを発行します。
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="target">変更するプロパティを指定します。</param>
        /// <param name="value">変更後のプロパティ値を指定します。</param>
        /// <param name="propertyName">プロパティ名を指定します。省略した場合、所有するすべてのプロパティに対して変更通知をおこないます。</param>
        /// <returns>プロパティ値に変更があった場合に true を返します。</returns>
        protected virtual bool SetProperty<T>(ref T target, T value, string propertyName = null)
        {
            if (Equals(target, value))
                return false;
            target = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
    public class DelegateCommand : ICommand
    {


        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        public DelegateCommand(Action<object> execute)
        : this(execute, null)
        {
        }

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }
        public bool CanExecute(object parameter)
        {
            return this._canExecute == null || this._canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        public void Execute(object parameter)
        {
            if (this._execute != null)
                _execute(parameter);
        }
    }
}
