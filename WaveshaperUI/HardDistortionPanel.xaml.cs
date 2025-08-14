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
using static Effects;

namespace WaveshaperUI
{
    /// <summary>
    /// Interaction logic for HardDistortionPanel.xaml
    /// Better documentation for certain things in VolumePanel.xaml.cs
    /// </summary>
    public partial class HardDistortionPanel : EffectPanel
    {
        private Effects.Hard_Distortion_Effect? distortion_effect;
        private bool symmetrical = true;

        public HardDistortionPanel() : base(Effects.EffectType.HardDistortion)
        {
            distortion_effect = (Effects.Hard_Distortion_Effect?)EffectOperations.unpackEffect(effect);

            base.DisplayModel.Title = "Hard Limiting Distortion";
            InitializeComponent();

            // Match UI to effect defaults:
            if (distortion_effect != null) LimitSlider.Value = distortion_effect.upperLimit * 10.0f;
            if (distortion_effect != null) LowerLimitSlider.Value = distortion_effect.lowerLimit * -10.0f;
            if (distortion_effect != null) GainBox.IsChecked = distortion_effect.makeupGain;
        }

        private void SymetryButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle logic for mathmatical symmetry
            if (symmetrical)
            {
                symmetrical = false;
                SymmetryButton.Content = "Symmetric / Odd";
                LowerControls.Visibility = Visibility.Visible;
                LimitLabel.Content = "Upper Limit";

                // This makes any past value stored in the slider active immidiately
                if (distortion_effect != null) distortion_effect.lowerLimit = (float)(LowerLimitSlider.Value / -10.0);
                updateModel();
                MyPlotView.Model.InvalidatePlot(true);
            }
            else
            {
                symmetrical = true;
                SymmetryButton.Content = "Asymmetric / Even";
                LowerControls.Visibility = Visibility.Hidden;
                LimitLabel.Content = "Limit";

                // This restores the lower limit to match the singular slider
                if (distortion_effect != null) distortion_effect.lowerLimit = (float)(LimitSlider.Value / -10.0);
                updateModel();
                MyPlotView.Model.InvalidatePlot(true);
            }
        }

        private void LimitSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (distortion_effect != null)
            {
                distortion_effect.upperLimit = (float)(e.NewValue / 10.0);
                // We set the lower to match only if the user is expecting the math to be symmetrical by now
                if (symmetrical) distortion_effect.lowerLimit = (float)(e.NewValue / -10.0);

                updateModel();
                MyPlotView.Model.InvalidatePlot(true);
            }
        }

        private void LowerLimitSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (distortion_effect != null)
            {
                distortion_effect.lowerLimit = (float)(e.NewValue / -10.0);

                updateModel();
                MyPlotView.Model.InvalidatePlot(true);
            }
        }

        private void GainBox_Checked(object sender, RoutedEventArgs e)
        {
            if (distortion_effect != null)
            {
                distortion_effect.makeupGain = true;

                updateModel();
                MyPlotView.Model.InvalidatePlot(true);
            }
        }

        private void GainBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (distortion_effect != null)
            {
                distortion_effect.makeupGain = false;

                updateModel();
                MyPlotView.Model.InvalidatePlot(true);
            }
        }
    }
}
