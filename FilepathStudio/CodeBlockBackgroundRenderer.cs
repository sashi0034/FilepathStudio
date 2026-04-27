using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace FilepathStudio
{
    public class CodeBlockBackgroundRenderer : IBackgroundRenderer
    {
        public KnownLayer Layer => KnownLayer.Background;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (!textView.VisualLinesValid) return;

            var visualLines = textView.VisualLines;
            if (visualLines.Count == 0) return;

            // Background color for code block
            Brush backgroundBrush = new SolidColorBrush(Color.FromRgb(20, 20, 20)); // Darker background
            backgroundBrush.Freeze();
            // Pen borderPen = new Pen(new SolidColorBrush(Color.FromRgb(60, 60, 60)), 1);
            // borderPen.Freeze();

            foreach (var visualLine in visualLines)
            {
                var line = visualLine.FirstDocumentLine;
                var text = textView.Document.GetText(line).Trim();

                bool isCodeBlockBoundary = text.StartsWith("```");

                bool insideBlock = false;
                for (int i = 1; i < line.LineNumber; i++)
                {
                    var l = textView.Document.GetLineByNumber(i);
                    var lText = textView.Document.GetText(l).Trim();
                    if (lText.StartsWith("```"))
                    {
                        insideBlock = !insideBlock;
                    }
                }

                if (insideBlock || isCodeBlockBoundary)
                {
                    var rects = BackgroundGeometryBuilder.GetRectsForSegment(textView, line).ToList();
                    if (rects.Count > 0)
                    {
                        var firstRect = rects.First();

                        double width = textView.ActualWidth;
                        Rect rect = new Rect(0, firstRect.Top, width, firstRect.Height);

                        // Fill background
                        drawingContext.DrawRectangle(backgroundBrush, null, rect);

                        // Draw borders
                        // if (isCodeBlockBoundary && !insideBlock)
                        // {
                        //     // Top boundary
                        //     drawingContext.DrawLine(borderPen, new Point(0, rect.Top), new Point(width, rect.Top));
                        // }
                        //
                        // if (isCodeBlockBoundary && insideBlock)
                        // {
                        //     // Bottom boundary
                        //     drawingContext.DrawLine(borderPen, new Point(0, rect.Bottom),
                        //         new Point(width, rect.Bottom));
                        // }
                    }
                }
            }
        }
    }
}