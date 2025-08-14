using NAudio.Gui;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using static Effects;

namespace WaveshaperUI
{
    /// <summary>
    /// Interaction logic for TanhPanel.xaml
    /// More robust documentation in VolumePanel.xaml.cs
    /// </summary>
    public partial class TanhPanel : EffectPanel
    {
        private Effects.Smooth_Distortion_Effect? smooth_effect;

        public TanhPanel() : base(Effects.EffectType.SmoothDistortion)
        {
            smooth_effect = (Effects.Smooth_Distortion_Effect?)EffectOperations.unpackEffect(effect);

            base.DisplayModel.Title = "Hyperbolic Tangent Distortion";
            InitializeComponent();
            if (smooth_effect != null) FactorSlider.Value = SliderExponentialConverter.NumericConvertBack(smooth_effect.distortionFactor);
        }

        private void FactorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (smooth_effect != null)
            {
                smooth_effect.distortionFactor = (float)SliderExponentialConverter.NumericConvert(e.NewValue);
            }
            updateModel();
            MyPlotView.Model.InvalidatePlot(true);
        }
    }

    public class SliderExponentialConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double x)
            {
                // Makes an exponential value from range 1/8 - 8, with 1 in the middle
                double v = ((x - 5.0) / 5.0);
                double finalVal = Double.Pow(8.0, v);

                return finalVal.ToString("N2");
            }
            else
            {
                return "0";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                try
                {
                    double d = double.Parse(s);
                    double originalPower = double.Log(d) / double.Log(8.0);      // Because of log properties this makes for log base 8
                    return (originalPower * 5.0) + 5.0;
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

        // Conversions between the slider and backend are housed here too
        public static double NumericConvert(double value)
        {
            double v = ((value - 5.0) / 5.0);
            return Double.Pow(8.0, v);
        }
        public static double NumericConvertBack(double value)
        {
            double originalPower = double.Log(value) / double.Log(8.0);      // Because of log properties this makes for log base 8
            return (originalPower * 5.0) + 5.0;
        }
    }
}
