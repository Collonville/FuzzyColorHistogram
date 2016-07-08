using OxyPlot;
using OxyPlot.Series;
using System;
using System.Linq;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace FuzzyColorHistogram1
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private List<string> dateSetPath = new List<string>();

        private List<Vector<double>> points = new List<Vector<double>>();
        private Vector<double> rgbHist;
        private Vector<double> labHist;

        private int Division = 4; 

        private string[] _supportExts = { ".bmp", ".jpg", ".jpeg", ".png", ".tif", ".tiff" };
        /// <summary>
        /// 読み込み可能な拡張子リスト
        /// </summary>
        public string[] SupportExts
        {
            get { return _supportExts; }
        }

        public PlotModel ColorHist  { get; private set; }
        public PlotModel LabHist { get; private set; }
        public PlotModel FCHHist { get; private set; }

        /// <summary>
        /// 表示する画像を取得または設定します
        /// </summary>
        public WriteableBitmap ImageSource
        {
            get { return (WriteableBitmap)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(WriteableBitmap), typeof(MainWindow), new PropertyMetadata(null));

        public MainWindow()
        {
            this.DataContext = this;

            ColorHist = new PlotModel();
            LabHist = new PlotModel();
            FCHHist = new PlotModel();

            initGraph();

            //データセットを開く
            //DataSet.openAllDataSet(ref dateSetPath);

            //CalcHistogram();
        }

        private void root_Drop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (files == null)
                return;

            string filename = files[0].ToLower();
            string ext = System.IO.Path.GetExtension(filename);
            if (SupportExts.Any(s => s == ext))
            {
                var img = new BitmapImage(new Uri(filename, UriKind.Absolute));
                ImageSource = BitmapFactory.ConvertToPbgra32Format(img);
                CalcHistogram();
            }
        }

        private void getColorData(ref List<Vector<double>> points)
        {
            for (int y = 0; y < ImageSource.PixelHeight; y++)
            {
                for (int x = 0; x < ImageSource.PixelWidth; x++)
                {
                    var color = ImageSource.GetPixel(x, y);
                    points.Add(new DenseVector(new double[] { color.R, color.G, color.B }));
                }
            }
        }


        private void initGraph()
        {
            ColorHist.Series.Clear();
            ColorHist.InvalidatePlot(true);
            ColorHist.Title = "RGB Histogram";

            LabHist.Series.Clear();
            LabHist.InvalidatePlot(true);
            LabHist.Title = "Lab Histogram";

            FCHHist.Series.Clear();
            FCHHist.InvalidatePlot(true);
            FCHHist.Title = "Fuzzy Color Histogram";
        }
        private Vector<double> createRGBHist(List<Vector<double>> rgbList)
        {
            Vector<double> RGB = new DenseVector((int)Math.Pow(Division, 3.0));

            foreach (var rgb in rgbList)
            {
                int index = (int)(rgb[0] % Division) * Division * Division + (int)(rgb[1] % Division) * Division + (int)(rgb[2] % Division);

                RGB[index]++;
            }

            // グラフの作成
            var graphStep = new StairStepSeries()
            {
                Color = OxyColors.Red,
                VerticalLineStyle = LineStyle.None,
                MarkerType = MarkerType.None
            };

            for (int x = 0; x < RGB.Count(); x++)
            {
                graphStep.Points.Add(new DataPoint(x, RGB[x]));
            }

            ColorHist.Series.Clear();
            ColorHist.Series.Add(graphStep);
            ColorHist.InvalidatePlot(true);

            return RGB;
        }

        private Vector<double> createLabHist(List<Vector<double>> rgbList)
        {
            Vector<double> Lab = new DenseVector((int)Math.Pow(Division, 3.0));

            foreach (var rgb in rgbList)
            {
                Vector<double> LabVec = CIELab.XYZtoLab(XYZ.RGBtoXYZ(rgb));

                int index =
                    ((int)(2.55 * LabVec[0]) % Division) * Division * Division +
                    ((int)(295.37 * LabVec[1] + 255) % Division) * Division +
                    (int)(237.22 * LabVec[2] + 255) % Division;

                Lab[index]++;
            }

            // グラフの作成
            var graphStep = new StairStepSeries()
            {
                Color = OxyColors.Red,
                VerticalLineStyle = LineStyle.None,
                MarkerType = MarkerType.None
            };

            for (int x = 0; x < Lab.Count(); x++)
            {
                graphStep.Points.Add(new DataPoint(x, Lab[x]));
            }

            LabHist.Series.Clear();
            LabHist.Series.Add(graphStep);
            LabHist.InvalidatePlot(true);

            return Lab;
        }

        private void CalcHistogram()
        {
            getColorData(ref points);

            Division = int.Parse(DivisionTextBox.Text);

            rgbHist = createRGBHist(points);
            labHist = createLabHist(points);

            InfoMationTextBlock.Text = "Load Histogram " + (int)Math.Pow(Division, 3.0) + " bins";
        }

        private void StartFCH_Click(object sender, RoutedEventArgs e)
        {
            double fuzzifier = double.Parse(FuzzifierTextBox.Text);
            int clusterNum = int.Parse(ClusterNumTextBox.Text);

            FuzzyCMeans fcm = new FuzzyCMeans(labHist, fuzzifier, clusterNum);

            fcm.run();

            var U = Matrix<double>.Build.DenseOfArray(fcm.U);

            Vector<double> FCH = Vector<double>.Build.Dense(clusterNum);

            FCH = U.Transpose() * rgbHist;

            // グラフの作成
            var graphStep = new StairStepSeries()
            {
                Color = OxyColors.Red,
                VerticalLineStyle = LineStyle.None,
                MarkerType = MarkerType.None
            };

            for (int x = 0; x < FCH.Count(); x++)
            {
                graphStep.Points.Add(new DataPoint(x, FCH[x]));
            }

            FCHHist.Series.Clear();
            FCHHist.Series.Add(graphStep);
            FCHHist.InvalidatePlot(true);

            FCH.ToList().ForEach(x => { Console.WriteLine(x); });
        }

        public void showToDialogTextBox(string str)
        {
            InfoMationTextBlock.Text = str;
        }

        private void CreateHistogramButton_Click(object sender, RoutedEventArgs e)
        {
            CalcHistogram();
        }
    }
}

