using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using winforms = System.Windows.Forms;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.IO;

namespace ImpactSimulation
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool Maximized = false;
        private int NormalWidth = 0;
        private int NormalHeight = 0;
        private int NormalX = 0;
        private int NormalY = 0;
        private string oldStr;

        public MainWindow()
        {
            InitializeComponent();
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);

            try
            {
                throw new Exception("1");
            }
            catch (Exception e)
            {
                Console.WriteLine("Catch clause caught : {0} \n", e.Message);
            }
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;

            StreamWriter er = new StreamWriter("errors.txt");
            er.WriteLine("MyHandler caught : " + e.Message);
            er.WriteLine("Runtime terminating: {0}", args.IsTerminating);
            er.Close();
        }

        #region Header & Resize
        private void Header_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                if (Maximized)
                {
                    WindowState = WindowState.Normal;
                    this.Left = e.GetPosition(null).X * (1 - NormalWidth / this.Width);
                    this.Top = e.GetPosition(null).Y - Top_Panel.ActualHeight / 2;
                    this.Width = NormalWidth;
                    this.Height = NormalHeight;
                    Maximized = false;
                    btnImage_MaximizedSize.Visibility = Visibility.Hidden;
                    btnImage_NormalSize.Visibility = Visibility.Visible;
                    Thumbs();
                }
                this.DragMove();
            }
        }

        private void ThumbBottomRightCorner_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.Width + e.HorizontalChange > 10)
                this.Width += e.HorizontalChange;
            if (this.Height + e.VerticalChange > 10)
                this.Height += e.VerticalChange;
        }
        private void ThumbTopRightCorner_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.Width + e.HorizontalChange > 10)
                this.Width += e.HorizontalChange;
            if (this.Top + e.VerticalChange > 10)
            {
                this.Top += e.VerticalChange;
                this.Height -= e.VerticalChange;
            }
        }
        private void ThumbTopLeftCorner_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.Left + e.HorizontalChange > 10)
            {
                this.Left += e.HorizontalChange;
                this.Width -= e.HorizontalChange;
            }
            if (this.Top + e.VerticalChange > 10)
            {
                this.Top += e.VerticalChange;
                this.Height -= e.VerticalChange;
            }
        }
        private void ThumbBottomLeftCorner_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.Left + e.HorizontalChange > 10)
            {
                this.Left += e.HorizontalChange;
                this.Width -= e.HorizontalChange;
            }
            if (this.Height + e.VerticalChange > 10)
                this.Height += e.VerticalChange;
        }
        private void ThumbRight_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.Width + e.HorizontalChange > 10)
                this.Width += e.HorizontalChange;
        }
        private void ThumbLeft_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.Left + e.HorizontalChange > 10)
            {
                this.Left += e.HorizontalChange;
                this.Width -= e.HorizontalChange;
            }
        }
        private void ThumbBottom_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.Height + e.VerticalChange > 10)
                this.Height += e.VerticalChange;
        }
        private void ThumbTop_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.Top + e.VerticalChange > 10)
            {
                this.Top += e.VerticalChange;
                this.Height -= e.VerticalChange;
            }
        }

        private void btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void btn_Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (Maximized)
            {
                WindowState = WindowState.Normal;
                this.Width = NormalWidth;
                this.Height = NormalHeight;
                this.Left = NormalX;
                this.Top = NormalY;
                Maximized = false;
                btnImage_MaximizedSize.Visibility = Visibility.Hidden;
                btnImage_NormalSize.Visibility = Visibility.Visible;
                Thumbs();
            }
            else
            {
                NormalX = (int)this.Left;
                NormalY = (int)this.Top;
                NormalHeight = (int)this.Height;
                NormalWidth = (int)this.Width;
                this.Left = winforms.Screen.FromHandle(new WindowInteropHelper(this).Handle).WorkingArea.Left - 1;
                this.Top = winforms.Screen.FromHandle(new WindowInteropHelper(this).Handle).WorkingArea.Top - 1;
                this.Width = winforms.Screen.FromHandle(new WindowInteropHelper(this).Handle).WorkingArea.Width + 2;
                this.Height = winforms.Screen.FromHandle(new WindowInteropHelper(this).Handle).WorkingArea.Height + 2;
                Maximized = true;
                btnImage_MaximizedSize.Visibility = Visibility.Visible;
                btnImage_NormalSize.Visibility = Visibility.Hidden;
                Thumbs();
            }
        }
        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void win_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                NormalX = (int)this.Left;
                NormalY = (int)this.Top;
                NormalHeight = (int)this.Height;
                NormalWidth = (int)this.Width;
                this.Left = winforms.Screen.FromHandle(new WindowInteropHelper(this).Handle).WorkingArea.Left;
                this.Top = winforms.Screen.FromHandle(new WindowInteropHelper(this).Handle).WorkingArea.Top;
                this.Width = winforms.Screen.FromHandle(new WindowInteropHelper(this).Handle).WorkingArea.Width;
                this.Height = winforms.Screen.FromHandle(new WindowInteropHelper(this).Handle).WorkingArea.Height;
                Maximized = true;
                btnImage_MaximizedSize.Visibility = Visibility.Visible;
                btnImage_NormalSize.Visibility = Visibility.Hidden;
                Thumbs();
            }
        }

        private void win_Closing(object sender, CancelEventArgs e)
        {
        }

        private void Thumbs()
        {
            if (Maximized == true)
            {
                ThumbBottom.Visibility = Visibility.Collapsed;
                ThumbLeft.Visibility = Visibility.Collapsed;
                ThumbTop.Visibility = Visibility.Collapsed;
                ThumbRight.Visibility = Visibility.Collapsed;
                ThumbTopLeftCorner.Visibility = Visibility.Collapsed;
                ThumbTopRightCorner.Visibility = Visibility.Collapsed;
                ThumbBottomLeftCorner.Visibility = Visibility.Collapsed;
                ThumbBottomRightCorner.Visibility = Visibility.Collapsed;
            }
            else
            {
                ThumbBottom.Visibility = Visibility.Visible;
                ThumbLeft.Visibility = Visibility.Visible;
                ThumbTop.Visibility = Visibility.Visible;
                ThumbRight.Visibility = Visibility.Visible;
                ThumbTopLeftCorner.Visibility = Visibility.Visible;
                ThumbTopRightCorner.Visibility = Visibility.Visible;
                ThumbBottomLeftCorner.Visibility = Visibility.Visible;
                ThumbBottomRightCorner.Visibility = Visibility.Visible;
            }
        }


        #endregion

        #region Workspace & Thread

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
        }
        private void btn_Restart_Click(object sender, RoutedEventArgs e)
        {
        }

        private void PreviewTextInputT(object sender, TextCompositionEventArgs e)
        {
            Regex regexF = new Regex(@"^[0-9-,]*$");
            e.Handled = !regexF.IsMatch(e.Text);
        }

        private void PreviewTextInputTP(object sender, TextCompositionEventArgs e)
        {
            Regex regexF = new Regex(@"^[0-9,]*$");
            e.Handled = !regexF.IsMatch(e.Text);
        }

        private void GotFocusT(object sender, RoutedEventArgs e)
        {
            TextBox b = e.Source as TextBox;
            oldStr = b.Text;
        }

        private void LostFocusT(object sender, RoutedEventArgs e)
        {
            TextBox b = e.Source as TextBox;
            if (b.Text.Length == 0)
                b.Text = "0";
            else if (!double.TryParse(b.Text, out _))
                b.Text = oldStr;
        }

        private void LostFocusTM(object sender, RoutedEventArgs e)
        {
            TextBox b = e.Source as TextBox;
            if (b.Text.Length == 0)
                b.Text = "1";
            else if (!decimal.TryParse(b.Text, out decimal temp) || temp == 0)
                b.Text = oldStr;
        }

        private void LostFocusTE(object sender, RoutedEventArgs e)
        {
            TextBox b = e.Source as TextBox;
            if (b.Text.Length == 0)
                b.Text = "1";
            else if (!decimal.TryParse(b.Text, out decimal temp) || temp > 1)
                b.Text = oldStr;
        }

        #endregion

    }
}
