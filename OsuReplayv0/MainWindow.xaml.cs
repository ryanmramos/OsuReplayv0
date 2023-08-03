using System.Windows;
using System.Windows.Controls;

namespace OsuReplayv0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        MainWindowViewModel viewModel = new MainWindowViewModel();

        public MainWindow()
        {
            DataContext = viewModel;
            InitializeComponent();
            viewModel.SetCanvas(ref canvas, ref PlayGrid);
            SizeChanged += MainWindow_SizeChange;
        }

        private void MainWindow_SizeChange(object sender, SizeChangedEventArgs e)
        {
            viewModel.WindowSizeChange(PlayGrid);
        }
    }
}