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
using System.ComponentModel;
using System.Windows.Threading;

namespace SLControls
{
    enum ResizeMode
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

    [TemplatePart(Name = SLWindow.ResizeRight, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = SLWindow.ResizeLeft, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = SLWindow.ResizeBottom, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = SLWindow.MainBorder, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = SLWindow.TitleContent, Type = typeof(TextBlock))]
    [TemplatePart(Name = SLWindow.CloseButton, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = SLWindow.MaximizeButton, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = SLWindow.MinimizeButton, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = SLWindow.TitleBarContent, Type = typeof(FrameworkElement))]
    [TemplateVisualState(GroupName=SLWindow.CommonStates,Name=SLWindow.OpenState)]
    [TemplateVisualState(GroupName = SLWindow.CommonStates, Name = SLWindow.ClosedState)]
    [TemplateVisualState(GroupName = SLWindow.CommonStates, Name = SLWindow.MinimizedState)]
    public class SLWindow : ContentControl, INotifyPropertyChanged
    {
        #region Constants

        private const string ResizeLeft = "ResizeLeft";
        private const string ResizeRight = "ResizeRight";
        private const string ResizeBottom = "ResizeBottom";
        private const string MainBorder = "MainBorder";
        private const string TitleContent = "TitleContent";
        private const string CloseButton = "CloseButton";
        private const string MaximizeButton = "MaximizeButton";
        private const string MinimizeButton = "MinimizeButton";
        private const string CommonStates = "CommonStates";
        private const string OpenState = "Open";
        private const string ClosedState = "Closed";
        private const string MinimizedState = "Minimized";
        private const string TitleBarContent = "TitleBarContent";

        #endregion

        #region Visual Elements

        private FrameworkElement _resizeLeft;
        private FrameworkElement _resizeRight;
        private FrameworkElement _resizeBottom;
        private FrameworkElement _mainBorder;
        private TextBlock _title;
        private FrameworkElement _closeButton;
        private FrameworkElement _maximizeButton;
        private FrameworkElement _minimizeButton;
        private FrameworkElement _titleBar;

        #endregion

        #region Static Members

        private static int _maxZIndex = 0;
        private static int _minimizedWindowCount = 0;

        #endregion

        #region Private Members

        //Drag - Move variables
        private bool _isMouseDown = false;
        private Point _windowMoveStartPosition;
        private Point _mouseMoveStartPosition;
        private Point _mouseMoveCurrentPosition;

        //Resize variables
        private ResizeMode _resizeMode = ResizeMode.None;
        private double _resizeStartWidth;
        private double _resizeStartHeight;
        private Point _windowResizeStartPosition;
        private Point _mouseResizeStartPosition;
        private Point _mouseResizeCurrentPosition;

        //Window State variables
        private double _memorizedWidth;
        private double _memorizedHeight;
        private Point _memorizedPosition;

        #endregion

