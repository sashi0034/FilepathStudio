using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace FilepathStudio
{
    public class ExplorerButtonGenerator : VisualLineElementGenerator
    {
        public override int GetFirstInterestedOffset(int startOffset)
        {
            var line = CurrentContext.VisualLine.LastDocumentLine;
            if (startOffset < line.EndOffset)
            {
                // We only want to place the button at the very end of the line
                return line.EndOffset;
            }
            return -1;
        }

        public override VisualLineElement ConstructElement(int offset)
        {
            var line = CurrentContext.Document.GetLineByOffset(offset);
            var lineText = CurrentContext.Document.GetText(line.Offset, line.Length).Trim();

            if (string.IsNullOrWhiteSpace(lineText) || lineText.StartsWith("#"))
                return null;

            // Basic check if it looks like a path (disk path or network path)
            bool isPath = false;
            try
            {
                if (Path.IsPathRooted(lineText) || lineText.StartsWith("\\\\"))
                {
                    isPath = true;
                }
            }
            catch
            {
                isPath = false;
            }

            if (!isPath)
                return null;

            var textBlock = new TextBlock
            {
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };

            var copyHyperlink = new Hyperlink(new Run("Copy"))
            {
                Foreground = new SolidColorBrush(Color.FromRgb(0, 120, 215)), // Windows Blue
                TextDecorations = null
            };
            copyHyperlink.MouseEnter += (s, e) => copyHyperlink.TextDecorations = TextDecorations.Underline;
            copyHyperlink.MouseLeave += (s, e) => copyHyperlink.TextDecorations = null;
            copyHyperlink.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(lineText);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not copy text: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            var explorerHyperlink = new Hyperlink(new Run("Open in Explorer"))
            {
                Foreground = new SolidColorBrush(Color.FromRgb(0, 120, 215)), // Windows Blue
                TextDecorations = null
            };

            explorerHyperlink.MouseEnter += (s, e) => explorerHyperlink.TextDecorations = TextDecorations.Underline;
            explorerHyperlink.MouseLeave += (s, e) => explorerHyperlink.TextDecorations = null;

            explorerHyperlink.Click += (s, e) =>
            {
                try
                {
                    if (File.Exists(lineText))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = $"/select,\"{lineText}\"",
                            UseShellExecute = true
                        });
                    }
                    else if (Directory.Exists(lineText))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = $"\"{lineText}\"",
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        MessageBox.Show($"Path does not exist:\n{lineText}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open path: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            textBlock.Inlines.Add(copyHyperlink);
            textBlock.Inlines.Add(new Run(" | ") { Foreground = Brushes.Gray });
            textBlock.Inlines.Add(explorerHyperlink);

            return new InlineObjectElement(0, textBlock);
        }
    }
}
