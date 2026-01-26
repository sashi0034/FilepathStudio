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
        private bool _isDirty = false;
        private bool _isLoading = false;
        private int _lastSearchIndex = -1;

        public MainWindow()
        {
            InitializeComponent();

            Editor.Background = new SolidColorBrush(Color.FromRgb(32, 32, 32));
            Editor.Foreground = Brushes.Gainsboro;

            // Register the inline button generator
            Editor.TextArea.TextView.ElementGenerators.Add(new ExplorerButtonGenerator());

            // Register the syntax highlighter
            Editor.TextArea.TextView.LineTransformers.Add(new FilepathColorizer());

            // Register the horizontal line renderer
            Editor.TextArea.TextView.BackgroundRenderers.Add(new HorizontalLineRenderer());

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
                    _isLoading = true;
                    _currentFilePath = settings.LastOpenedFilePath;
                    Editor.Text = File.ReadAllText(_currentFilePath);
                    _isDirty = false;
                    _isLoading = false;
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
            _isLoading = true;
            Editor.Text = "# これはコメントです\n" +
                          "C:\\Windows\\System32\\drivers\\etc\\hosts\n" +
                          "C:\\Windows\n" +
                          "\n" +
                          "# これはコメントです (2)\n" +
                          "C:\\Users\n";
            _isDirty = false;
            _isLoading = false;
            UpdateFilePathDisplay();
        }

        private void ModifyKeyBindings()
        {
            // Redo: Ctrl+Shift+Z
            Editor.TextArea.InputBindings.Add(
                new KeyBinding(
                    ApplicationCommands.Redo,
                    Key.Z,
                    ModifierKeys.Control | ModifierKeys.Shift
                )
            );

            // Disable default Ctrl+Y Redo
            Editor.TextArea.InputBindings.Add(
                new KeyBinding(
                    ApplicationCommands.NotACommand,
                    Key.Y,
                    ModifierKeys.Control
                )
            );

            // Add Save / Save As / Open bindings to the editor to ensure they work when editor is focused
            Editor.TextArea.InputBindings.Add(new KeyBinding(ApplicationCommands.Save, Key.S, ModifierKeys.Control));
            Editor.TextArea.InputBindings.Add(new KeyBinding(ApplicationCommands.SaveAs, Key.S,
                ModifierKeys.Control | ModifierKeys.Shift));
            Editor.TextArea.InputBindings.Add(new KeyBinding(ApplicationCommands.Open, Key.O, ModifierKeys.Control));

            // Search/Replace bindings using standard ApplicationCommands
            Editor.TextArea.InputBindings.Add(new KeyBinding(ApplicationCommands.Find, Key.F, ModifierKeys.Control));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Find, (s, e) => ToggleSearch(false)));

            Editor.TextArea.InputBindings.Add(new KeyBinding(ApplicationCommands.Replace, Key.H, ModifierKeys.Control));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Replace, (s, e) => ToggleSearch(true)));

            // Close Search binding (Escape)
            var closeSearchCmd = new RoutedCommand("CloseSearch", typeof(MainWindow),
                new InputGestureCollection { new KeyGesture(Key.Escape) });
            Editor.TextArea.InputBindings.Add(new KeyBinding(closeSearchCmd, Key.Escape, ModifierKeys.None));
            this.CommandBindings.Add(new CommandBinding(closeSearchCmd,
                (s, e) => CloseSearch_Click(s, (RoutedEventArgs)e)));

            // Ensure Escape works even when focus is in the search text boxes
            SearchTextBox.InputBindings.Add(new KeyBinding(closeSearchCmd, Key.Escape, ModifierKeys.None));
            ReplaceTextBox.InputBindings.Add(new KeyBinding(closeSearchCmd, Key.Escape, ModifierKeys.None));
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
                _isLoading = true;
                _currentFilePath = openFileDialog.FileName;
                Editor.Text = File.ReadAllText(_currentFilePath);
                _isDirty = false;
                _isLoading = false;
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
                _isDirty = false;
                UpdateFilePathDisplay();
            }
        }

        private void SaveFileAs_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                _currentFilePath = saveFileDialog.FileName;
                File.WriteAllText(_currentFilePath, Editor.Text);
                _isDirty = false;
                UpdateFilePathDisplay();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_isDirty && !string.IsNullOrEmpty(_currentFilePath))
            {
                try
                {
                    File.WriteAllText(_currentFilePath, Editor.Text);
                }
                catch (Exception ex)
                {
                    var result = MessageBox.Show($"Failed to auto-save: {ex.Message}\n\nDo you want to exit anyway?",
                        "Auto-save Error", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

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

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                _isDirty = true;
                UpdateFilePathDisplay();
            }
        }

        private void UpdateFilePathDisplay()
        {
            string dirtyIndicator = _isDirty ? "*" : "";
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                FilePathTextBlock.Text = "No file opened" + dirtyIndicator;
            }
            else
            {
                string fileName = Path.GetFileName(_currentFilePath);
                FilePathTextBlock.Text = $"{fileName}{dirtyIndicator} ({_currentFilePath})";
            }

            this.Title = "FilepathStudio" + dirtyIndicator;
        }

        private void CopyPath_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                Clipboard.SetText(_currentFilePath);
            }
        }

        #region Search and Replace

        private void FindMenu_Click(object sender, RoutedEventArgs e) => ToggleSearch(false);
        private void ReplaceMenu_Click(object sender, RoutedEventArgs e) => ToggleSearch(true);

        private void ToggleSearch(bool showReplace)
        {
            SearchPanel.Visibility = Visibility.Visible;
            ReplaceRow.Visibility = showReplace ? Visibility.Visible : Visibility.Collapsed;
            SearchTextBox.Focus();
            SearchTextBox.SelectAll();
        }

        private void CloseSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchPanel.Visibility = Visibility.Collapsed;
            Editor.Focus();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool next = !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
                PerformFind(next);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CloseSearch_Click(sender, e);
                e.Handled = true;
            }
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _lastSearchIndex = -1; // Reset search position when text changes
        }

        private void ReplaceTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformReplace();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CloseSearch_Click(sender, e);
                e.Handled = true;
            }
        }

        private void FindNext_Click(object sender, RoutedEventArgs e) => PerformFind(true);
        private void FindPrev_Click(object sender, RoutedEventArgs e) => PerformFind(false);

        private void PerformFind(bool next)
        {
            string searchText = SearchTextBox.Text;
            if (string.IsNullOrEmpty(searchText)) return;

            string editorText = Editor.Text;
            int startIndex = next ? Editor.CaretOffset : Editor.SelectionStart;

            if (!next && startIndex > 0) startIndex--; // Move back one to find previous

            StringComparison comparison = StringComparison.OrdinalIgnoreCase;
            int index;

            if (next)
            {
                index = editorText.IndexOf(searchText, startIndex, comparison);
                if (index == -1 && startIndex > 0) // Wrap around
                {
                    index = editorText.IndexOf(searchText, 0, comparison);
                }
            }
            else
            {
                index = editorText.LastIndexOf(searchText, startIndex, comparison);
                if (index == -1 && startIndex < editorText.Length - 1) // Wrap around
                {
                    index = editorText.LastIndexOf(searchText, editorText.Length - 1, comparison);
                }
            }

            if (index != -1)
            {
                Editor.Select(index, searchText.Length);
                Editor.ScrollToLine(Editor.Document.GetLineByOffset(index).LineNumber);
                _lastSearchIndex = index;
                SearchTextBox.BorderBrush = null; // Clear error if any
            }
            else
            {
                SearchTextBox.BorderBrush = Brushes.Red;
            }
        }

        private void Replace_Click(object sender, RoutedEventArgs e) => PerformReplace();

        private void PerformReplace()
        {
            string searchText = SearchTextBox.Text;
            if (string.IsNullOrEmpty(searchText)) return;

            if (Editor.SelectedText.Equals(searchText, StringComparison.OrdinalIgnoreCase))
            {
                int start = Editor.SelectionStart;
                Editor.Document.Replace(start, Editor.SelectionLength, ReplaceTextBox.Text);
                PerformFind(true); // Find next match
            }
            else
            {
                PerformFind(true); // Just find first
            }
        }

        private void ReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text;
            if (string.IsNullOrEmpty(searchText)) return;

            string replaceText = ReplaceTextBox.Text;
            string editorText = Editor.Text;

            // Note: Simple replace all. For large files this might be slow if done via Document.Replace repeatedly.
            // But for this app it's likely fine.
            int count = 0;
            Editor.BeginChange();
            try
            {
                int index = editorText.IndexOf(searchText, 0, StringComparison.OrdinalIgnoreCase);
                while (index != -1)
                {
                    Editor.Document.Replace(index, searchText.Length, replaceText);
                    editorText = Editor.Text; // Refresh text for next index
                    index = editorText.IndexOf(searchText, index + replaceText.Length,
                        StringComparison.OrdinalIgnoreCase);
                    count++;
                }
            }
            finally
            {
                Editor.EndChange();
            }

            if (count > 0)
            {
                MessageBox.Show($"Replaced {count} occurrences.", "Replace All", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        #endregion
    }
}