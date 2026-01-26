using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace FilepathStudio
{
    public class FilepathColorizer : DocumentColorizingTransformer
    {
        protected override void ColorizeLine(DocumentLine line)
        {
            string lineText = CurrentContext.Document.GetText(line);
            
            if (string.IsNullOrWhiteSpace(lineText))
                return;

            if (lineText.TrimStart().StartsWith("#"))
            {
                // Comment color: Grayish Green
                ChangeLinePart(line.Offset, line.EndOffset, element => {
                    element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(Color.FromRgb(87, 166, 74)));
                });
                return;
            }

            // Path colorization: Gainsboro (default) but let's make it slightly different if needed
            // Actually, let's color the whole line first
            ChangeLinePart(line.Offset, line.EndOffset, element => {
                element.TextRunProperties.SetForegroundBrush(Brushes.Gainsboro);
            });

            // Colorize slashes and backslashes specifically
            for (int i = 0; i < lineText.Length; i++)
            {
                char c = lineText[i];
                if (c == '/' || c == '\\')
                {
                    ChangeLinePart(line.Offset + i, line.Offset + i + 1, element => {
                        element.TextRunProperties.SetForegroundBrush(Brushes.Coral);
                        // Make it slightly bolder to stand out
                        element.TextRunProperties.SetTypeface(new Typeface(
                            element.TextRunProperties.Typeface.FontFamily,
                            element.TextRunProperties.Typeface.Style,
                            FontWeights.SemiBold,
                            element.TextRunProperties.Typeface.Stretch));
                    });
                }
            }
        }
    }
}
