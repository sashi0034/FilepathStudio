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

            // Load settings
            var settings = SettingsManager.Load();
            if (settings.FontSize.HasValue)
            {
                Editor.FontSize = settings.FontSize.Value;
            }

            // Restore last opened file
            if (!string.IsNullOrEmpty(settings.LastOpenedFilePath) && File.Exists(settings.LastOpenedFilePath))
            {
                try
                {
                    _currentFilePath = settings.LastOpenedFilePath;
                    Editor.Text = File.ReadAllText(_currentFilePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to restore file: {ex.Message}");
                    LoadDefaultText();
                }
            }
            else
            {
                LoadDefaultText();
            }

            ModifyKeyBindings();
            UpdateFilePathDisplay();
        }

        private void LoadDefaultText()
        {
            // Initial text
            Editor.Text = "# これはコメントです\n" +
                          "C:\\Windows\\System32\\drivers\\etc\\hosts\n" +
                          "C:\\Windows\n" +
                          "\n" +
                          "# これはコメントです (2)\n" +
                          "C:\\Users\n";
            UpdateFilePathDisplay();
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
                UpdateFilePathDisplay();
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
                UpdateFilePathDisplay();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var settings = new AppSettings
            {
                LastOpenedFilePath = _currentFilePath,
                FontSize = Editor.FontSize
            };
            SettingsManager.Save(settings);
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

        private void Editor_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                double fontSize = Editor.FontSize;
                if (e.Delta > 0)
                {
                    fontSize += 1;
                }
                else
                {
                    fontSize -= 1;
                }

                if (fontSize < 6) fontSize = 6;
                if (fontSize > 100) fontSize = 100;

                Editor.FontSize = fontSize;
                e.Handled = true;
            }
        }

        private void UpdateFilePathDisplay()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                FilePathTextBlock.Text = "No file opened";
            }
            else
            {
                string fileName = Path.GetFileName(_currentFilePath);
                FilePathTextBlock.Text = $"{fileName} ({_currentFilePath})";
            }
        }

        private void CopyPath_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                Clipboard.SetText(_currentFilePath);
            }
        }
    }
}