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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using structure_movement_summarizer_esapi_v15_5.ViewModels;
using ScottPlot;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
//using VMS.CA.Scripting;

namespace VMS.TPS
{
    /// <summary>
    /// UserControl1.xaml の相互作用ロジック
    /// </summary>
    public partial class Script : UserControl
    {
        public Script()
        {
            InitializeComponent();

        }
        public void Execute(ScriptContext context, System.Windows.Window window)
        {
            window.Height = 650;
            window.Width = 1150;
            window.Content = this;
            window.Background = Brushes.WhiteSmoke;
            window.SizeChanged += (sender, args) => 
            { 
//                this.Height = window.ActualHeight;
//                this.Width = window.ActualWidth;
                this.Height = window.ActualHeight * 0.95;
                this.Width = window.ActualWidth * 0.98;
            };

            var view_model = this.DataContext as ViewModel;
            view_model.SetScriptContextToModel(context);
            view_model.InstModel.PhaseArray.RePlot += (sender, args) => { RePlotPosDiff(); };
            RePlotPosDiff();

            var plt = wpfplot_amp.Plot;
            plt.Style(figureBackground: System.Drawing.Color.WhiteSmoke,
                dataBackground: System.Drawing.Color.WhiteSmoke);
        }

        public void RePlotPosDiff()
        {
            var view_model = this.DataContext as ViewModel;
            var (x, y_x, y_y, y_z, y_norm) = view_model.InstModel.PhaseArray.GetPosDiffArrays();

            if (x.Count() > 0)
            {
                // Ref: https://scottplot.net/cookbook/4.1/category/multi-axis/
                var plt = wpfplot_amp.Plot;
                plt.Clear();

                var plt_x = plt.AddScatter(x, y_x, label: "X");
                plt_x.YAxisIndex = plt.LeftAxis.AxisIndex;
                plt_x.MarkerShape = MarkerShape.cross;
                plt_x.MarkerSize = 13;
                plt_x.LineStyle = LineStyle.Dash;
                var plt_y = plt.AddScatter(x, y_y, label: "Y");
                plt_y.YAxisIndex = plt.LeftAxis.AxisIndex;
                plt_y.MarkerShape = MarkerShape.triDown;
                plt_y.MarkerSize = 13;
                plt_y.LineStyle = LineStyle.DashDot;
                var plt_z = plt.AddScatter(x, y_z, label: "Z");
                plt_z.YAxisIndex = plt.LeftAxis.AxisIndex;
                plt_z.MarkerShape = MarkerShape.triUp;
                plt_z.LineStyle = LineStyle.DashDotDot;
                plt_z.MarkerSize = 13;
                plt.SetAxisLimits(-5, 100, -20, 20, yAxisIndex: plt.LeftAxis.AxisIndex);

                var plt_norm = plt.AddScatter(x, y_norm, label: "Length");
//                plt_norm.YAxisIndex = plt.RightAxis.AxisIndex;
                plt_norm.YAxisIndex = plt.LeftAxis.AxisIndex;
                plt_norm.MarkerShape = MarkerShape.filledCircle;
                plt_norm.LineStyle = LineStyle.Solid;
                plt_norm.MarkerSize = 10;
                plt_norm.LineWidth = 2;
//                plt.SetAxisLimits(0, 100, 0, 3, yAxisIndex: plt.RightAxis.AxisIndex);

                //                plt.BottomAxis.Ticks(true);
                //                plt.BottomAxis.Color(plt_xyz.Color);
                plt.BottomAxis.Label("Phase [%]");
                plt.LeftAxis.Ticks(true);
                //                plt.LeftAxis.Color(plt_xyz.Color);
                plt.LeftAxis.Label("基準からの距離 [mm]");
                //               plt.RightAxis.Ticks(true);
                //               plt.RightAxis.Color(plt_norm.Color);
                //               plt.RightAxis.Label("L2 Norm [mm]");

                var hline = plt.AddHorizontalLine(10.0);
                hline.Color = System.Drawing.Color.Gold;
                hline.LineWidth = 2;
                //                hline.LineColor = System.Drawing.Color.Yellow;
                //                hline.PositionLabel = true;
                ////                hline.PositionLabelBackground = System.Drawing.Color.Yellow;
                ////                hline.PositionLabelBackground = System.Drawing.Color.Black;
                //                hline.PositionLabelOppositeAxis = true;
                //                Func<double, string> hlineFormatter = a => $"同期照射({a:f0} mm)";
                //                hline.PositionFormatter = hlineFormatter;

                // AddHorizontalLine のラベルがいまいち（色の自由度が低い、長さが変えられないなど）なので、テキスト挿入をする。
//                var text = plt.AddText("Resp. sync", 3, 12, size: 16);
                var text = plt.AddText("同期照射", 3, 12, size: 16);
                text.BackgroundFill = true;
                text.BackgroundColor = System.Drawing.Color.Gold;
                text.Color = System.Drawing.Color.Black;

                plt.Legend();
                wpfplot_amp.Refresh();
            }
        }
    }
}
