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

namespace WaveshaperUI
{
    /// <summary>
    /// Interaction logic for VolumePanel.xaml
    /// </summary>
    public partial class VolumePanel : UserControl
    {
        public VolumePanel()
        {
            InitializeComponent();
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
            if (value is string)
            {
                try
                {
                    string s = value.ToString();
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
