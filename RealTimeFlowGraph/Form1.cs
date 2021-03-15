using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ZedGraph;
using LastFilePath;
using System.IO.Pipes;

namespace NewEventTry
{
    public partial class Form1 : Form
    {
        private Thread workerThreadForStartSignal = null;
        private Thread plotDataToChart;
        private NamedPipeServerStream server;
        private string tempFilePath;
        private string signalFromClient;
        private static readonly string OPEN = "Open";
        private static readonly string END = "End";
        private static readonly string NPTEST = "NPtest";
        private bool plotDataToChartCondition;

        public Form1()
        {
            InitializeComponent();
        }

        private void Run_server()
        {
            // Server with python csv Creator Code
            server = new NamedPipeServerStream(NPTEST);
            Console.WriteLine("Waiting for connection...");
            server.WaitForConnection();
            Console.WriteLine("Connected.");
            var readFromClient = new BinaryReader(server);
            plotDataToChartCondition = true;
           
            plotDataToChart = new Thread(() =>
            {
                while (plotDataToChartCondition)
                {
                    var len = (int)readFromClient.ReadUInt32(); 
                    signalFromClient = new string(readFromClient.ReadChars(len));

                    if (signalFromClient == OPEN)
                    {
                        this.workerThreadForStartSignal = new Thread(new ThreadStart(this.StartReadingFile)); // worker thread terminates when function operation done.
                        this.workerThreadForStartSignal.Start();
                    }

                    if (signalFromClient == END)
                    {
                        plotDataToChartCondition = false;
                    }
                }

            });
            plotDataToChart.Start();
        }

        private void Run_server_Closed()
        {
            Console.WriteLine("Client disconnected.");
            server.Close();
            server.Dispose();
            signalFromClient = null;
            Run_server();
        }

        private void StartReadingFile()
        {
            LastCreatedFilePath read = new LastCreatedFilePath();
            tempFilePath = read.NewPath;
            signalFromClient = null;
            ReadCsv(tempFilePath);
        }

