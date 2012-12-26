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
using SLControls;

namespace SLControlsTest
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            this.MouseRightButtonDown += (s, e) => 
            {
                e.Handled = true;
            };
            button1_Copy.Click += (sndr, arg) => 
            {
                //SLMessageWindow.Display("Hello World!","Message",SLMessageWindowButton.OKCancel);
                SLMessageWindow messageWindow = new SLMessageWindow("Would you like a cup of coffee?", "Warning!"
                    ,SLMessageWindowButton.YesNoCancel,SLMessageWindowType.Warning);
                messageWindow.Show();
                messageWindow.Closed += (s,e) => 
                {
                    if (messageWindow.DialogResult == SLMessageWindowResult.Yes)
                    {
                        SLMessageWindow.Display("One cup of coffee coming right up!");
                    }
                };
            };
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MyWindow window = new MyWindow();
            window.RootView = LayoutRoot;
            window.Show();
            window.Closed += (s, arg) => { SLMessageWindow.Display("Window is closed"); };
            window.Shown += (s, arg) => { SLMessageWindow.Display("Window is shown"); };

        }
    }
}
