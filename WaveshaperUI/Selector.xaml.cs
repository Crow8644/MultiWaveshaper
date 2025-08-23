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
            // Put the panel together with the closing button
            Grid grid = new Grid();
            grid.Children.Add(panel);
            Button closeButton = new Button
            {
                Height = 50,
                Width = 50,
                Content = "X",
                FontSize = 18,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                Margin = new Thickness(5.0, 5.0, 5.0, 5.0)
            };
            closeButton.Click += CloseButton_Click;
            grid.Children.Add(closeButton);

            TabItem item = new TabItem();
            item.Content = grid;
            Tabs.Items.Insert(Tabs.Items.Count - 1, item);
            Tabs.SelectedIndex = Tabs.Items.Count - 2;

            // Build a model where only the line is visible
            PlotModel iconModel = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(0.0) // Removes borders from sight
            };

            Axis lower = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = -1,
                Maximum = 1,
                IsAxisVisible = false       // Remove the axis from sight
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

        // This same method will be called reguardless of which X button is clicked
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Tabs.SelectedContent is Grid grid &&
                grid.Children[0] is EffectPanel panel)
            {
                panel.removeEffect();                       // Removes the effect from the backend
                Tabs.Items.Remove(Tabs.SelectedItem);    // Removes the panel from the scene
            }
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
            if (e.RemovedItems.Count > 0)   // This method does get called with no removed items when rendering, so this is necessary
            {
                // Pattern matching to get each relevant object
                if (e.RemovedItems[0] is TabItem item)
                {
                    if (item.Header is PlotView icon && item.Content is Grid grid && grid.Children[0] is EffectPanel panel)
                    {
                        icon.Model.Series.Clear();
                        icon.Model.Series.Add(panel.MakeDisplaySeries());
                        icon.Model.InvalidatePlot(true);
                    }
                }
            }
        }

        // The following two methods, which allow re-ording of effects, were copied from this stackoverflow post:
        // https://stackoverflow.com/questions/10738161/is-it-possible-to-rearrange-tab-items-in-tab-control-in-wpf
        // And modified slightly to work with this project
        // The post references a MSDN post, but that link is dead

        // This function initiates the drag and drop process
        private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!(e.Source is TabItem tabItem))
            {
                return;
            }

            if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
            }
        }

        // This function moves the tab items after the user drags them somewhere
        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            if (e.Source is TabItem tabItemTarget &&
                e.Data.GetData(typeof(TabItem)) is TabItem tabItemSource &&
                !tabItemSource.Equals(Adding_Tab) &&            // Prevents moving the adding tab
                !tabItemTarget.Equals(Adding_Tab) &&            // Prevents moving past the adding tab
                !tabItemTarget.Equals(tabItemSource) &&         // Prevents moves to the same place
                tabItemTarget.Parent is TabControl tabControl)
            {
                int targetIndex = tabControl.Items.IndexOf(tabItemTarget);
                int sourceIndex = tabControl.Items.IndexOf(tabItemSource);  // I've added this variable in order to interface with the backend

                if (EffectOperations.moveEffect(sourceIndex, targetIndex))      // Moves the effect as well as the tab items
                {
                    tabControl.Items.Remove(tabItemSource);
                    tabControl.Items.Insert(targetIndex, tabItemSource);
                }

                tabItemSource.IsSelected = true;
            }
        }
        
        private void RemoveCurrentTabItem()
        {

            Tabs.Items.Remove(Tabs.SelectedContent);
        }
    }
}
