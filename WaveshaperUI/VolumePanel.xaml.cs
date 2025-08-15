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
        // This variable is used for direct control of effect parameters,
        // Whereas the union type (effects) is used for calls to the F# code that happen for all panel types
        private readonly Effects.Volume_Effect? volume_effect;
        public VolumePanel(): base(Effects.EffectType.Volume)
        {
            // This is an unchecked cast, however, this function just initialized effects, so we know what type it is
            volume_effect = (Effects.Volume_Effect?)EffectOperations.unpackEffect(effect);

            base.DisplayModel.Title = "Volume";
            InitializeComponent();
            if (volume_effect != null) VolumeSlider.Value = volume_effect.volume * 10.0f;
        }


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
