using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using winforms = System.Windows.Forms;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Threading;
using System.IO;

namespace ImpactSimulation
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool Maximized = false;
        private bool Start = false;
        private bool InProcess = false;
        private int NormalWidth = 0;
        private int NormalHeight = 0;
        private int NormalX = 0;
        private int NormalY = 0;
        private long Сollisions = 0;
        private string oldStr;
        private object threadLock = new object();

        private Block Block1;
        private Block Block2;

        private BackgroundWorker Worker;
        private Thread LogicalThread;

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
            Worker = ((BackgroundWorker)this.FindResource("backgroundWorker"));
            Block.Resiliency = Convert.ToDecimal(TextBox_Initial_Resiliency.Text);
            Block1 = new Block(Convert.ToDecimal(TextBox_Initial_Mass1.Text), ((decimal)Canvas.GetLeft(Border_Block1), (int)Canvas.GetTop(Border_Block1)), (int)Border_Block1.Width, 0);
            Block2 = new Block(Convert.ToDecimal(TextBox_Initial_Mass2.Text), ((decimal)Canvas.GetLeft(Border_Block2), (int)Canvas.GetTop(Border_Block2)), (int)Border_Block2.Width, -1m / 10000);
            TextBox_Variable_Collisions.Text = Сollisions.ToString();
            TextBox_Variable_KinEnergy1.Text = (Block1.Mass * (Block1.Speed * 10000) * (Block1.Speed * 10000) / 2).ToString("G10");
            TextBox_Variable_KinEnergy2.Text = (Block2.Mass * (Block2.Speed * 10000) * (Block2.Speed * 10000) / 2).ToString("G10");
            TextBox_Variable_Position1.Text = Block1.Position.X.ToString("G10");
            TextBox_Variable_Position2.Text = Block2.Position.X.ToString("G10");
            TextBox_Variable_Speed1.Text = (Block1.Speed * 10000).ToString("G10");
            TextBox_Variable_Speed2.Text = (Block2.Speed * 10000).ToString("G10");
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
            if (Start)
                Worker.CancelAsync();
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
            LogicalThread = new Thread(new ThreadStart(ThreadProc));
            if (Start)
            {
                Start = false;
                btnImage_Start.Visibility = Visibility.Visible;
                btnImage_Stop.Visibility = Visibility.Hidden;
                Worker.CancelAsync();
            }
            else
            {
                Start = true;
                if (!InProcess)
                {
                    Block1.Mass = Convert.ToDecimal(TextBox_Initial_Mass1.Text);
                    Block1.Speed = Convert.ToDecimal(TextBox_Initial_Speed1.Text) / 10000;
                    Block2.Mass = Convert.ToDecimal(TextBox_Initial_Mass2.Text);
                    Block2.Speed = Convert.ToDecimal(TextBox_Initial_Speed2.Text) / 10000;
                    Block.Resiliency = Convert.ToDecimal(TextBox_Initial_Resiliency.Text);
                    Canvas.SetLeft(Border_Block1, (double)Block1.Position.X);
                    Canvas.SetLeft(Border_Block2, (double)Block2.Position.X);
                    Сollisions = 0;
                    TextBox_Variable_Collisions.Text = Сollisions.ToString();
                    InProcess = true;
                }
                btnImage_Start.Visibility = Visibility.Hidden;
                btnImage_Stop.Visibility = Visibility.Visible;
                Worker.RunWorkerAsync();
                LogicalThread.Start();
            }
        }
        private void btn_Restart_Click(object sender, RoutedEventArgs e)
        {
            if (!Start)
            {
                InProcess = false;
            }
            lock (threadLock)
            {
                Block1.Mass = Convert.ToDecimal(TextBox_Initial_Mass1.Text);
                Block1.Speed = Convert.ToDecimal(TextBox_Initial_Speed1.Text) / 10000;
                Block2.Mass = Convert.ToDecimal(TextBox_Initial_Mass2.Text);
                Block2.Speed = Convert.ToDecimal(TextBox_Initial_Speed2.Text) / 10000;
                Block.Resiliency = Convert.ToDecimal(TextBox_Initial_Resiliency.Text);
                Block1.Position.X = 1100;
                Block2.Position.X = 1700;
                Canvas.SetLeft(Border_Block1, (double)Block1.Position.X);
                Canvas.SetLeft(Border_Block2, (double)Block2.Position.X);
                Сollisions = 0;
                TextBox_Variable_Collisions.Text = Сollisions.ToString();
                TextBox_Variable_Collisions.Text = Сollisions.ToString();
                TextBox_Variable_KinEnergy1.Text = (Block1.Mass * (Block1.Speed * 10000) * (Block1.Speed * 10000) / 2).ToString("G10");
                TextBox_Variable_KinEnergy2.Text = (Block2.Mass * (Block2.Speed * 10000) * (Block2.Speed * 10000) / 2).ToString("G10");
                TextBox_Variable_Position1.Text = Block1.Position.X.ToString("G10");
                TextBox_Variable_Position2.Text = Block2.Position.X.ToString("G10");
                TextBox_Variable_Speed1.Text = (Block1.Speed * 10000).ToString("G10");
                TextBox_Variable_Speed2.Text = (Block2.Speed * 10000).ToString("G10");
            }
        }

        private void ThreadProc()
        {
            while (true)
            {
                if (Worker.CancellationPending)
                    break;
                lock (threadLock)
                {
                    Block1.Position.X += Block1.Speed;
                    Block2.Position.X += Block2.Speed;
                    if (Block1.Position.X < 400)
                    {
                        Block.Impact(Block1);
                        Сollisions++;
                    }
                    if (Block1.Position.X + Block1.Size > Block2.Position.X)
                    {
                        Block.Impact(Block1, Block2);
                        Сollisions++;
                    }
                }
                //Thread.Sleep(new TimeSpan(10));
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            long update = 0;
            while (true)
            {
                if (Worker.CancellationPending)
                    break;
                if (Worker.WorkerReportsProgress)
                    Worker.ReportProgress(1);
                update++;
                Thread.Sleep(1);
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lock (threadLock)
            {
                TextBox_Variable_Collisions.Text = Сollisions.ToString();
                TextBox_Variable_KinEnergy1.Text = (Block1.Mass * (Block1.Speed * 10000) * (Block1.Speed * 10000) / 2).ToString("G10");
                TextBox_Variable_KinEnergy2.Text = (Block2.Mass * (Block2.Speed * 10000) * (Block2.Speed * 10000) / 2).ToString("G10");
                TextBox_Variable_Position1.Text = Block1.Position.X.ToString("G10");
                TextBox_Variable_Position2.Text = Block2.Position.X.ToString("G10");
                TextBox_Variable_Speed1.Text = (Block1.Speed * 10000).ToString("G10");
                TextBox_Variable_Speed2.Text = (Block2.Speed * 10000).ToString("G10");
                decimal tempPosBl1 = Block1.Position.X;
                decimal tempPosBl2 = Block2.Position.X;
                Canvas.SetLeft(Border_Block1, tempPosBl1 >= 400 && tempPosBl1 <= tempPosBl2 ? (double)tempPosBl1 : 400);
                Canvas.SetLeft(Border_Block2, tempPosBl2 >= Block1.Size + 400 ? (double)tempPosBl2 : Block1.Size + 400);
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Произошла ошибка");
            }
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