        #region Properties
        private bool _dialogReslut;
        public bool DialogResult 
        {
            get { return _dialogReslut; }
            set { _dialogReslut = value; Close(); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(SLWindow),new PropertyMetadata("Window",TitlePropertyChanged));

        public Panel RootView
        {
            get { return (Panel)GetValue(RootViewProperty); }
            set { SetValue(RootViewProperty, value); }
        }
        public static readonly DependencyProperty RootViewProperty =
            DependencyProperty.Register("RootView", typeof(Panel), typeof(SLWindow), null);

        public SLWindowState WindowState
        {
            get { return (SLWindowState)GetValue(WindowStateProperty); }
            set 
            {
                SLWindowState currentState = WindowState;
                if (currentState != value)
                {
                    SetValue(WindowStateProperty, value);
                    if (currentState == SLWindowState.Normal)
                    {
                        _memorizedPosition.X = (this.RenderTransform as TranslateTransform).X;
                        _memorizedPosition.Y = (this.RenderTransform as TranslateTransform).Y;
                        _memorizedWidth = this.Width;
                        _memorizedHeight = this.Height;
                    }
                    switch (value)
                    {
                        case SLWindowState.Normal:
                            if (currentState == SLWindowState.Minimized)
                                _minimizedWindowCount--;
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                            VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                            (this.RenderTransform as TranslateTransform).X = _memorizedPosition.X;
                            (this.RenderTransform as TranslateTransform).Y = _memorizedPosition.Y;
                            this.Width = _memorizedWidth;
                            this.Height = _memorizedHeight;
                            break;
                        case SLWindowState.Maximized:
                            if (currentState == SLWindowState.Minimized)
                                _minimizedWindowCount--;
                            this.Width = double.NaN;
                            this.Height = double.NaN;
                            this.RenderTransform = new TranslateTransform();
                            VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                            break;
                        case SLWindowState.Minimized:
                            this.Width = MinWidth;
                            this.Height = 38;
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                            VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                            this.RenderTransform = new TranslateTransform();
                            (this.RenderTransform as TranslateTransform).X = MinWidth * _minimizedWindowCount + _minimizedWindowCount * 3;
                            _minimizedWindowCount++;
                            break;
                        default:
                            break;
                    }
                    if (WindowStateChanged != null)
                    {
                        WindowStateChanged(this, new EventArgs());
                    }
                }
            }
        }
        public static readonly DependencyProperty WindowStateProperty =
            DependencyProperty.Register("WindowState", typeof(SLWindowState), typeof(SLWindow), new PropertyMetadata(SLWindowState.Normal));

        public bool IsResizable
        {
            get { return (bool)GetValue(IsResizableProperty); }
            set { SetValue(IsResizableProperty, value); }
        }
        public static readonly DependencyProperty IsResizableProperty =
            DependencyProperty.Register("IsResizable", typeof(bool), typeof(SLWindow), new PropertyMetadata(true));

        public bool IsMaximizable
        {
            get { return (bool)GetValue(IsMaximizableProperty); }
            set { SetValue(IsMaximizableProperty, value); }
        }
        public static readonly DependencyProperty IsMaximizableProperty =
            DependencyProperty.Register("IsMaximizable", typeof(bool), typeof(SLWindow), new PropertyMetadata(true,IsMaximizablePropertyChanged));

        public bool IsMinimizable
        {
            get { return (bool)GetValue(IsMinimizableProperty); }
            set { SetValue(IsMinimizableProperty, value); }
        }
        public static readonly DependencyProperty IsMinimizableProperty =
            DependencyProperty.Register("IsMinimizable", typeof(bool), typeof(SLWindow), new PropertyMetadata(true, IsMinimizablePropertyChanged));

        #endregion

        #region Storyboards

        #endregion

        #region Events

        public event EventHandler Shown;
        public event EventHandler Closed;
        public event EventHandler TemplateApplied;
        public event EventHandler WindowStateChanged;

        #endregion

        #region CTOR
        public SLWindow()
        {
            this.DefaultStyleKey = typeof(SLWindow);
            RootView = new Grid();
            this.Width = 640;
            this.Height = 480;
            this.MinWidth = 175;
            this.MinHeight = 38;
        }
        #endregion

        #region Static Methods

        #endregion

        #region Methods
        
        public void Show()
        {
            RootView.Children.Add(this);
            this.TemplateApplied += (s, e) => { VisualStateManager.GoToState(this,OpenState,false); };
            this.Focus();
            DispatcherTimer dT = new DispatcherTimer();
            dT.Interval = new TimeSpan(0, 0, 0, 0, 300);
            dT.Tick += (s, e) =>
            {
                if (Shown != null)
                {
                    Shown(this, new EventArgs());
                }
                dT.Stop();
            };
            dT.Start();
        }

        public void Close()
        {
            if (WindowState == SLWindowState.Minimized)
                _minimizedWindowCount--;

            VisualStateManager.GoToState(this, ClosedState, false);
            DispatcherTimer dT = new DispatcherTimer();
            dT.Interval = new TimeSpan(0,0,0,0,400);
            dT.Tick += (s, e) => 
            {
                if (Closed != null)
                {
                    Closed(this, new EventArgs());
                }
                RootView.Children.Remove(this);
                dT.Stop();
            };
            dT.Start();
        }

        public void BringFront()
        {
            if (Canvas.GetZIndex(this) < _maxZIndex)
            {
                Canvas.SetZIndex(this, ++_maxZIndex);
            }
        }

        private void DragMove()
        {
            if (_isMouseDown && WindowState == SLWindowState.Normal)
            {
                //if (WindowState == SLWindowState.Maximized)
                //{
                //    WindowState = SLWindowState.Normal;
                //    VerticalAlignment = System.Windows.VerticalAlignment.Top;
                //    HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                //}
                double distanceX = _mouseMoveCurrentPosition.X - _mouseMoveStartPosition.X;
                double distanceY = _mouseMoveCurrentPosition.Y - _mouseMoveStartPosition.Y;

                (this.RenderTransform as TranslateTransform).X = _windowMoveStartPosition.X + distanceX;
                (this.RenderTransform as TranslateTransform).Y = _windowMoveStartPosition.Y + distanceY;
            }
        }

        private void Resize()
        {
            if (WindowState == SLWindowState.Minimized || WindowState == SLWindowState.Maximized)
                return;

            if (IsResizable && _resizeMode != ResizeMode.None)
            {
                if (VerticalAlignment == System.Windows.VerticalAlignment.Top && HorizontalAlignment == System.Windows.HorizontalAlignment.Right)
                {
                    VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                }
                TranslateTransform transform = this.RenderTransform as TranslateTransform;
                double amountWidth = _mouseResizeCurrentPosition.X - _mouseResizeStartPosition.X;
                double amountHeight = _mouseResizeCurrentPosition.Y - _mouseResizeStartPosition.Y;
                switch (_resizeMode)
                {
                    case ResizeMode.Left:
                    case ResizeMode.LeftTop:
                    case ResizeMode.LeftBottom:
                        Width = _resizeStartWidth - amountWidth > 0 ? _resizeStartWidth - amountWidth : 0;
                        if (this.MinWidth <= this.Width)
                        {
                            (this.RenderTransform as TranslateTransform).X = _windowResizeStartPosition.X + amountWidth / 2;
                        }
                        else
                            this.Width = this.MinWidth;
                        if (_resizeMode == ResizeMode.LeftBottom || _resizeMode == ResizeMode.LeftTop)
                            Height = _resizeStartHeight + amountHeight;
                        break;
                    case ResizeMode.Right:
                    case ResizeMode.RightBottom:
                    case ResizeMode.RightTop:
                        Width = _resizeStartWidth + amountWidth > 0 ? _resizeStartWidth + amountWidth : 0;
                        if (this.MinWidth <= this.Width)
                        {
                            (this.RenderTransform as TranslateTransform).X = _windowResizeStartPosition.X + amountWidth / 2;                            
                        }
                        else
                            this.Width = this.MinWidth;
                        if (_resizeMode == ResizeMode.RightBottom || _resizeMode == ResizeMode.RightTop)
                            Height = _resizeStartHeight + amountHeight;
                        break;
                    case ResizeMode.Top:
                    case ResizeMode.Bottom:
                        Height = _resizeStartHeight + amountHeight > 0 ? _resizeStartHeight + amountHeight : 0;
                        if (this.MinHeight <= this.Height)
                        {
                            (this.RenderTransform as TranslateTransform).Y = _windowResizeStartPosition.Y + amountHeight / 2;
                        }
                        else
                            this.Height = this.MinHeight;
                        break;
                }
            }
        }
        #endregion

        #region Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.RenderTransform = new TranslateTransform();
            Canvas.SetZIndex(this, ++_maxZIndex);

            this.MouseLeftButtonDown += (s, e) => { BringFront(); };
            if (Content != null)
            {
                (this.Content as FrameworkElement).MouseLeftButtonDown += (s, e) => { BringFront(); };
            }

            _resizeLeft = GetTemplateChild(ResizeLeft) as FrameworkElement;
            _resizeRight = GetTemplateChild(ResizeRight) as FrameworkElement;
            _resizeBottom = GetTemplateChild(ResizeBottom) as FrameworkElement;
            _mainBorder = GetTemplateChild(MainBorder) as FrameworkElement;
            _title = GetTemplateChild(TitleContent) as TextBlock;
            _closeButton = GetTemplateChild(CloseButton) as FrameworkElement;
            _maximizeButton = GetTemplateChild(MaximizeButton) as FrameworkElement;
            _minimizeButton = GetTemplateChild(MinimizeButton) as FrameworkElement;
            _titleBar = GetTemplateChild(TitleBarContent) as FrameworkElement;

            //WindowBar Buttons Visiblities
            if (!IsMaximizable)
            {
                _maximizeButton.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (!IsMinimizable)
            {
                _minimizeButton.Visibility = System.Windows.Visibility.Collapsed;
            }

            //Window Resize Events
            if (IsResizable)
            {
                MouseEventHandler mouseLeave = (s, e) => { _resizeMode = ResizeMode.None; };
                MouseButtonEventHandler mouseUp = (s, e) => { _resizeMode = ResizeMode.None; };
                MouseEventHandler mouseMove = (s, e) =>
                {
                    if (_resizeMode != ResizeMode.None)
                    {
                        _mouseResizeCurrentPosition = e.GetPosition(null);
                        Resize();
                    }
                };
                MouseButtonEventHandler mouseDown = (s, e) =>
                {
                    FrameworkElement sender = s as FrameworkElement;
                    _windowResizeStartPosition = new Point()
                    {
                        X = (this.RenderTransform as TranslateTransform).X,
                        Y = (this.RenderTransform as TranslateTransform).Y
                    };
                    _mouseResizeStartPosition = e.GetPosition(null);
                    _resizeStartHeight = this.Height;
                    _resizeStartWidth = this.Width;
                    if (sender == _resizeRight)
                    {
                        _resizeMode = ResizeMode.Right;
                    }
                    else if (sender == _resizeLeft)
                    {
                        _resizeMode = ResizeMode.Left;
                    }
                    else if (sender == _resizeBottom)
                    {
                        _resizeMode = ResizeMode.Bottom;
                    }
                };

                _resizeRight.MouseLeftButtonDown += mouseDown;
                _resizeLeft.MouseLeftButtonDown += mouseDown;
                _resizeBottom.MouseLeftButtonDown += mouseDown;

                this.MouseLeftButtonUp += mouseUp;
                //_resizeLeft.MouseLeftButtonUp += mouseUp;
                //_resizeBottom.MouseLeftButtonUp += mouseUp;

                RootView.MouseMove += mouseMove;
                RootView.MouseMove += mouseMove;
                RootView.MouseMove += mouseMove;
            }
            else
            {
                _resizeLeft.Cursor = _resizeRight.Cursor = _resizeBottom.Cursor = Cursors.Arrow;
            }

            //Window Button Events
            _closeButton.MouseLeftButtonUp += (s, e) => { Close(); };
            _minimizeButton.MouseLeftButtonUp += (s, e) => { WindowState = SLWindowState.Minimized; };
            _maximizeButton.MouseLeftButtonUp += (s, e) => 
            {
                if (WindowState == SLWindowState.Normal)
                    WindowState = SLWindowState.Maximized;
                else
                    WindowState = SLWindowState.Normal;
            };

            _title.Text = Title;

            //Mouse Drag-Move
            RootView.MouseMove += (s, e) => { _mouseMoveCurrentPosition = e.GetPosition(null); DragMove(); };
            _titleBar.MouseLeftButtonDown += (s, e) => 
            {
                _windowMoveStartPosition = new Point()
                {
                    X = (this.RenderTransform as TranslateTransform).X,
                    Y = (this.RenderTransform as TranslateTransform).Y
                };
                _isMouseDown = true;
                _mouseMoveStartPosition = e.GetPosition(null);
                e.Handled = true;
                BringFront();
            };
            _titleBar.MouseLeftButtonUp += (s, e) => { _isMouseDown = false; };

            //TemplateApplied Event
            if (TemplateApplied != null)
            {
                TemplateApplied(this,new EventArgs());
            }
        }

        #endregion

        #region Property Changed Callbacks
        private static void TitlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SLWindow slWindow = d as SLWindow;
            if (slWindow._title != null)
            {
                slWindow._title.Text = (string)e.NewValue;
            }
        }
        private static void IsMaximizablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SLWindow slWindow = d as SLWindow;
            bool value = (bool)e.NewValue;
            if (slWindow._maximizeButton != null)
            {
                slWindow._maximizeButton.Visibility = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }
        private static void IsMinimizablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SLWindow slWindow = d as SLWindow;
            bool value = (bool)e.NewValue;
            if (slWindow._minimizeButton != null)
            {
                slWindow._minimizeButton.Visibility = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }
        #endregion

        #region PropertyChanged event
        public event PropertyChangedEventHandler  PropertyChanged;
        protected void OnPropertyChanged(string Name)
        {
            if (PropertyChanged != null)
	        {
		        PropertyChanged(this,new PropertyChangedEventArgs(Name));
	        }
        }
        #endregion
    }
}
