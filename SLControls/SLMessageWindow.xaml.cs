using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Threading;

namespace SLControls
{
    public enum SLMessageWindowResult
    {
        None,
        OK,
        Cancel,
        Yes,
        No,
        Abort,
        Retry,
        Ignore
    }

    public enum SLMessageWindowButton
    {
        OK,
        OKCancel,
        AbortRetryIgnore,
        YesNoCancel,
        YesNo,
        RetryCancel
    }

    public enum SLMessageWindowType
    {
        Information,
        Error,
        Warning
    }

    public partial class SLMessageWindow : ChildWindow
    {
        #region Constraints
        private const string DEFAULTTITLE = "Message";
        private readonly BitmapImage IMAGEINFORMATION = new BitmapImage(new Uri("/SLControls;component/Images/MessageWindowImages/info.png", UriKind.Relative));
        private readonly BitmapImage IMAGEERROR = new BitmapImage(new Uri("/SLControls;component/Images/MessageWindowImages/error.png", UriKind.Relative));
        private readonly BitmapImage IMAGEWARNING = new BitmapImage(new Uri("/SLControls;component/Images/MessageWindowImages/alert.png", UriKind.Relative));
        #endregion

        #region Public Properties
        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                txtMessage.Text = _message;
            }
        }

        private SLMessageWindowType _messageType;
        public SLMessageWindowType MessageType
        {
            get { return _messageType; }
            set
            {
                _messageType = value;
                switch (_messageType)
                {
                    case SLMessageWindowType.Information:
                        imgMessageType.Source = IMAGEINFORMATION;
                        break;
                    case SLMessageWindowType.Error:
                        imgMessageType.Source = IMAGEERROR;
                        break;
                    case SLMessageWindowType.Warning:
                        imgMessageType.Source = IMAGEWARNING;
                        break;
                    default:
                        imgMessageType.Source = IMAGEINFORMATION;
                        break;
                }
            }
        }

        private SLMessageWindowButton _buttons;
        public SLMessageWindowButton Buttons
        {
            get { return _buttons; }
            set
            {
                _buttons = value;
                OKButton.Visibility = Visibility.Collapsed;
                CancelButton.Visibility = Visibility.Collapsed;
                AbortButton.Visibility = Visibility.Collapsed;
                RetryButton.Visibility = Visibility.Collapsed;
                IgnoreButton.Visibility = Visibility.Collapsed;
                YesButton.Visibility = Visibility.Collapsed;
                NoButton.Visibility = Visibility.Collapsed;
                switch (_buttons)
                {
                    case SLMessageWindowButton.OK:
                        OKButton.Visibility = Visibility.Visible;
                        break;
                    case SLMessageWindowButton.OKCancel:
                        OKButton.Visibility = Visibility.Visible;
                        CancelButton.Visibility = Visibility.Visible;
                        break;
                    case SLMessageWindowButton.AbortRetryIgnore:
                        AbortButton.Visibility = Visibility.Visible;
                        RetryButton.Visibility = Visibility.Visible;
                        IgnoreButton.Visibility = Visibility.Visible;
                        break;
                    case SLMessageWindowButton.YesNoCancel:
                        YesButton.Visibility = Visibility.Visible;
                        NoButton.Visibility = Visibility.Visible;
                        CancelButton.Visibility = Visibility.Visible;
                        break;
                    case SLMessageWindowButton.YesNo:
                        YesButton.Visibility = Visibility.Visible;
                        NoButton.Visibility = Visibility.Visible;
                        break;
                    case SLMessageWindowButton.RetryCancel:
                        RetryButton.Visibility = Visibility.Visible;
                        CancelButton.Visibility = Visibility.Visible;
                        break;
                    default:
                        OKButton.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        public new SLMessageWindowResult DialogResult { get; private set; } 
        #endregion

        #region CTOR

        public SLMessageWindow()
            : this("")
        {
        }

        public SLMessageWindow(string message)
            : this(message, DEFAULTTITLE, SLMessageWindowButton.OK, SLMessageWindowType.Information)
        {
        }

        public SLMessageWindow(string message,string title)
            : this(message,title,SLMessageWindowButton.OK, SLMessageWindowType.Information)
        {
        }

        public SLMessageWindow(string message, SLMessageWindowType messageType)
            : this(message, DEFAULTTITLE, SLMessageWindowButton.OK, messageType)
        {
        }

        public SLMessageWindow(string message,string title, SLMessageWindowButton buttons)
            : this(message, DEFAULTTITLE, buttons,SLMessageWindowType.Information)
        {
        }

        public SLMessageWindow(string message, string title, SLMessageWindowType messageType)
            : this(message, title, SLMessageWindowButton.OK, messageType)
        {
        }

        public SLMessageWindow(string message, string title, SLMessageWindowButton buttons, SLMessageWindowType messageType)
        {
            InitializeComponent();

            OKButton.Click += (s, e) =>
            {
                this.DialogResult = SLMessageWindowResult.OK;
                base.DialogResult = true;
            };

            CancelButton.Click += (s, e) =>
            {
                this.DialogResult = SLMessageWindowResult.Cancel;
                base.DialogResult = true;
            };

            AbortButton.Click += (s, e) =>
            {
                this.DialogResult = SLMessageWindowResult.Abort;
                base.DialogResult = true;
            };

            RetryButton.Click += (s, e) =>
            {
                this.DialogResult = SLMessageWindowResult.Retry;
                base.DialogResult = true;
            };

            IgnoreButton.Click += (s, e) =>
            {
                this.DialogResult = SLMessageWindowResult.Ignore;
                base.DialogResult = true;
            };

            YesButton.Click += (s, e) =>
            {
                this.DialogResult = SLMessageWindowResult.Yes;
                base.DialogResult = true;
            };

            NoButton.Click += (s, e) =>
            {
                this.DialogResult = SLMessageWindowResult.No;
                base.DialogResult = true;
            };

            Message = message;
            MessageType = messageType;
            Title = title;
            DialogResult = SLMessageWindowResult.None;
            Buttons = buttons;
        }

        #endregion

        #region Public Methods
        public static void Display()
        {
            (new SLMessageWindow() as ChildWindow).Show();
        }

        public static void Display(string message)
        {
            (new SLMessageWindow(message) as ChildWindow).Show();
        }

        public static void Display(string message, string title)
        {
            (new SLMessageWindow(message, title) as ChildWindow).Show();
        }

        public static void Display(string message, SLMessageWindowType messageType)
        {
            (new SLMessageWindow(message, messageType) as ChildWindow).Show();
        }

        public static void Display(string message, string title, SLMessageWindowType messageType)
        {
            (new SLMessageWindow(message, title, messageType) as ChildWindow).Show();
        }

        public static void Display(string message, string title, SLMessageWindowButton buttons)
        {
            (new SLMessageWindow(message, title,buttons) as ChildWindow).Show();
        }
        #endregion
    }
}

