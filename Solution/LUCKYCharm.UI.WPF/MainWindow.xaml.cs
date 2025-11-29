using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace LUCKYCharm.UI.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _numbersFrom = 1;
        private int _numbersTo = 1000;

        private StreamWriter? _numbersWriter;

        public Random Random { get; } = new Random();

        public HashSet<int> NumberHistory = [];

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(StartButton != null) {
                StartButton.IsEnabled = int.TryParse(NumRangeFromTextBox.Text, out int from)
                    && int.TryParse(NumRangeToTextBox.Text, out int to)
                    && from < to;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _numbersFrom = int.Parse(NumRangeFromTextBox.Text);
            _numbersTo = int.Parse(NumRangeToTextBox.Text);

            ConfigurationPanel.Visibility = Visibility.Collapsed;
            CounterViewBox.Visibility = Visibility.Visible;

            Directory.CreateDirectory("logs");
            _numbersWriter = new(new FileStream($"logs/{DateTime.Now:yyyy}{DateTime.Now:MM}{DateTime.Now:dd}-{DateTime.Now:HH}{DateTime.Now:mm}{DateTime.Now:ss} Zahlenprotokoll.log", FileMode.CreateNew));
            _numbersWriter.WriteLine($"Zahlenbereich: {_numbersFrom} bis {_numbersTo}");
            _numbersWriter.WriteLine($"Index:Nummer");
            _numbersWriter.WriteLine($"------------");
            _numbersWriter.Flush();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextButton.IsEnabled = false;
            CloseButton.IsEnabled = false;
            CloseButton.Visibility = Visibility.Hidden;

            Task.Run(StartZiehungAsync);
        }

        private async Task StartZiehungAsync()
        {
            for (int n = 10; n < 300; n += n/8) {
                Dispatcher.Invoke(() => NumberTextBlock.Text = Random.Next(_numbersFrom, _numbersTo + 1).ToString());
                await Task.Delay(n);
            }

            int nextNumber = 0;

            do {
                nextNumber = Random.Next(_numbersFrom, _numbersTo + 1);
            } while (NumberHistory.Contains(nextNumber));

            NumberHistory.Add(nextNumber);

            _numbersWriter!.WriteLine($"{NumberHistory.Count}:{nextNumber}");
            _numbersWriter.Flush();

            Dispatcher.Invoke(() => {
                NumberTextBlock.Text = nextNumber.ToString();
                NextButton.IsEnabled = NumberHistory.Count < (_numbersTo - _numbersFrom);
                CloseButton.IsEnabled = true;
                CloseButton.Visibility = Visibility.Visible;
            });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _numbersWriter!.Flush();
            _numbersWriter.Close();
            Application.Current.Shutdown();
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) {
                DragMove();
            }
        }
    }
}