using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for Selector.xaml
    /// </summary>
    public partial class Selector : UserControl
    {
        public Selector()
        {
            InitializeComponent();
            //Tabs.Items.Add(new TabItem { Header = "Added" });
        }
        private void makeNewTab(EffectPanel panel)
        {
            TabItem item = new TabItem();
            item.Content = panel;
            Tabs.Items.Insert(Tabs.Items.Count - 1, item);
            Tabs.SelectedIndex = Tabs.Items.Count - 2;

            // Build a model where only the line is visible
            PlotModel iconModel = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(0.0)
            };

            Axis lower = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = -1,
                Maximum = 1,
                IsAxisVisible = false
            };
            Axis side = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = -1.03,            // Wider range than [-1, 1] ensures the tops of lines don't get cut off
                Maximum = 1.03,
                IsAxisVisible = false
            };
            iconModel.Axes.Add(lower);
            iconModel.Axes.Add(side);

            iconModel.Series.Add(panel.MakeDisplaySeries());

            PlotView icon = new PlotView
            {
                Height = 60,
                Width = 60,
                Opacity = 0.75,
                IsEnabled = false       // Makes it so clicking the model is just clicking the tab, rather than giving extra information
            };
            icon.Model = iconModel;

            item.Header = icon;
        }
        private void Volume_Add_Click(object sender, RoutedEventArgs e)
        {
            makeNewTab(new VolumePanel());
        }

        private void Smooth_Add_Click(object sender, RoutedEventArgs e)
        {
            makeNewTab(new TanhPanel());
        }

        private void Hard_Add_Click(object sender, RoutedEventArgs e)
        {
            makeNewTab(new HardDistortionPanel());
        }

        // The function updates the UI of the tab headers when the user selects a new one
        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Plotted icon will only update after being unselected
            if (e.RemovedItems.Count > 0)
            {
                // Pattern matching to get icon
                if (e.RemovedItems[0] is TabItem item)
                {
                    if (item.Header is PlotView icon && item.Content is EffectPanel panel)
                    {
                        icon.Model.Series.Clear();
                        icon.Model.Series.Add(panel.MakeDisplaySeries());
                        icon.Model.InvalidatePlot(true);
                    }
                }
            }
        }
    }
}
