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
            EffectOperations.createEffect(Effects.EffectType.SmoothDistortion);
            //Playback.play();
            Recording.saveCurrentFile();
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Microsoft.FSharp.Core.FSharpFunc<float, Microsoft.FSharp.Core.Unit> fSharpFunc = EffectOperations.getChangeBinding(0);
            //fSharpFunc.Invoke((float)e.NewValue / 10.0f);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Playback.closeObjects();
        }

        private void CommandAdd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: Connect to selector panel
        }

        private void CommandAdd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

        }
    }
}