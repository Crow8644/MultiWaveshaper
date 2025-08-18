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
    /// Interaction logic for Selector.xaml
    /// </summary>
    public partial class Selector : UserControl
    {
        public Selector()
        {
            InitializeComponent();
            //Tabs.Items.Add(new TabItem { Header = "Added" });
        }

        public void AddTabItem()
        {
            TabItem item = new TabItem();
            item.Content = new VolumePanel();
            Tabs.Items.Insert(Tabs.Items.Count - 1, item);
            Tabs.SelectedIndex = Tabs.Items.Count - 2;
        }

        private void Volume_Add_Click(object sender, RoutedEventArgs e)
        {
            TabItem item = new TabItem();
            item.Content = new VolumePanel();
            Tabs.Items.Insert(Tabs.Items.Count - 1, item);
            Tabs.SelectedIndex = Tabs.Items.Count - 2;
        }

        private void Smooth_Add_Click(object sender, RoutedEventArgs e)
        {
            TabItem item = new TabItem();
            item.Content = new TanhPanel();
            Tabs.Items.Insert(Tabs.Items.Count - 1, item);
            Tabs.SelectedIndex = Tabs.Items.Count - 2;
        }
    }
}