        private void ReadCsv(string filePath)
        {

            if (File.Exists(filePath))
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string headerLine = sr.ReadLine();
                    List<String[]> linesFromFile = new List<String[]>(); // it will hold splitted lines from index 0 to ...
                    string[][] dataToPlot = null;
                    int dataCounter = 0;

                    while (true)
                    {
                        if (!sr.EndOfStream)
                        {
                            char[] delimiters = new char[] { ',' };
                            string csvFileLines = sr.ReadLine(); //Read line by line from file until the end of stream.
                            string[] splittedLine = csvFileLines.Split(delimiters, StringSplitOptions.None);

                            linesFromFile.Add(splittedLine);
                            dataToPlot = linesFromFile.ToArray();
                            dataCounter++;
                            if (dataCounter == 20) //when data counter reaches 20 send 2d data array to plot function.
                            {
                                WhenFileReadedPlotChart(dataToPlot);
                                dataCounter = 0;
                                linesFromFile.Clear();
                            }
                        }

                        if (signalFromClient == END)
                        {
                            break; //if coming signal from server equals End , stop reading data from file.
                        }
                    }

                    ClearGraph();
                }
            }
        }

        // Red File dynamically and plot chart
        private void WhenFileReadedPlotChart(string[][] data)
        {
            
            if (zedGraphControl1.GraphPane.CurveList.Count == 0)
            {
                return;
            }
            if (zedGraphControl2.GraphPane.CurveList.Count == 0)
            {
                return;
            }
            if (zedGraphControl3.GraphPane.CurveList.Count == 0)
            {
                return;
            }

            CurveItem curve = zedGraphControl1.GraphPane.CurveList[0];
            if (curve == null)
                return;
            CurveItem curve1 = zedGraphControl2.GraphPane.CurveList[0];
            if (curve1 == null)
                return;
            CurveItem curve2 = zedGraphControl2.GraphPane.CurveList[1];
            if (curve2 == null)
                return;
            CurveItem curve3 = zedGraphControl3.GraphPane.CurveList[0];
            if (curve3 == null)
                return;
            CurveItem curve4 = zedGraphControl3.GraphPane.CurveList[1];
            if (curve4 == null)
                return;

            // Get the PointPairList
            if (!(curve.Points is IPointListEdit list))
                return;
            if (!(curve1.Points is IPointListEdit list1))
                return;
            if (!(curve2.Points is IPointListEdit list2))
                return;
            if (!(curve3.Points is IPointListEdit list3))
                return;
            if (!(curve4.Points is IPointListEdit list4))
                return;

            for (int chartdatacounter = 0; chartdatacounter < data.Length; chartdatacounter++)
            {
                list.Add(int.Parse(data[chartdatacounter][0]), int.Parse(data[chartdatacounter][1], System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")));
                list1.Add(int.Parse(data[chartdatacounter][0]), double.Parse(data[chartdatacounter][2], System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")));
                list2.Add(int.Parse(data[chartdatacounter][0]), double.Parse(data[chartdatacounter][3], System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")));
                list3.Add(int.Parse(data[chartdatacounter][0]), double.Parse(data[chartdatacounter][4], System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")));
                list4.Add(int.Parse(data[chartdatacounter][0]), double.Parse(data[chartdatacounter][5], System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")));
            }

            zedGraphControl1.GraphPane.AxisChange();
            zedGraphControl2.AxisChange();
            zedGraphControl3.AxisChange();

            // Force a redraw
            zedGraphControl1.Invalidate();
            zedGraphControl2.Invalidate();
            zedGraphControl3.Invalidate();
        }

        private void CreateGraph() 
        {
            GraphPane myPane = zedGraphControl1.GraphPane;
            myPane.Title.Text = "Real Distance Values";
            myPane.XAxis.Title.Text = "Point Que";
            myPane.YAxis.Title.Text = "Real Distance";

            GraphPane myPane1 = zedGraphControl2.GraphPane;
            myPane1.Title.Text = "Edges Distances Values";
            myPane1.XAxis.Title.Text = "Point Que";
            myPane1.YAxis.Title.Text = "Edges Distances";

            GraphPane myPane2 = zedGraphControl3.GraphPane;
            myPane2.Title.Text = "Angles Distances Values";
            myPane2.XAxis.Title.Text = "Point Que";
            myPane2.YAxis.Title.Text = "Angles Distances";

            // Save all points.  Eventually this demo will run out of memory, so it automatically
            // stops after 500 seconds.
            PointPairList list = new PointPairList();
            PointPairList list1 = new PointPairList();
            PointPairList list2 = new PointPairList();
            PointPairList list3 = new PointPairList();
            PointPairList list4 = new PointPairList();

            // Initially, a curve is added with no data points (list is empty)
            // Colors are red and blue and there will be no symbols
            LineItem curve = myPane.AddCurve("Real Distance", list, Color.Red, SymbolType.None);
            curve.Line.IsSmooth = true;
            LineItem curve1 = myPane1.AddCurve("Edge 1 Distance", list1, Color.Blue, SymbolType.None);
            curve1.Line.IsSmooth = true;
            LineItem curve2 = myPane1.AddCurve("Edge 2 Distance", list2, Color.Green, SymbolType.None);
            curve2.Line.IsSmooth = true;
            LineItem curve3 = myPane2.AddCurve("Angle 1 Distance", list3, Color.Gray, SymbolType.Circle);
            curve3.Line.IsSmooth = true;
            LineItem curve4 = myPane2.AddCurve("Angle 2 Distance", list4, Color.Black, SymbolType.None);
            curve4.Line.IsSmooth = true;

            myPane.YAxis.Scale.Min = 1200;
            myPane.YAxis.Scale.Max = 1450;

            myPane1.YAxis.Scale.Min = -2000;
            myPane1.YAxis.Scale.Max = 1000;

            myPane2.YAxis.Scale.Min = 250;
            myPane2.YAxis.Scale.Max = 550;

            zedGraphControl1.AxisChange();
            zedGraphControl2.AxisChange();
            zedGraphControl3.AxisChange();
        }

        private void ClearGraph()
        {

            zedGraphControl1.GraphPane.CurveList.Clear();
            zedGraphControl1.GraphPane.GraphObjList.Clear();

            zedGraphControl2.GraphPane.CurveList.Clear();
            zedGraphControl2.GraphPane.GraphObjList.Clear();

            zedGraphControl3.GraphPane.CurveList.Clear();
            zedGraphControl3.GraphPane.GraphObjList.Clear();

            zedGraphControl1.AxisChange();
            zedGraphControl2.AxisChange();
            zedGraphControl3.AxisChange();

            zedGraphControl1.Invalidate();
            zedGraphControl2.Invalidate();
            zedGraphControl3.Invalidate();

            GraphPane myPanezgc1 = zedGraphControl1.GraphPane;
            myPanezgc1.Title.Text = "Real Distance Values";
            myPanezgc1.XAxis.Title.Text = "Point Que";
            myPanezgc1.YAxis.Title.Text = "Real Distance";

            GraphPane myPanezgc2 = zedGraphControl2.GraphPane;
            myPanezgc2.Title.Text = "Real Distance Values";
            myPanezgc2.XAxis.Title.Text = "Point Que";
            myPanezgc2.YAxis.Title.Text = "Real Distance";

            GraphPane myPanezgc3 = zedGraphControl3.GraphPane;
            myPanezgc3.Title.Text = "Angles Distances Values";
            myPanezgc3.XAxis.Title.Text = "Point Que";
            myPanezgc3.YAxis.Title.Text = "Angles Distances";

            PointPairList list = new PointPairList();
            PointPairList list1 = new PointPairList();
            PointPairList list2 = new PointPairList();
            PointPairList list3 = new PointPairList();
            PointPairList list4 = new PointPairList();

            LineItem curve = myPanezgc1.AddCurve("Real Distance", list, Color.Red, SymbolType.None);
            curve.Line.IsSmooth = true;
            LineItem curve1 = myPanezgc2.AddCurve("Edge 1 Distance", list1, Color.Blue, SymbolType.None);
            curve1.Line.IsSmooth = true;
            LineItem curve2 = myPanezgc2.AddCurve("Edge 2 Distance", list2, Color.Green, SymbolType.None);
            curve2.Line.IsSmooth = true;
            LineItem curve3 = myPanezgc3.AddCurve("Angle 1 Distance", list3, Color.Gray, SymbolType.Circle);
            curve3.Line.IsSmooth = true;
            LineItem curve4 = myPanezgc3.AddCurve("Angle 2 Distance", list4, Color.Black, SymbolType.None);
            curve4.Line.IsSmooth = true;

            myPanezgc1.YAxis.Scale.Min = 1200;
            myPanezgc1.YAxis.Scale.Max = 1450;

            myPanezgc2.YAxis.Scale.Min = -2000;
            myPanezgc2.YAxis.Scale.Max = 1000;

            myPanezgc3.YAxis.Scale.Min = 250;
            myPanezgc3.YAxis.Scale.Max = 550;

            zedGraphControl1.AxisChange();
            zedGraphControl2.AxisChange();
            zedGraphControl3.AxisChange();

            Run_server_Closed();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            CreateGraph();
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            SetSize();
        }

        private void SetSize()
        {

            // Control is always 10 pixels inset from the client rectangle of the form
            Rectangle formRect = this.ClientRectangle;
            Rectangle formRect1 = this.ClientRectangle;
            Rectangle formRect2 = this.ClientRectangle;

            formRect.Inflate(-26, -10);
            formRect1.Inflate(-26, -250);
            formRect2.Inflate(-26, -490);

            if (WindowState == FormWindowState.Maximized)
            {

                zedGraphControl1.Location = formRect.Location;
                zedGraphControl1.Size = new Size(1300, 230);

                zedGraphControl2.Location = formRect1.Location;
                zedGraphControl2.Size = new Size(1300, 230);

                zedGraphControl3.Location = formRect2.Location;
                zedGraphControl3.Size = new Size(1300, 230);

            }
            else
            {
                zedGraphControl1.Location = new Point(26, 10);
                zedGraphControl1.Size = new Size(1017, 176);

                zedGraphControl2.Location = new Point(26, 196);
                zedGraphControl2.Size = new Size(1017, 176);

                zedGraphControl3.Location = new Point(26, 382);
                zedGraphControl3.Size = new Size(1017, 176);

            }

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            ButtonClicked();
            Run_server();
        }

        void ButtonClicked()
        {
            Button1.BackColor = Color.Green;
        }
    }

}
