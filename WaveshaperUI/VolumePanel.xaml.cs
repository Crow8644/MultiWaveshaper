using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;
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
    public partial class VolumePanel : EffectPanel
    {
        private Effects.Volume_Effect? volume_effect;
        public VolumePanel(): base(Effects.EffectType.Volume)
        {
            // Both this and the inherited union version (effect) are used for different things
            volume_effect = (Effects.Volume_Effect?)EffectOperations.unpackEffect(effect);

            base.DisplayModel.Title = "Volume Effect";
            InitializeComponent();
        }

        //public Binding SeriesBinding { get; private set; }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (volume_effect != null)
            {
                volume_effect.volume = (float)(e.NewValue / 10.0);
            }
            updateModel();
            MyPlotView.Model.InvalidatePlot(true);
        }
    }
}
