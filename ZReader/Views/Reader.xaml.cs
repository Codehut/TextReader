using System;
using System.Collections.ObjectModel;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ZReader.Views
{
    /// <summary>
    /// Interaction logic for Reader.xaml
    /// </summary>
    public partial class Reader : Window
    {
        #region Variables
        private SpeechSynthesizer synthesizer;
        #endregion

        public Reader()
        {
            InitializeComponent();

            synthesizer = new SpeechSynthesizer();
            this.Loaded += ReaderLoaded;

            SetStartupLocation();   
        }

        private void SetStartupLocation()
        {
            MainFrame.WindowStartupLocation = WindowStartupLocation.Manual;
            MainFrame.Left = SystemParameters.PrimaryScreenWidth - 550;
            MainFrame.Top = SystemParameters.PrimaryScreenHeight - 260;
        }

        private void ReaderLoaded(object sender, RoutedEventArgs e)
        {
            ReadOnlyCollection<InstalledVoice> voices = synthesizer.GetInstalledVoices();
            foreach (var voice in voices)
            {
                ListedVoices.Items.Add(voice.VoiceInfo.Name);
            }
            InitializeSynthesizerState();
        }
        private void Player_MouseEnter(object sender, MouseEventArgs e)
        {
            Player.StretchDirection = StretchDirection.UpOnly;
        }

        private void InitializeSynthesizerState()
        {
            synthesizer.SpeakStarted += synthesizer_SpeakStarted;
            synthesizer.SpeakProgress += synthesizer_SpeakProgress;
            synthesizer.SpeakCompleted += synthesizer_SpeakCompleted;
            synthesizer.StateChanged += synthesizer_StateChanged;
        }

        private void synthesizer_StateChanged(object sender, StateChangedEventArgs e)
        {
            switch (synthesizer.State)
            {
                case SynthesizerState.Speaking:
                    Notifier.Fill = Brushes.Green;
                    Status.Text = "Reading...";
                    break;
                case SynthesizerState.Paused:
                    Notifier.Fill = Brushes.GreenYellow;
                    Status.Text = "Paused";
                    break;
                default:
                    Notifier.Fill = Brushes.Red;
                    Status.Text = "Ready";
                    break;
            }
        }

        private void synthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            Notifier.Fill = Brushes.Red;
            Status.Text = "Ready";
            Player.Source = new BitmapImage(new Uri(@"/ZReader;component/Assets/Play.png", UriKind.Relative));
        }

        private async void synthesizer_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            Notifier.Fill = Brushes.Green;
            Status.Text = "Reading...";
            CurrentText.Text += e.Text + " ";
            if (CurrentText.Text.Length >= 64)
            {
                await Task.Delay(250);
                CurrentText.Text = string.Empty;
            }
        }

        private void synthesizer_SpeakStarted(object sender, SpeakStartedEventArgs e)
        {
            CurrentText.Text = string.Empty;
            Notifier.Fill = Brushes.Green;
            Status.Text = "Reading...";
        }

        private void Player_MouseLeave(object sender, MouseEventArgs e)
        {
            Player.StretchDirection = StretchDirection.Both;
        }

        private void Player_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (synthesizer.State == SynthesizerState.Speaking)
            {
                synthesizer.Pause();
                Player.Source = new BitmapImage(new Uri(@"/ZReader;component/Assets/Pause.png", UriKind.Relative));
            }
            else if (synthesizer.State == SynthesizerState.Paused)
            {
                synthesizer.Resume();
                Player.Source = new BitmapImage(new Uri(@"/ZReader;component/Assets/Play.png", UriKind.Relative));
            }
            else if (synthesizer.State == SynthesizerState.Ready)
            {
                //May start the synthesizer from starting
                var feedValue = string.Empty;
                feedValue = new TextRange(Feed.Document.ContentStart, Feed.Document.ContentEnd).Text;
                if (feedValue != string.Empty)
                {
                    synthesizer.SpeakAsync(feedValue);
                }
                else
                {
                    MessageBox.Show("Nothing to read.");
                }
                Player.Source = new BitmapImage(new Uri(@"/ZReader;component/Assets/Pause.png", UriKind.Relative));
            }
        }

        private void ListedVoices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            synthesizer.SelectVoice(ListedVoices.SelectedItem.ToString());
        }
    }
}
