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

namespace WaveshaperUI
{
    /// <summary>
    /// Interaction logic for OversamplingControler.xaml
    /// </summary>
    public partial class OversamplingControler : UserControl
    {
        public OversamplingControler()
        {
            InitializeComponent();
            Rate = 1;
        }

        private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // This if statement unpacks the selected value
            if (Selector.SelectedItem is ComboBoxItem b &&
                b.Content is string s)
            {
                Rate = int.Parse(s);
                Playback.oversamplingChanged(Rate);

                UIAction?.Invoke();         // Will invoke only if UIAction is not null

            }
        }

        public int Rate { get; private set; }

        // An action to be taken on other UIElements every time the selection is changed
        public Action? UIAction { get; set; }
    }
}
