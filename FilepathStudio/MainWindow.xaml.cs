using System.Windows;
using System.Windows.Media;

namespace FilepathStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Editor.Background = new SolidColorBrush(Color.FromRgb(32, 32, 32));
            Editor.Foreground = Brushes.Gainsboro;
        }
    }
}