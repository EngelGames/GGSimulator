using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GGSimulator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>

    public enum TModus {Pause, Play, Stop}

    public enum TStatus { Start, Eintauchen, AnhebenVertauschen, Umfüllen}

    public partial class MainWindow : Window {

        private TModus Modus;
        private TStatus Status;

        private Timer Uhr;

        Image IImagePlay = new Image();
        Image IImagePause = new Image();
        BitmapImage BImagePlay = new BitmapImage();
        BitmapImage BImagePause = new BitmapImage();

        List<double> WerteL = new List<double>();
        List<double> WerteR = new List<double>();
        int Schritt, Intervall, StartVolumenL, StartVolumenR, MaxVolumen;
        double VWanneL, VWanneR, VZylinderL, VZylinderR, RadiusL, RadiusR;

        const double HöheVerhältnis = 2;
        const int Kantenlänge = 10;


        public MainWindow()
        {
            InitializeComponent();

            IImagePlay.Width = 16;
            IImagePause.Width = 16;

            BImagePlay.BeginInit();
            BImagePlay.UriSource = new Uri("assets/Play.png", UriKind.Relative);
            BImagePlay.DecodePixelWidth = 16;
            BImagePlay.EndInit();

            BImagePause.BeginInit();
            BImagePause.UriSource = new Uri("assets/Pause.png", UriKind.Relative);
            BImagePause.DecodePixelWidth = 16;
            BImagePause.EndInit();

            IImagePlay.Source = BImagePlay;
            IImagePause.Source = BImagePause;

            Schritt = 0;
            Status = TStatus.Start;
            Modus = TModus.Stop;
        }

        private void MenuItemBeenden_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /*
            if((ToggleButton)(sender) == ToggleButtonPlay)
            {
                if(Modus == TModus.Play)
                {
                    Pause();
                }
                else if (Modus == TModus.Pause || Modus == TModus.Stop)
                {
                    Start(false);
                }
            }
            */
            if((ToggleButton)(sender) == ToggleButtonStop)
            {
                Stop();
            }
            else if((ToggleButton)(sender) == ToggleButtonSkip)
            {
                if(Modus == TModus.Stop || Modus == TModus.Pause)
                {
                    Einzelschritt();
                }
            }

            AktualisiereButtons();

        }

        private TModus ButtonToModus(ToggleButton Button)
        {
            /*
            if(Button == ToggleButtonPlay && ToggleButtonPlay.IsEnabled)
            {
                if(Modus == TModus.Play)
                {
                    return TModus.Play;
                }
                else
                {
                    return TModus.Pause;
                }
            }
            */
            if(Button == ToggleButtonSkip && ToggleButtonSkip.IsEnabled)
            {
                return Modus;
            }

            return TModus.Stop;
        }

        private void AktualisiereButtons()
        {
            if(Modus == TModus.Stop)
            {
                ToggleButtonStop.IsEnabled = false;
                //ToggleButtonPlay.IsEnabled = true;
                ToggleButtonSkip.IsEnabled = true;

                //ToggleButtonPlay.IsChecked = false;
                ToggleButtonSkip.IsChecked = false;
                ToggleButtonStop.IsChecked = false;

                TextBoxVolumen1.IsEnabled = true;
                TextBoxVolumen2.IsEnabled = true;
                TextBoxRadius1.IsEnabled = true;
                TextBoxRadius2.IsEnabled = true;

                SliderVolumen1.IsEnabled = true;
                SliderVolumen2.IsEnabled = true;
                SliderRadius1.IsEnabled = true;
                SliderRadius2.IsEnabled = true;

                //ToggleButtonPlay.Content = IImagePlay;
            }

            if (Modus == TModus.Play)
            {
                ToggleButtonStop.IsEnabled = true;
                //ToggleButtonPlay.IsEnabled = true;
                ToggleButtonSkip.IsEnabled = false;

                //ToggleButtonPlay.IsChecked = false;
                ToggleButtonSkip.IsChecked = false;
                ToggleButtonStop.IsChecked = false;

                TextBoxVolumen1.IsEnabled = false;
                TextBoxVolumen2.IsEnabled = false;
                TextBoxRadius1.IsEnabled = false;
                TextBoxRadius2.IsEnabled = false;

                SliderVolumen1.IsEnabled = false;
                SliderVolumen2.IsEnabled = false;
                SliderRadius1.IsEnabled = false;
                SliderRadius2.IsEnabled = false;

                //ToggleButtonPlay.Content = IImagePause;
            }

            if(Modus == TModus.Pause)
            {
                ToggleButtonStop.IsEnabled = true;
                //ToggleButtonPlay.IsEnabled = true;
                ToggleButtonSkip.IsEnabled = true;

                //ToggleButtonPlay.IsChecked = false;
                ToggleButtonSkip.IsChecked = false;
                ToggleButtonStop.IsChecked = false;

                TextBoxVolumen1.IsEnabled = false;
                TextBoxVolumen2.IsEnabled = false;
                TextBoxRadius1.IsEnabled = false;
                TextBoxRadius2.IsEnabled = false;

                SliderVolumen1.IsEnabled = false;
                SliderVolumen2.IsEnabled = false;
                SliderRadius1.IsEnabled = false;
                SliderRadius2.IsEnabled = false;

                //ToggleButtonPlay.Content = IImagePlay;
            }
        }

        double MinX, MaxX, MinY, MaxY;
        double DX, X0, DY, Y0;
        double rand = 20.0, StiftDicke = 2;

        void Zeichne(bool graph)
        {
            CanvasGraph.Children.Clear();
            SetzeParameter();
            SetzeSkalierung();
            ZeichneAchsenkreuz();
            if (graph)
            {
                ZeichneSchaubildL();
                ZeichneSchaubildR();
            }
            
        }

        void SetzeParameter()
        {
            MinX = 0.0;
            if(Math.Max(WerteL.Count, WerteR.Count) > 5)
            {
                MaxX = Math.Max(WerteL.Count, WerteR.Count);
            }
            else
            {
                MaxX = 5;
            }
            
            MinY = 0;
            if((WerteL.Count + WerteR.Count) > 0)
            {
                if (Math.Max(WerteL.Max(), WerteR.Max()) > 100)
                {
                    MaxY = Math.Max(WerteL.Max(), WerteR.Max());
                }
                else
                {
                    MaxY = 100;
                }
            }
            else
            {
                MaxY = 100;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Zeichne(false);
        }

        void SetzeSkalierung()
        {
            DX = (CanvasGraph.ActualWidth - 2 * rand) / (MaxX - MinX);
            X0 = rand - DX * MinX;
            DY = (CanvasGraph.ActualHeight - 2 * rand) / (MinY - MaxY);
            Y0 = rand - DY * MaxY;
        }

        Point PlottoCanvas(Point P)
        {
            return new Point(DX * P.X + X0, DY * P.Y + Y0);
        }

        private void SliderVolumen1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                TextBoxVolumen1.Text = Math.Round(SliderVolumen1.Value).ToString();
                VWanneL = int.Parse(TextBoxVolumen1.Text.ToString());
                VZylinderL = Math.PI * Math.Pow(RadiusL / 10, 2) * (VWanneL / (Math.Pow(Kantenlänge, 2)));
                VZylinderR = Math.PI * Math.Pow(RadiusR / 10, 2) * (VWanneR / (Math.Pow(Kantenlänge, 2)));
                setWaterWanneL();
                setWaterZylinderL();
            }
            catch
            {

            }
        }

        private void SliderRadius1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
            try
            {
                TextBoxRadius1.Text = Math.Round(SliderRadius1.Value).ToString();
                RadiusL = int.Parse(TextBoxRadius1.Text.ToString());
                VZylinderL = Math.PI * Math.Pow(RadiusL / 10, 2) * (VWanneL / (Math.Pow(Kantenlänge, 2)));
                VZylinderR = Math.PI * Math.Pow(RadiusR / 10, 2) * (VWanneR / (Math.Pow(Kantenlänge, 2)));
                setZylinderBreiteL();
                setWaterZylinderL();
            }
            catch
            {

            }
        }

        private void SliderVolumen2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
            try
            {
                TextBoxVolumen2.Text = Math.Round(SliderVolumen2.Value).ToString();
                VWanneR = int.Parse(TextBoxVolumen2.Text.ToString());
                VZylinderL = Math.PI * Math.Pow(RadiusL / 10, 2) * (VWanneL / (Math.Pow(Kantenlänge, 2)));
                VZylinderR = Math.PI * Math.Pow(RadiusR / 10, 2) * (VWanneR / (Math.Pow(Kantenlänge, 2)));
                setWaterWanneR();
                setWaterZylinderR();
            }
            catch
            {

            }
        }

        private void SliderRadius2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
            try
            {
                TextBoxRadius2.Text = Math.Round(SliderRadius2.Value).ToString();
                RadiusR = int.Parse(TextBoxRadius2.Text.ToString());
                VZylinderL = Math.PI * Math.Pow(RadiusL / 10, 2) * (VWanneL / (Math.Pow(Kantenlänge, 2)));
                VZylinderR = Math.PI * Math.Pow(RadiusR / 10, 2) * (VWanneR / (Math.Pow(Kantenlänge, 2)));
                setZylinderBreiteR();
                setWaterZylinderR();
            }
            catch
            {

            }
        }

        private void TextBoxVolumen1_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SliderVolumen1.Value = int.Parse(TextBoxVolumen1.Text.ToString());
                VWanneL = int.Parse(TextBoxVolumen1.Text.ToString());
                VZylinderL = Math.PI * Math.Pow(RadiusL / 10, 2) * (VWanneL / (Math.Pow(Kantenlänge, 2)));
                VZylinderR = Math.PI * Math.Pow(RadiusR / 10, 2) * (VWanneR / (Math.Pow(Kantenlänge, 2)));
                setWaterWanneL();
                setWaterZylinderL();
            }
            catch
            {

            }
        }

        private void TextBoxVolumen2_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SliderVolumen2.Value = int.Parse(TextBoxVolumen2.Text.ToString());
                VWanneR = int.Parse(TextBoxVolumen2.Text.ToString());
                VZylinderL = Math.PI * Math.Pow(RadiusL / 10, 2) * (VWanneL / (Math.Pow(Kantenlänge, 2)));
                VZylinderR = Math.PI * Math.Pow(RadiusR / 10, 2) * (VWanneR / (Math.Pow(Kantenlänge, 2)));
                setWaterWanneR();
                setWaterZylinderR();
            }
            catch
            {

            }
        }

        private void TextBoxRadius1_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SliderRadius1.Value = int.Parse(TextBoxRadius1.Text.ToString());
                RadiusL = int.Parse(TextBoxRadius1.Text.ToString());
                VZylinderL = Math.PI * Math.Pow(RadiusL / 10, 2) * (VWanneL / (Math.Pow(Kantenlänge, 2)));
                VZylinderR = Math.PI * Math.Pow(RadiusR / 10, 2) * (VWanneR / (Math.Pow(Kantenlänge, 2)));
                setZylinderBreiteL();
                setWaterZylinderL();
            }
            catch
            {

            }
        }

        private void MenuItemDrucken_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog prnt = new PrintDialog();

            if (prnt.ShowDialog() == true)
            {
                prnt.PrintVisual(CanvasGraph, "Printing Canvas");
            }
        }

        private void TextBoxRadius2_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SliderRadius2.Value = int.Parse(TextBoxRadius2.Text.ToString());
                RadiusR = int.Parse(TextBoxRadius2.Text.ToString());
                VZylinderL = Math.PI * Math.Pow(RadiusL / 10, 2) * (VWanneL / (Math.Pow(Kantenlänge, 2)));
                VZylinderR = Math.PI * Math.Pow(RadiusR / 10, 2) * (VWanneR / (Math.Pow(Kantenlänge, 2)));
                setZylinderBreiteR();
                setWaterZylinderR();
            }
            catch
            {

            }
        }

        void ZeichneAchsenkreuz()
        {
            double d;
            Point p1, p2, s1, s2, s3;
            int Tickmin, Tickmax, I, Tickminx, Tickmaxx;
            TextBlock Tick;
            d = rand / 2.0;

            //X-Achse
            p1 = PlottoCanvas(new Point(MinX, 0.0));
            p2 = PlottoCanvas(new Point(MaxX, 0.0));
            ZeichneLinie(p1, p2);

            //X-Achse Pfeilspitze
            s1 = new Point(p2.X, p2.Y + d);
            s2 = new Point(p2.X + d, p2.Y);
            s3 = new Point(p2.X, p2.Y - d);
            ZeichneDreieck(s1, s2, s3);

            //Y-Achse
            p1 = PlottoCanvas(new Point(0.0, MinY - 1));
            p2 = PlottoCanvas(new Point(0.0, MaxY - 1));
            ZeichneLinie(p1, p2);

            //Y-Achse Pfeilspitze
            s1 = new Point(p2.X - d, p2.Y);
            s2 = new Point(p2.X, p2.Y - d);
            s3 = new Point(p2.X + d, p2.Y);
            ZeichneDreieck(s1, s2, s3);

            //yTicks
            Tickmin = (int)Math.Ceiling(MinY + 0.5);
            Tickmax = (int)Math.Floor(MaxY - 0.5);

            int Größe = Tickmax - Tickmin;

            if(Größe <= 20)
            {
                for (I = Tickmin; I <= Tickmax; I++)
                {
                    if (I == 0)
                    {
                        continue;
                    }
                    p1 = PlottoCanvas(new Point(0, I - 1));
                    p2 = p1;
                    p1.X -= d;
                    p2.X += d;
                    ZeichneLinie(p1, p2);
                    Tick = new TextBlock();
                    Tick.Text = (I-1).ToString();
                    Tick.FontSize = 15;
                    Tick.Measure(new Size(double.MaxValue, double.MaxValue));
                    Canvas.SetLeft(Tick, (p1.X - 1) - Tick.DesiredSize.Width / 2);
                    Canvas.SetTop(Tick, p2.Y);
                    CanvasGraph.Children.Add(Tick);
                }
            }
            else if(Größe < 50)
            {
                for (I = Tickmin; I <= Tickmax; I += 2)
                {
                    if (I == 0)
                    {
                        continue;
                    }
                    p1 = PlottoCanvas(new Point(0, I-1));
                    p2 = p1;
                    p1.X -= d;
                    p2.X += d;
                    ZeichneLinie(p1, p2);
                    Tick = new TextBlock();
                    Tick.Text = (I-1).ToString();
                    Tick.FontSize = 15;
                    Tick.Measure(new Size(double.MaxValue, double.MaxValue));
                    Canvas.SetLeft(Tick, (p1.X - 1) - Tick.DesiredSize.Width / 2);
                    Canvas.SetTop(Tick, p2.Y);
                    CanvasGraph.Children.Add(Tick);
                }
            }
            else if (Größe < 100)
            {
                for (I = Tickmin; I <= Tickmax; I += 5)
                {
                    if (I == 0)
                    {
                        continue;
                    }
                    p1 = PlottoCanvas(new Point(0, I - 1));
                    p2 = p1;
                    p1.X -= d;
                    p2.X += d;
                    ZeichneLinie(p1, p2);
                    Tick = new TextBlock();
                    Tick.Text = (I-1).ToString();
                    Tick.FontSize = 15;
                    Tick.Measure(new Size(double.MaxValue, double.MaxValue));
                    Canvas.SetLeft(Tick, (p1.X - 1) - Tick.DesiredSize.Width / 2);
                    Canvas.SetTop(Tick, p2.Y);
                    CanvasGraph.Children.Add(Tick);
                }
            }
            else if (Größe < 500)
            {
                for (I = Tickmin; I <= Tickmax; I += 25)
                {
                    if (I == 0)
                    {
                        continue;
                    }
                    p1 = PlottoCanvas(new Point(0, I-1));
                    p2 = p1;
                    p1.X -= d;
                    p2.X += d;
                    ZeichneLinie(p1, p2);
                    Tick = new TextBlock();
                    Tick.Text = (I-1).ToString();
                    Tick.FontSize = 15;
                    Tick.Measure(new Size(double.MaxValue, double.MaxValue));
                    Canvas.SetLeft(Tick, (p1.X - 1) - Tick.DesiredSize.Width / 2);
                    Canvas.SetTop(Tick, p2.Y);
                    CanvasGraph.Children.Add(Tick);
                }
            }
            else if (Größe < 2000)
            {
                for (I = Tickmin; I <= Tickmax; I += 100)
                {
                    if (I == 0)
                    {
                        continue;
                    }
                    p1 = PlottoCanvas(new Point(0, I-1));
                    p2 = p1;
                    p1.X -= d;
                    p2.X += d;
                    ZeichneLinie(p1, p2);
                    Tick = new TextBlock();
                    Tick.Text = (I-1).ToString();
                    Tick.FontSize = 15;
                    Tick.Measure(new Size(double.MaxValue, double.MaxValue));
                    Canvas.SetLeft(Tick, (p1.X - 10) - Tick.DesiredSize.Width / 2);
                    Canvas.SetTop(Tick, p2.Y);
                    CanvasGraph.Children.Add(Tick);
                }
            }



            //xTicks
            Tickmin = (int)Math.Ceiling(MinX + 0.5);
            Tickmax = (int)Math.Floor(MaxX - 0.5);

            Größe = Tickmax - Tickmin;

            if(Größe <= 20)
            {
                for (I = Tickmin; I <= Tickmax; I++)
                {
                    if (I == 0)
                    {
                        continue;
                    }
                    p1 = PlottoCanvas(new Point(I, 0));
                    p2 = p1;
                    p1.Y -= d;
                    p2.Y += d;
                    ZeichneLinie(p1, p2);
                    Tick = new TextBlock();
                    Tick.Text = I.ToString();
                    Tick.FontSize = 15;
                    Tick.Measure(new Size(double.MaxValue, double.MaxValue));
                    Canvas.SetLeft(Tick, p1.X);
                    Canvas.SetTop(Tick, p2.Y - Tick.DesiredSize.Height / 2);
                    CanvasGraph.Children.Add(Tick);
                }
            }
            else if (Größe < 50)
            {
                for (I = Tickmin; I <= Tickmax; I += 2)
                {
                    if (I == 0)
                    {
                        continue;
                    }
                    p1 = PlottoCanvas(new Point(I, 0));
                    p2 = p1;
                    p1.Y -= d;
                    p2.Y += d;
                    ZeichneLinie(p1, p2);
                    Tick = new TextBlock();
                    Tick.Text = I.ToString();
                    Tick.FontSize = 15;
                    Tick.Measure(new Size(double.MaxValue, double.MaxValue));
                    Canvas.SetLeft(Tick, p1.X);
                    Canvas.SetTop(Tick, p2.Y - Tick.DesiredSize.Height / 2);
                    CanvasGraph.Children.Add(Tick);
                }
            }
            else if (Größe < 100)
            {
                for (I = Tickmin; I <= Tickmax; I += 5)
                {
                    if (I == 0)
                    {
                        continue;
                    }
                    p1 = PlottoCanvas(new Point((I - 1), 0));
                    p2 = p1;
                    p1.Y -= d;
                    p2.Y += d;
                    ZeichneLinie(p1, p2);
                    Tick = new TextBlock();
                    Tick.Text = (I - 1).ToString();
                    Tick.FontSize = 15;
                    Tick.Measure(new Size(double.MaxValue, double.MaxValue));
                    Canvas.SetLeft(Tick, p1.X);
                    Canvas.SetTop(Tick, p2.Y - Tick.DesiredSize.Height / 2);
                    CanvasGraph.Children.Add(Tick);
                }
            }
            else if (Größe < 500)
            {
                for (I = Tickmin; I <= Tickmax; I += 50)
                {
                    if (I == 0)
                    {
                        continue;
                    }
                    p1 = PlottoCanvas(new Point((I - 1), 0));
                    p2 = p1;
                    p1.Y -= d;
                    p2.Y += d;
                    ZeichneLinie(p1, p2);
                    Tick = new TextBlock();
                    Tick.Text = (I - 1).ToString();
                    Tick.FontSize = 15;
                    Tick.Measure(new Size(double.MaxValue, double.MaxValue));
                    Canvas.SetLeft(Tick, p1.X);
                    Canvas.SetTop(Tick, p2.Y - Tick.DesiredSize.Height / 2);
                    CanvasGraph.Children.Add(Tick);
                }
            }

            
        }

        void ZeichneLinie(Point p1, Point p2)
        {
            Line Linie = new Line();

            Linie.X1 = p1.X;
            Linie.X2 = p2.X;
            Linie.Y1 = p1.Y;
            Linie.Y2 = p2.Y;

            Linie.Stroke = Brushes.Black;
            Linie.StrokeThickness = StiftDicke;

            CanvasGraph.Children.Add(Linie);
        }

        void ZeichneDreieck(Point s1, Point s2, Point s3)
        {
            Polygon Dreieck = new Polygon();

            Dreieck.Points.Add(s1);
            Dreieck.Points.Add(s2);
            Dreieck.Points.Add(s3);

            Dreieck.Stroke = Brushes.Black;
            Dreieck.StrokeThickness = StiftDicke;
            Dreieck.Fill = Brushes.Black;

            CanvasGraph.Children.Add(Dreieck);
        }

        void ZeichneSchaubildL()
        {
            int schritte = WerteL.Count;
            int i;
            double deltax, x, y;
            Point PlotPoint, ClientPoint;
            Polyline Kurve = new Polyline();

            Kurve.Stroke = Brushes.Red;
            Kurve.StrokeThickness = StiftDicke;
            Kurve.StrokeLineJoin = PenLineJoin.Round;

            deltax = (MaxX - MinX) / schritte;
            x = MinX;

            foreach (double Wert in WerteL)
            {
                y = Wert;
                PlotPoint = new Point(x, y);
                ClientPoint = PlottoCanvas(PlotPoint);
                Kurve.Points.Add(ClientPoint);
                x += deltax;
            }

            CanvasGraph.Children.Add(Kurve);
        }

        void ZeichneSchaubildR()
        {
            int schritte = WerteR.Count;
            int i;
            double deltax, x, y;
            Point PlotPoint, ClientPoint;
            Polyline Kurve = new Polyline();

            Kurve.Stroke = Brushes.LimeGreen;
            Kurve.StrokeThickness = StiftDicke;
            Kurve.StrokeLineJoin = PenLineJoin.Round;

            deltax = (MaxX - MinX) / schritte;
            x = MinX;

            foreach (double Wert in WerteR)
            {
                y = Wert;
                PlotPoint = new Point(x, y);
                ClientPoint = PlottoCanvas(PlotPoint);
                Kurve.Points.Add(ClientPoint);
                x += deltax;
            }

            CanvasGraph.Children.Add(Kurve);
        }

        string StatustoString(TStatus AStatus)
        {
            if (AStatus == TStatus.Start)
            {
                return "Startposition";
            } 
            else if (AStatus == TStatus.Eintauchen)
            {
                return "Nach dem Eintauchen";
            }
            else if (AStatus == TStatus.AnhebenVertauschen)
            {
                return "Zylinder wurden angebohen und vertauscht";
            }
            else if (AStatus == TStatus.Umfüllen)
            {
                return "Wasser wurde umgefüllt";
            }
            else
            {
                return "";
            }
        }

        void Einzelschritt()
        {
            
            LabelStatus.Content = StatustoString(Status);

            if (Status == TStatus.Start)
            {
                Start(true);

                WerteL.Add(VWanneL);
                WerteR.Add(VWanneR);

                setWaterWanneL();
                setWaterWanneR();

                updateInfos();
            }
            else if (Status == TStatus.Eintauchen)
            {
                VZylinderL = Math.PI * Math.Pow(RadiusL / 10, 2) * (VWanneL / (Math.Pow(Kantenlänge, 2)));
                VZylinderR = Math.PI * Math.Pow(RadiusR / 10, 2) * (VWanneR / (Math.Pow(Kantenlänge, 2)));

                if(Schritt >= 1)
                {
                    ZylinderSwapBack();
                }
                ZylinderUnten();

                setWaterWanneL();
                setWaterWanneR();
                setWaterZylinderL();
                setWaterZylinderR();

                updateInfos();
            }
            else if(Status == TStatus.AnhebenVertauschen)
            {
                VWanneL -= VZylinderL;
                VWanneR -= VZylinderR;

                ZylinderHoch();
                ZylinderSwap();

                setWaterWanneL();
                setWaterWanneR();

                updateInfos();
            }
            else if (Status == TStatus.Umfüllen)
            {
                VWanneL += VZylinderR;
                VWanneR += VZylinderL;

                VZylinderR = 0;
                VZylinderL = 0;

                WerteL.Add(VWanneL);
                WerteR.Add(VWanneR);

                Schritt++;
                LabelSchritt.Content = Schritt.ToString();

                setWaterWanneL();
                setWaterWanneR();
                setWaterZylinderL();
                setWaterZylinderR();

                updateInfos();

                Zeichne(true);
            }

            if (Status == TStatus.Eintauchen)
            {
                Status = TStatus.AnhebenVertauschen;
            }
            else if (Status == TStatus.AnhebenVertauschen)
            {
                Status = TStatus.Umfüllen;
            }
            else if (Status == TStatus.Umfüllen)
            {
                Status = TStatus.Eintauchen;
            }
            else if (Status == TStatus.Start)
            {
                Status = TStatus.Eintauchen;
            }
        }

        void updateInfos()
        {
            LabelVWanneL.Content = Math.Round(VWanneL, 2).ToString();
            LabelVWanneR.Content = Math.Round(VWanneR, 2).ToString();
            LabelZylinderL.Content = Math.Round(VZylinderL, 2).ToString();
            LabelVZylinderR.Content = Math.Round(VZylinderR, 2).ToString();
        }

        bool starteTimer()
        {
            try
            {
                Intervall = int.Parse(TextBoxIntervall.Text);
            }
            catch
            {
                MessageBox.Show("Bitte gib nur eine gültige Zahl ein");
                return false;
            }

            if(Intervall < 1)
            {
                MessageBox.Show("Bitte gib eine Zahl höher als 1ms ein");
            }

            TextBoxIntervall.IsEnabled = false;

            Uhr = new System.Timers.Timer(Intervall);

            Uhr.Elapsed += OnTimedEvent;
            Uhr.AutoReset = true;
            Uhr.Enabled = true;

            return true;
        }

        private void ButtonGroßerSchritt_Click(object sender, RoutedEventArgs e)
        {
            Modus = TModus.Pause;
            AktualisiereButtons();
            Einzelschritt();
            Einzelschritt();
            Einzelschritt();
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Einzelschritt();
        }

        bool stoppeTimer()
        {
            if (Uhr != null)
            {
                if (Uhr.Enabled)
                {
                    Uhr.Stop();
                    Uhr.Dispose();
                }
            }

            return true;
        }

        bool Einlesen()
        {
            try
            {
                StartVolumenL = int.Parse(TextBoxVolumen1.Text);
                if(StartVolumenL < 0 || StartVolumenL > 1000)
                {
                    MessageBox.Show("Das eingegebene Volumen muss zwischen 1 und 1000ml sein");
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("Das eingegebene Volumen muss zwischen 1 und 1000ml sein");
                return false;
            }

            try
            {
                StartVolumenR = int.Parse(TextBoxVolumen2.Text);
                if (StartVolumenR < 0 || StartVolumenR > 1000)
                {
                    MessageBox.Show("Das eingegebene Volumen muss zwischen 1 und 1000 ml sein");
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("Das eingegebene Volumen muss zwischen 1 und 1000 ml sein");
                return false;
            }

            try
            {
                RadiusL = int.Parse(TextBoxRadius1.Text);
                if (RadiusL < 5 || RadiusL > 50)
                {
                    MessageBox.Show("Der eingegebene Radius muss zwischen 5 und 50 ml sein");
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("Der eingegebene Radius muss zwischen 5 und 50 ml sein");
                return false;
            }

            try
            {
                RadiusR = int.Parse(TextBoxRadius2.Text);
                if (RadiusR < 5 || RadiusR > 50)
                {
                    MessageBox.Show("Der eingegebene Radius muss zwischen 5 und 50 ml sein");
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("Der eingegebene Radius muss zwischen 5 und 50 ml sein");
                return false;
            }

            try
            {
                Intervall = int.Parse(TextBoxIntervall.Text);
            }
            catch
            {
                MessageBox.Show("Das eingegebene Intervall ist keine zulässige Zahl");
            }

            return true;
        }

        void Start(bool einzeln)
        {
            if(Modus == TModus.Stop)
            {
                if (Einlesen())
                {
                    MaxVolumen = StartVolumenL + StartVolumenR;

                    //Kantenlänge = Math.Pow((MaxVolumen / HöheVerhältnis), (1.0 / 3.0));

                    //LabelKantenlänge.Content = Kantenlänge.ToString();

                    VWanneL = StartVolumenL;
                    VWanneR = StartVolumenR;

                    LabelVWanneL.Content = StartVolumenL.ToString();
                    LabelVWanneR.Content = StartVolumenR.ToString();

                    if (!einzeln)
                    {
                        Modus = TModus.Play;

                        starteTimer();
                    }
                    else
                    {
                        Modus = TModus.Pause;
                        //Einzelschritt();
                    }
                }
            }
            else if(Modus == TModus.Pause)
            {
                Modus = TModus.Play;

                if (!einzeln)
                {
                    starteTimer();
                }
                else
                {
                    Modus = TModus.Pause;
                    //Einzelschritt();
                }
            }
            
        }

        void Pause()
        {
            Modus = TModus.Pause;

            stoppeTimer();
        }

        void Stop()
        {
            Modus = TModus.Stop;
            

            VWanneL = 0;
            VWanneR = 0;
            VZylinderL = 0;
            VZylinderR = 0;

            WerteL.Clear();
            WerteR.Clear();

            if(Schritt >= 1)
            {
                if (Status == TStatus.Umfüllen || Status == TStatus.Eintauchen)
                {
                    ZylinderSwapBack();
                    ZylinderUnten();
                }
            }

            Status = TStatus.Start;
            Schritt = 0;

            LabelStatus.Content = StatustoString(Status);
            LabelSchritt.Content = Schritt.ToString();

            Zeichne(false);
            updateInfos();
            stoppeTimer();
        }

        void ZylinderHoch()
        {
            double höheL = VZylinderL / (Math.PI * Math.Pow(RadiusL / 10, 2));
            ZylinderL1.Y1 = 400;
            ZylinderL2.Y1 = 400;
            ZylinderL1.Y2 = 0;
            ZylinderL2.Y2 = 0;
            Canvas.SetTop(ZylinderLE1, -15);
            Canvas.SetTop(ZylinderLE2, 385);
            Canvas.SetTop(ZylinderLE1Fill, (385 - 20 * höheL));
            Canvas.SetTop(ZylinderLE2Fill, 385);
            ZylinderLFill.Height = 20 * höheL;
            Canvas.SetTop(ZylinderLFill, 400 - 20 * höheL);

            double höheR = VZylinderL / (Math.PI * Math.Pow(RadiusR / 10, 2));
            ZylinderR1.Y1 = 400;
            ZylinderR2.Y1 = 400;
            ZylinderR1.Y2 = 0;
            ZylinderR2.Y2 = 0;
            Canvas.SetTop(ZylinderRE1, -15);
            Canvas.SetTop(ZylinderRE2, 385);
            Canvas.SetTop(ZylinderRE1Fill, (385 - 20 * höheR));
            Canvas.SetTop(ZylinderRE2Fill, 385);
            ZylinderRFill.Height = 20 * höheR;
            Canvas.SetTop(ZylinderRFill, 400 - 20 * höheR);
        }

        void ZylinderSwap()
        {
            ZylinderL1.X1 += 400;
            ZylinderL1.X2 += 400;
            ZylinderL2.X1 += 400;
            ZylinderL2.X2 += 400;
            Canvas.SetLeft(ZylinderLE1, 712);
            Canvas.SetLeft(ZylinderLE2, 712);
            Canvas.SetLeft(ZylinderLE1Fill, 712);
            Canvas.SetLeft(ZylinderLE2Fill, 712);
            Canvas.SetLeft(ZylinderLFill, 715);

            ZylinderR1.X1 -= 400;
            ZylinderR1.X2 -= 400;
            ZylinderR2.X1 -= 400;
            ZylinderR2.X2 -= 400;
            Canvas.SetLeft(ZylinderRE1, 312);
            Canvas.SetLeft(ZylinderRE2, 312);
            Canvas.SetLeft(ZylinderRE1Fill, 312);
            Canvas.SetLeft(ZylinderRE2Fill, 312);
            Canvas.SetLeft(ZylinderRFill, 315);
        }

        void ZylinderSwapBack()
        {
            ZylinderR1.X1 += 400;
            ZylinderR1.X2 += 400;
            ZylinderR2.X1 += 400;
            ZylinderR2.X2 += 400;
            Canvas.SetLeft(ZylinderRE1, 712);
            Canvas.SetLeft(ZylinderRE2, 712);
            Canvas.SetLeft(ZylinderRE1Fill, 712);
            Canvas.SetLeft(ZylinderRE2Fill, 712);
            Canvas.SetLeft(ZylinderRFill, 715);

            ZylinderL1.X1 -= 400;
            ZylinderL1.X2 -= 400;
            ZylinderL2.X1 -= 400;
            ZylinderL2.X2 -= 400;
            Canvas.SetLeft(ZylinderLE1, 312);
            Canvas.SetLeft(ZylinderLE2, 312);
            Canvas.SetLeft(ZylinderLE1Fill, 312);
            Canvas.SetLeft(ZylinderLE2Fill, 312);
            Canvas.SetLeft(ZylinderLFill, 315);
        }

        void ZylinderUnten()
        {
            double höheL = VZylinderL / (Math.PI * Math.Pow(RadiusL / 10, 2));
            ZylinderL1.Y1 = 800;
            ZylinderL2.Y1 = 800;
            ZylinderL1.Y2 = 400;
            ZylinderL2.Y2 = 400;
            Canvas.SetTop(ZylinderLE1, 385);
            Canvas.SetTop(ZylinderLE2, 785);
            Canvas.SetTop(ZylinderLFill, 800 - 20 * höheL);
            Canvas.SetTop(ZylinderLE1Fill, 785 - 20 * höheL);
            Canvas.SetTop(ZylinderLE2Fill, 785);

            double höheR = VZylinderR / (Math.PI * Math.Pow(RadiusR / 10, 2));
            ZylinderR1.Y1 = 800;
            ZylinderR2.Y1 = 800;
            ZylinderR1.Y2 = 400;
            ZylinderR2.Y2 = 400;
            Canvas.SetTop(ZylinderRE1, 385);
            Canvas.SetTop(ZylinderRE2, 785);
            Canvas.SetTop(ZylinderRFill, 800 - 20 * höheR);
            Canvas.SetTop(ZylinderRE1Fill, 785 - 20 * höheR);
            Canvas.SetTop(ZylinderRE2Fill, 785);
        }

        void setWaterWanneL()
        {
            double höhe = VWanneL / Math.Pow(Kantenlänge, 2);
            RectangleL1.Height = 20 * höhe;
            Canvas.SetTop(RectangleL1, 450 + (400 - 20 * höhe));
            RectangleL2.Height = 20 * höhe;
            Canvas.SetTop(RectangleL2, 350 + (400 - 20 * höhe));

            Point p1 = new Point(200, 850);
            Point p2 = new Point(300, 750);
            Point p3 = new Point(300, 350 + (400 - 20 * höhe));
            Point p4 = new Point(200, 450 + (400 - 20 * höhe));
            PointCollection pc1 = new PointCollection();
            pc1.Add(p1);
            pc1.Add(p2);
            pc1.Add(p3);
            pc1.Add(p4);
            PolygonL1.Points = pc1;

            PointCollection pc2 = new PointCollection();
            Point p5 = new Point(400, 850);
            Point p6 = new Point(500, 750);
            Point p7 = new Point(500, 350 + (400 - 20 * höhe));
            Point p8 = new Point(400, 450 + (400 - 20 * höhe));
            pc2.Add(p5);
            pc2.Add(p6);
            pc2.Add(p7);
            pc2.Add(p8);
            PolygonR2.Points = pc2;
        }

        void setWaterWanneR()
        {
            double höhe = VWanneR / Math.Pow(Kantenlänge, 2);
            RectangleR1.Height = 20 * höhe;
            Canvas.SetTop(RectangleR1, 450 + (400 - 20 * höhe));
            RectangleR2.Height = 20 * höhe;
            Canvas.SetTop(RectangleR2, 350 + (400 - 20 * höhe));

            Point p1 = new Point(600, 850);
            Point p2 = new Point(700, 750);
            Point p3 = new Point(700, 350 + (400 - 20 * höhe));
            Point p4 = new Point(600, 450 + (400 - 20 * höhe));
            PointCollection pc1 = new PointCollection();
            pc1.Add(p1);
            pc1.Add(p2);
            pc1.Add(p3);
            pc1.Add(p4);
            PolygonR1.Points = pc1;

            PointCollection pc2 = new PointCollection();
            Point p5 = new Point(800, 850);
            Point p6 = new Point(900, 750);
            Point p7 = new Point(900, 350 + (400 - 20 * höhe));
            Point p8 = new Point(800, 450 + (400 - 20 * höhe));
            pc2.Add(p5);
            pc2.Add(p6);
            pc2.Add(p7);
            pc2.Add(p8);
            PolygonL2.Points = pc2;
        }

        void setWaterZylinderL()
        {
            double höhe = VZylinderL / (Math.PI * Math.Pow(RadiusL / 10, 2));
            ZylinderLFill.Height = 20 * höhe;
            Canvas.SetTop(ZylinderLFill, 800 - 20 * höhe);
            if(höhe >= 0)
            {
                Canvas.SetTop(ZylinderLE1Fill, 785 - 20 * höhe);
            }
            else
            {
                Canvas.SetTop(ZylinderLE1Fill, 385);
            }
        }

        void setWaterZylinderR()
        {
            double höhe = VZylinderR / (Math.PI * Math.Pow(RadiusR / 10, 2));
            ZylinderRFill.Height = 20 * höhe;
            Canvas.SetTop(ZylinderRFill, 800 - 20 * höhe);
            if(höhe >= 0)
            {
                Canvas.SetTop(ZylinderRE1Fill, 785 - 20 * höhe);
            }
            else
            {
                Canvas.SetTop(ZylinderRE1Fill, 385);
            }
            
        }

        void setZylinderBreiteL()
        {
            ZylinderL2.X1 = 315 + RadiusL;
            ZylinderL2.X2 = 315 + RadiusL;
            ZylinderLE1.Width = 6 + RadiusL;
            ZylinderLE2.Width = 6 + RadiusL;
            ZylinderLFill.Width = RadiusL;
            ZylinderLE1Fill.Width = 6 + RadiusL;
            ZylinderLE2Fill.Width = 6 + RadiusL;

        }

        void setZylinderBreiteR()
        {
            ZylinderR2.X1 = 715 + RadiusR;
            ZylinderR2.X2 = 715 + RadiusR;
            ZylinderRE1.Width = 6 + RadiusR;
            ZylinderRE2.Width = 6 + RadiusR;
            ZylinderRFill.Width = RadiusR;
            ZylinderRE1Fill.Width = 6 + RadiusR;
            ZylinderRE2Fill.Width = 6 + RadiusR;
        }
    }
}
