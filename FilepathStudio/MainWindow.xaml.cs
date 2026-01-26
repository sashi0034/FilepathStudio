using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FilepathStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string? _currentFilePath;

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

            ModifyKeyBindings();
        }

        private void ModifyKeyBindings()
        {
            Editor.TextArea.InputBindings.Add(
                new KeyBinding(
                    ApplicationCommands.Redo,
                    Key.Z,
                    ModifierKeys.Control | ModifierKeys.Shift
                )
            );

            Editor.TextArea.InputBindings.Add(
                new KeyBinding(
                    ApplicationCommands.NotACommand,
                    Key.Y,
                    ModifierKeys.Control
                )
            );
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.ContextMenu != null)
            {
                element.ContextMenu.PlacementTarget = element;
                element.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                element.ContextMenu.IsOpen = true;
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                _currentFilePath = openFileDialog.FileName;
                Editor.Text = File.ReadAllText(_currentFilePath);
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                SaveFileAs_Click(sender, e);
            }
            else
            {
                File.WriteAllText(_currentFilePath, Editor.Text);
            }
        }

        private void SaveFileAs_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                _currentFilePath = saveFileDialog.FileName;
                File.WriteAllText(_currentFilePath, Editor.Text);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenTerminal_Click(object sender, RoutedEventArgs e)
        {
            string directory = string.Empty;
            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                directory = Path.GetDirectoryName(_currentFilePath) ?? string.Empty;
            }

            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                directory = AppDomain.CurrentDomain.BaseDirectory;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = directory,
                UseShellExecute = true
            });
        }
    }
}