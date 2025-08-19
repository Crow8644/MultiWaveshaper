using Microsoft.FSharp.Core;
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
    /// Interaction logic for FileBar.xaml
    /// </summary>
    public partial class FileBar : UserControl
    {
        private bool paused = true;
        public FileBar()
        {
            InitializeComponent();
            CompositionTarget.Rendering += Update_Slider;
        }

        // Has the slider and time counter track the progress of a track
        private void Update_Slider(object? sender, EventArgs e)
        {
            if (!paused && !Progress.IsMouseCaptureWithin)
            {
                Progress.Value = Streams.getFileProgress(10);
                if (Streams.isFileOver())   // Resets UI properties when the end of a file is reached
                {
                    Play_Pause.Content = "play";
                    paused = true;
                    Progress.Value = 0;
                    Start_Time.Content = "0:00";
                }
                Start_Time.Content = Streams.getTimeDisplay();
            }
        }

        public void Try_Set_Data(string defaultStr)
        {
            File_Label.Content = Streams.getFileName();
            End_Time.Content = Streams.getEndTimeDisplay();
            Start_Time.Content = defaultStr;
        }

        private void Play_Pause_Click(object sender, RoutedEventArgs e)
        {
            // Toggle logic for the button that plays and pauses audio
            if (paused)
            {
                if (Playback.play())
                {
                    Play_Pause.Content = "pause";
                    paused = false;
                }
            }
            else
            {
                if (Playback.pause())
                {
                    Play_Pause.Content = "play";
                    paused = true;
                }
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (Playback.stop())
            {
                Play_Pause.Content = "play";
                paused = true;
                Progress.Value = 0; // Resets the slider
                Start_Time.Content = "0:00";
            }
        }

        private void Progress_ValueChanged(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            Streams.getRepositionFunction().Invoke(Progress.Value / 10.0);
        }

    }
}
