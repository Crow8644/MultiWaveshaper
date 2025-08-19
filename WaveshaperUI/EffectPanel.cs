using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace WaveshaperUI
{
    public abstract class EffectPanel : UserControl
    {
        protected Effects.EffectUnion effect;
        //protected PlotModel displayModel;
        public EffectPanel(Effects.EffectType effectType)
        {
            effect = EffectOperations.createEffect(effectType);
            this.DataContext = this;

            this.DisplayModel = new PlotModel
            {
                // Standard Model colors
                Background = OxyColor.FromRgb(60, 50, 60),
                TextColor = OxyColors.White,
            };
            // Declaring model axies:
            Axis lower = new LinearAxis
            {
                Position = AxisPosition.Bottom,     // Axis position
                Minimum = -1,                       // Same range as the effect functions
                Maximum = 1,
                ExtraGridlines = [0d],              // Adds a center line
            };
            Axis side = new LinearAxis
            {
                Position = AxisPosition.Left,       // Axis position
                Minimum = -1.01,                    // Slightly larger range to catch lines that hang near the top
                Maximum = 1.01,
                ExtraGridlines = [0d],              // Adds a center line
            };
            this.DisplayModel.Axes.Add(lower);
            this.DisplayModel.Axes.Add(side);

            // Set series to graph
            this.DisplayModel.Series.Add(MakeDisplaySeries());
        }

        // Converts the current effect function into a C# function that can be passed to the necessary objects
        protected double ProperFunction(double d)
        {
            Microsoft.FSharp.Core.FSharpFunc<double, double> fSharpFunc = Effects.getGraphicsFunction(effect);
            return fSharpFunc.Invoke(d);
        }

        protected void updateModel()
        {
            this.DisplayModel.Series.Clear();
            this.DisplayModel.Series.Add(MakeDisplaySeries());
        }

        public PlotModel DisplayModel { get; protected set; }

        // Makes copies of the display series, since a single series be re-bound to a model
        public FunctionSeries MakeDisplaySeries()
        {
            return new FunctionSeries(ProperFunction, -1.0f, 1.0f, 0.1f);
        }

    }

    public class SliderPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                double x = (double)value;
                x /= 10;

                return x.ToString("N2");    // N2 indicates that we want two decimal places
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s) // Using pattern matching to convert value to string s
            {
                try
                {
                    return double.Parse(s) * 10.0;
                }
                catch
                {
                    return 0.0;
                }
            }
            else
            {
                return 0.0;
            }
        }
    }
}
