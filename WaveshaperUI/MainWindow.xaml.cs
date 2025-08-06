using System.Text;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Run tests on functions in Functionality here
            Playback.newFile(1);
            Thread.Sleep(400);
            EffectOperations.createEffect(Effects.EffectType.Volume);
            Playback.play();
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Microsoft.FSharp.Core.FSharpFunc<float, Microsoft.FSharp.Core.Unit> fSharpFunc = EffectOperations.getChangeBinding(0);
            fSharpFunc.Invoke((float)e.NewValue / 10.0f);
        }
    }
}