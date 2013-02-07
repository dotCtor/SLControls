using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SLControls
{
    #region SLWindow
    public enum SLWindowResizeMode
    {
        None,
        Left,
        Right,
        Top,
        Bottom,
        LeftTop,
        RightTop,
        LeftBottom,
        RightBottom
    }

    public enum SLWindowState
    {
        Normal,
        Maximized,
        Minimized
    } 
    #endregion

    #region SLMessageWindow
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
    #endregion
}
