using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace FilepathStudio
{
    public class PowerShellButtonGenerator : VisualLineElementGenerator
    {
        public override int GetFirstInterestedOffset(int startOffset)
        {
            var line = CurrentContext.VisualLine.LastDocumentLine;
            if (startOffset < line.EndOffset)
            {
                return line.EndOffset;
            }
            return -1;
        }

        public override VisualLineElement? ConstructElement(int offset)
        {
            var line = CurrentContext.Document.GetLineByOffset(offset);
            var lineText = CurrentContext.Document.GetText(line.Offset, line.Length).Trim();

            if (string.IsNullOrWhiteSpace(lineText) || lineText.StartsWith("```"))
                return null;

            // Check if we are inside a code block
            bool inCodeBlock = false;
            for (int i = 1; i < line.LineNumber; i++)
            {
                var l = CurrentContext.Document.GetLineByNumber(i);
                var text = CurrentContext.Document.GetText(l.Offset, l.Length).Trim();
                if (text.StartsWith("```"))
                {
                    inCodeBlock = !inCodeBlock;
                }
            }

            if (!inCodeBlock)
                return null;

            var textBlock = new TextBlock
            {
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };

            var psHyperlink = new Hyperlink(new Run("Run in PowerShell"))
            {
                Foreground = new SolidColorBrush(Color.FromRgb(0, 120, 215)), // Windows Blue
                TextDecorations = null
            };

            psHyperlink.MouseEnter += (s, e) => psHyperlink.TextDecorations = TextDecorations.Underline;
            psHyperlink.MouseLeave += (s, e) => psHyperlink.TextDecorations = null;

            psHyperlink.Click += (s, e) =>
            {
                try
                {
                    string encoded = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(lineText));
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoExit -EncodedCommand {encoded}",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not run command: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            textBlock.Inlines.Add(psHyperlink);

            return new InlineObjectElement(0, textBlock);
        }
    }
}
