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

            // Register the inline button generator
            Editor.TextArea.TextView.ElementGenerators.Add(new ExplorerButtonGenerator());

            // Register the syntax highlighter
            Editor.TextArea.TextView.LineTransformers.Add(new FilepathColorizer());

            // Initial text
            Editor.Text = "# これはコメントです\n" +
                          "C:\\Windows\\System32\\drivers\\etc\\hosts\n" +
                          "C:\\Windows\n" +
                          "\n" +
                          "# これはコメントです (2)\n" +
                          "C:\\Users\n";
        }
    }
}