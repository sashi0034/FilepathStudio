using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace FilepathStudio
{
    public class HorizontalLineRenderer : IBackgroundRenderer
    {
        public KnownLayer Layer => KnownLayer.Background;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (textView == null || !textView.VisualLinesValid) return;

            var visualLines = textView.VisualLines;
            if (visualLines.Count == 0) return;

            // Use a color that matches the dark theme
            Pen pen = new Pen(new SolidColorBrush(Color.FromRgb(70, 70, 70)), 1);
            pen.Freeze();

            foreach (var visualLine in visualLines)
            {
                var line = visualLine.FirstDocumentLine;
                string text = textView.Document.GetText(line);

                if (text.TrimStart().StartsWith("---"))
                {
                    // Get the end position of the text in this visual line
                    // We can use the last element's right edge or the whole line's width
                    
                    // BackgroundGeometryBuilder.GetRectsForSegment is a reliable way to get text bounds
                    var rects = BackgroundGeometryBuilder.GetRectsForSegment(textView, line).ToList();
                    if (rects.Count > 0)
                    {
                        // The last rect in the list represents the end of the line
                        var lastRect = rects.Last();
                        
                        double y = lastRect.Top + (lastRect.Height / 2);
                        double startX = lastRect.Right + 10; // 10px gap after text
                        double endX = textView.ActualWidth;

                        if (startX < endX)
                        {
                            // Snapping to pixels for a sharp line
                            double snappedY = Math.Round(y) + 0.5;
                            drawingContext.DrawLine(pen, new Point(startX, snappedY), new Point(endX, snappedY));
                        }
                    }
                }
            }
        }
    }
}
