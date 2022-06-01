using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using System.Windows.Shapes;
using winforms = System.Windows.Forms;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Drawing;

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
        private (double X, double Y) RatioSpeedGraph;
        private (double X, double Y) RatioPositionGraph;
        private double RatioEnergeGraph;
        double Gr4_Temp;

        private Block Block1;
        private Block Block2;
        private List<(double X, double Y)> Points_Gr12;
        private List<(double X, double Y)> Points_Gr3;
        private List<(double X, double Y)> Points_Gr4;

        private BackgroundWorker Worker = new BackgroundWorker();
        private BackgroundWorker GrafWorker = new BackgroundWorker();
        private Thread LogicalThread;
        private RenderTargetBitmap bmp_Gr1 = new RenderTargetBitmap(1470, 1470, 96, 96, PixelFormats.Pbgra32);
        private RenderTargetBitmap bmp_Gr2 = new RenderTargetBitmap(1470, 1470, 96, 96, PixelFormats.Pbgra32);
        private RenderTargetBitmap bmp_Gr3 = new RenderTargetBitmap(1470, 1470, 96, 96, PixelFormats.Pbgra32);
        private RenderTargetBitmap bmp_Gr4 = new RenderTargetBitmap(1470, 1470, 96, 96, PixelFormats.Pbgra32);

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

            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;
            Worker.DoWork += worker_DoWork;
            Worker.ProgressChanged += worker_ProgressChanged;
            Worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            //GrafWorker.WorkerReportsProgress = true;
            //GrafWorker.DoWork += grafWorker_DoWork;
            //GrafWorker.ProgressChanged += grafWorker_ProgressChanged;
            //GrafWorker.RunWorkerCompleted += grafWorker_RunWorkerCompleted;

            Block1 = new Block(Convert.ToDecimal(TextBox_Initial_Mass1.Text), ((decimal)Canvas.GetLeft(Border_Block1), (int)Canvas.GetTop(Border_Block1)), (int)Border_Block1.Width, 0);
            Block2 = new Block(Convert.ToDecimal(TextBox_Initial_Mass2.Text), ((decimal)Canvas.GetLeft(Border_Block2), (int)Canvas.GetTop(Border_Block2)), (int)Border_Block2.Width, -1m / 10000);

            Points_Gr12 = new List<(double X, double Y)>();
            Points_Gr3 = new List<(double X, double Y)>();
            Points_Gr4 = new List<(double X, double Y)>();

            InitializeOther();
            
        }

        #region Initialize
        private void InitializeOther()
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

            TextBox_Variable_KinEnergy1.Text = (Block1.Mass * (Block1.Speed * 10000) * (Block1.Speed * 10000) / 2).ToString("G10");
            TextBox_Variable_KinEnergy2.Text = (Block2.Mass * (Block2.Speed * 10000) * (Block2.Speed * 10000) / 2).ToString("G10");
            TextBox_Variable_Position1.Text = (Block1.Position.X - 400).ToString("G10");
            TextBox_Variable_Position2.Text = (Block2.Position.X - 400).ToString("G10");
            TextBox_Variable_Speed1.Text = (Block1.Speed * 10000).ToString("G10");
            TextBox_Variable_Speed2.Text = (Block2.Speed * 10000).ToString("G10");

            Graph.EllGraphSp(ref RatioSpeedGraph, ((double)Block1.Speed * 10000, (double)Block2.Speed * 10000), ((double)Block1.Mass, (double)Block2.Mass));
            Graph.EllGraphCi(ref RatioEnergeGraph, ((double)Block1.Speed * 10000, (double)Block2.Speed * 10000), ((double)Block1.Mass, (double)Block2.Mass));
            Graph.EllGraphPo(ref RatioPositionGraph, ((double)Block1.Mass, (double)Block2.Mass));
            Gr4_Temp = RatioPositionGraph.Y > RatioPositionGraph.X ? RatioPositionGraph.Y : RatioPositionGraph.X;

            Canvas.SetTop(Ellipse_Gr1, Math.Sqrt((double)Block1.Mass) * (double)Block1.Speed * 10000 / RatioEnergeGraph * 500 + 720);
            Canvas.SetLeft(Ellipse_Gr1, Math.Sqrt((double)Block2.Mass) * (double)Block2.Speed * 10000 / RatioEnergeGraph * 500 + 720);
            Line_Gr1_BtwEl.X1 = Canvas.GetLeft(Ellipse_Gr1) + 15;
            Line_Gr1_BtwEl.Y1 = Canvas.GetTop(Ellipse_Gr1) + 15;
            Line_Gr1_BtwEl.X2 = Canvas.GetLeft(Ellipse_Gr1) + 15;
            Line_Gr1_BtwEl.Y2 = Canvas.GetTop(Ellipse_Gr1) + 15;


            Canvas.SetTop(Ellipse_Gr2, Math.Sqrt((double)Block1.Mass) * (double)Block1.Speed * 10000 / RatioEnergeGraph * 500 + 720);
            Canvas.SetLeft(Ellipse_Gr2, Math.Sqrt((double)Block2.Mass) * (double)Block2.Speed * 10000 / RatioEnergeGraph * 500 + 720);
            Line_Gr2_BtwEl.X1 = Canvas.GetLeft(Ellipse_Gr2) + 15;
            Line_Gr2_BtwEl.Y1 = Canvas.GetTop(Ellipse_Gr2) + 15;
            Line_Gr2_BtwEl.X2 = Canvas.GetLeft(Ellipse_Gr2) + 15;
            Line_Gr2_BtwEl.Y2 = Canvas.GetTop(Ellipse_Gr2) + 15;

            Canvas.SetTop(Ellipse_Gr3, -((double)Block1.Position.X - 175) * 250 / 600 + 1220);
            Canvas.SetLeft(Ellipse_Gr3, ((double)Block2.Position.X - 400) * 250 / 600 + 220);
            Line_Gr3_BtwEl.X1 = Canvas.GetLeft(Ellipse_Gr3) + 15;
            Line_Gr3_BtwEl.Y1 = Canvas.GetTop(Ellipse_Gr3) + 15;
            Line_Gr3_BtwEl.X2 = Canvas.GetLeft(Ellipse_Gr3) + 15;
            Line_Gr3_BtwEl.Y2 = Canvas.GetTop(Ellipse_Gr3) + 15;

            Canvas.SetTop(Ellipse_Gr4, -((double)Block1.Position.X - 175) * RatioPositionGraph.X * 250 / (Gr4_Temp * 600) + 1220);
            Canvas.SetLeft(Ellipse_Gr4, ((double)Block2.Position.X - 400) * RatioPositionGraph.Y * 250 / (Gr4_Temp * 600) + 220);
            Line_Gr4_BtwEl.X1 = Canvas.GetLeft(Ellipse_Gr4) + 15;
            Line_Gr4_BtwEl.Y1 = Canvas.GetTop(Ellipse_Gr4) + 15;
            Line_Gr4_BtwEl.X2 = Canvas.GetLeft(Ellipse_Gr4) + 15;
            Line_Gr4_BtwEl.Y2 = Canvas.GetTop(Ellipse_Gr4) + 15;

            TB_Gr1_X2.Text = RatioSpeedGraph.X.ToString("G2");
            TB_Gr1_Y2.Text = RatioSpeedGraph.Y.ToString("G2");
            TB_Gr1_X2m.Text = (-RatioSpeedGraph.X).ToString("G2");
            TB_Gr1_Y2m.Text = (-RatioSpeedGraph.Y).ToString("G2");
            TB_Gr1_X1.Text = (RatioSpeedGraph.X / 2).ToString("G2");
            TB_Gr1_Y1.Text = (RatioSpeedGraph.Y / 2).ToString("G2");
            TB_Gr1_X1m.Text = (-RatioSpeedGraph.X / 2).ToString("G2");
            TB_Gr1_Y1m.Text = (-RatioSpeedGraph.Y / 2).ToString("G2");
            bmp_Gr1.Clear();

            TB_Gr2_X2.Text = RatioEnergeGraph.ToString("G2");
            TB_Gr2_Y2.Text = RatioEnergeGraph.ToString("G2");
            TB_Gr2_X2m.Text = (-RatioEnergeGraph).ToString("G2");
            TB_Gr2_Y2m.Text = (-RatioEnergeGraph).ToString("G2");
            TB_Gr2_X1.Text = (RatioEnergeGraph / 2).ToString("G2");
            TB_Gr2_Y1.Text = (RatioEnergeGraph / 2).ToString("G2");
            TB_Gr2_X1m.Text = (-RatioEnergeGraph / 2).ToString("G2");
            TB_Gr2_Y1m.Text = (-RatioEnergeGraph / 2).ToString("G2");
            bmp_Gr2.Clear();
            Points_Gr12.Clear();

            bmp_Gr3.Clear();
            Points_Gr3.Clear();

            TB_Gr4_Y_1.Text = (Gr4_Temp * 600).ToString("G2");
            TB_Gr4_Y_2.Text = (Gr4_Temp * 600 * 2).ToString("G2");
            TB_Gr4_Y_3.Text = (Gr4_Temp * 600 * 3).ToString("G2");
            TB_Gr4_Y_4.Text = (Gr4_Temp * 600 * 4).ToString("G2");
            TB_Gr4_X_1.Text = (Gr4_Temp * 600).ToString("G2");
            TB_Gr4_X_2.Text = (Gr4_Temp * 600 * 2).ToString("G2");
            TB_Gr4_X_3.Text = (Gr4_Temp * 600 * 3).ToString("G2");
            TB_Gr4_X_4.Text = (Gr4_Temp * 600 * 4).ToString("G2");

            if (RatioPositionGraph.Y > RatioPositionGraph.X)
            {
                Line_B1_Gr4.Y2 = -RatioPositionGraph.X * 2400 * 250 / (Gr4_Temp * 600) + 1235;
                Line_B1_Gr4.X2 = 2400 * 250 / 600 + 220;
            }
            else
            {
                Line_B1_Gr4.Y2 = -2400 * 250 / 600 + 1220;
                Line_B1_Gr4.X2 = RatioPositionGraph.Y * 2400 * 250 / (Gr4_Temp * 600) + 235;
            }

            Line_B2_Gr4.Y1 = -RatioPositionGraph.X * 225 * 250 / (Gr4_Temp * 600) + 1235;
            Line_B2_Gr4.Y2 = -RatioPositionGraph.X * 225 * 250 / (Gr4_Temp * 600) + 1235;

            bmp_Gr4.Clear();
            Points_Gr4.Clear();

            Canvas_Gr1.Background = null;
            Canvas_Gr2.Background = null;
            Canvas_Gr2.Background = null;
        }
        #endregion

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
                    InitializeOther();
                    InProcess = true;
                }
                btnImage_Start.Visibility = Visibility.Hidden;
                btnImage_Stop.Visibility = Visibility.Visible;
                Worker.RunWorkerAsync();
                GrafWorker.RunWorkerAsync();
                LogicalThread.Start();
            }
        }
        bool tetsB = false;
        private void btn_Restart_Click(object sender, RoutedEventArgs e)
        {
            if (!Start)
            {
                InProcess = false;
            }
            lock (threadLock)
            {
                InitializeOther();
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
                        Points_Gr12.Add((-Math.Sqrt((double)Block1.Mass) * (double)Block1.Speed * 10000 / RatioEnergeGraph * 500 + 720,
                            Math.Sqrt((double)Block2.Mass) * (double)Block2.Speed * 10000 / RatioEnergeGraph * 500 + 720));
                        Points_Gr3.Add((-((double)Block1.Position.X - 175) * 250 / 600 + 1220, 
                            ((double)Block2.Position.X - 400) * 250 / 600 + 220));
                        Points_Gr4.Add((-((double)Block1.Position.X - 175) * RatioPositionGraph.X * 250 / (Gr4_Temp * 600) + 1220,
                            ((double)Block2.Position.X - 400) * RatioPositionGraph.Y * 250 / (Gr4_Temp * 600) + 220));
                        tetsB = true;
                        Сollisions++;
                    }
                    if (Block1.Position.X + Block1.Size > Block2.Position.X)
                    {
                        Block.Impact(Block1, Block2);
                        Points_Gr12.Add((-Math.Sqrt((double)Block1.Mass) * (double)Block1.Speed * 10000 / RatioEnergeGraph * 500 + 720,
                            Math.Sqrt((double)Block2.Mass) * (double)Block2.Speed * 10000 / RatioEnergeGraph * 500 + 720));
                        Points_Gr3.Add((-((double)Block1.Position.X - 175) * 250 / 600 + 1220, 
                            ((double)Block2.Position.X - 400) * 250 / 600 + 220));
                        Points_Gr4.Add((-((double)Block1.Position.X - 175) * RatioPositionGraph.X * 250 / (Gr4_Temp * 600) + 1220,
                            ((double)Block2.Position.X - 400) * RatioPositionGraph.Y * 250 / (Gr4_Temp * 600) + 220));
                        tetsB = true;
                        Сollisions++;
                    }
                }
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

                TextBox_Variable_Position1.Text = (Block1.Position.X - 400).ToString("G10");
                TextBox_Variable_Position2.Text = (Block2.Position.X - 400).ToString("G10");

                TextBox_Variable_Speed1.Text = (Block1.Speed * 10000).ToString("G10");
                TextBox_Variable_Speed2.Text = (Block2.Speed * 10000).ToString("G10");

                decimal tempPosBl1 = Block1.Position.X;
                decimal tempPosBl2 = Block2.Position.X;

                Canvas.SetLeft(Border_Block1, tempPosBl1 >= 400 && tempPosBl1 <= tempPosBl2 ? (double)tempPosBl1 : 400);
                Canvas.SetLeft(Border_Block2, tempPosBl2 >= Block1.Size + 400 ? (double)tempPosBl2 : Block1.Size + 400);

                Canvas.SetTop(Ellipse_Gr3, -((double)Block1.Position.X - 175) * 250 / 600 + 1220);
                Canvas.SetLeft(Ellipse_Gr3, ((double)Block2.Position.X - 400) * 250 / 600 + 220);
                Line_Gr3_BtwEl.X2 = Canvas.GetLeft(Ellipse_Gr3) + 15;
                Line_Gr3_BtwEl.Y2 = Canvas.GetTop(Ellipse_Gr3) + 15;

                Canvas.SetTop(Ellipse_Gr4, -((double)Block1.Position.X - 175) * RatioPositionGraph.X * 250 / (Gr4_Temp * 600) + 1220);
                Canvas.SetLeft(Ellipse_Gr4, ((double)Block2.Position.X - 400) * RatioPositionGraph.Y * 250 / (Gr4_Temp * 600) + 220);
                Line_Gr4_BtwEl.X2 = Canvas.GetLeft(Ellipse_Gr4) + 15;
                Line_Gr4_BtwEl.Y2 = Canvas.GetTop(Ellipse_Gr4) + 15;
            }
            if (tetsB)
            {
                Line_Gr1_BtwEl.X1 = Canvas.GetLeft(Ellipse_Gr1) + 15;
                Line_Gr1_BtwEl.Y1 = Canvas.GetTop(Ellipse_Gr1) + 15;

                Canvas.SetTop(Ellipse_Gr1, Points_Gr12[0].X);
                Canvas.SetLeft(Ellipse_Gr1, Points_Gr12[0].Y);

                Line_Gr1_BtwEl.X2 = Canvas.GetLeft(Ellipse_Gr1) + 15;
                Line_Gr1_BtwEl.Y2 = Canvas.GetTop(Ellipse_Gr1) + 15;

                lock (threadLock)
                {
                    bmp_Gr1.Render(Ellipse_Gr1);
                    bmp_Gr1.Render(Line_Gr1_BtwEl);
                }
                Canvas_Gr1.Background = new ImageBrush(bmp_Gr1);


                Line_Gr2_BtwEl.X1 = Canvas.GetLeft(Ellipse_Gr2) + 15;
                Line_Gr2_BtwEl.Y1 = Canvas.GetTop(Ellipse_Gr2) + 15;

                Canvas.SetTop(Ellipse_Gr2, Points_Gr12[0].X);
                Canvas.SetLeft(Ellipse_Gr2, Points_Gr12[0].Y);

                Line_Gr2_BtwEl.X2 = Canvas.GetLeft(Ellipse_Gr2) + 15;
                Line_Gr2_BtwEl.Y2 = Canvas.GetTop(Ellipse_Gr2) + 15;

                lock (threadLock)
                {
                    bmp_Gr2.Render(Ellipse_Gr2);
                    bmp_Gr2.Render(Line_Gr2_BtwEl);
                }
                Canvas_Gr2.Background = new ImageBrush(bmp_Gr2);
                Points_Gr12.RemoveAt(0);

                Canvas.SetTop(Ellipse_Gr3, Points_Gr3[0].X);
                Canvas.SetLeft(Ellipse_Gr3, Points_Gr3[0].Y);
                Line_Gr3_BtwEl.X2 = Canvas.GetLeft(Ellipse_Gr3) + 15;
                Line_Gr3_BtwEl.Y2 = Canvas.GetTop(Ellipse_Gr3) + 15;

                lock (threadLock)
                {
                    bmp_Gr3.Render(Ellipse_Gr3);
                    bmp_Gr3.Render(Line_Gr3_BtwEl);
                }
                Canvas_Gr3.Background = new ImageBrush(bmp_Gr3);
                Points_Gr3.RemoveAt(0);

                Line_Gr3_BtwEl.X1 = Canvas.GetLeft(Ellipse_Gr3) + 15;
                Line_Gr3_BtwEl.Y1 = Canvas.GetTop(Ellipse_Gr3) + 15;

                Canvas.SetTop(Ellipse_Gr4, Points_Gr4[0].X);
                Canvas.SetLeft(Ellipse_Gr4, Points_Gr4[0].Y);
                Line_Gr4_BtwEl.X2 = Canvas.GetLeft(Ellipse_Gr4) + 15;
                Line_Gr4_BtwEl.Y2 = Canvas.GetTop(Ellipse_Gr4) + 15;

                lock (threadLock)
                {
                    bmp_Gr4.Render(Ellipse_Gr4);
                    bmp_Gr4.Render(Line_Gr4_BtwEl);
                }
                Canvas_Gr4.Background = new ImageBrush(bmp_Gr4);
                Points_Gr4.RemoveAt(0);

                Line_Gr4_BtwEl.X1 = Canvas.GetLeft(Ellipse_Gr4) + 15;
                Line_Gr4_BtwEl.Y1 = Canvas.GetTop(Ellipse_Gr4) + 15;

                if (Points_Gr12.Count == 0)
                    tetsB = false;
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
