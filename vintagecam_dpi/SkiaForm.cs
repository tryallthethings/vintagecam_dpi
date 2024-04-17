using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Diagnostics;
using System.Drawing.Printing;

namespace vintagecam_dpi
{
    public partial class SkiaForm : Form
    {
        private SKControl skControl;
        private List<DrawableObject> objects = new List<DrawableObject>();
        private float dpiScale; // DPI scale will be set based on actual display DPI
        private DrawableObject selectedObject;
        private SKPoint lastLocation;
        private SKMatrix matrix = SKMatrix.CreateIdentity();
        private bool resizing = false;
        private ResizeDirection currentResizeDirection = ResizeDirection.None;

        public SkiaForm()
        {
            InitializeComponent();
            SetDpiScale();
            AutoScaleMode = AutoScaleMode.Dpi;
            // Calculate ClientSize to represent an A4 size adjusted by the DPI scale
            ClientSize = new Size((int)(2481 / dpiScale), (int)(3507 / dpiScale));
            Text = "SkiaSharp A4 Drawing Example";

            skControl = new SKControl();
            skControl.Dock = DockStyle.Fill;

            skControl.PaintSurface += OnPaintSurface;
            skControl.MouseDown += SkControl_MouseDown;
            skControl.MouseMove += SkControl_MouseMove;
            skControl.MouseUp += SkControl_MouseUp;
            skControl.MouseWheel += SkControl_MouseWheel;

            // Create a header
            var headerObject = new DrawableObject(100, 100, 200, 100, "Dresden Post", DrawableObjectType.Header);
            objects.Add(headerObject);

            // Create a text
            var textObject = new DrawableObject(100, 200, 300, 100, "Willkommen in Dresden, der Stadt mit einem Hauch von Humor! Diese wunderbare Perle an der Elbe hat nicht nur eine reiche Geschichte und beeindruckende Architektur zu bieten, sondern auch eine Fülle von amüsanten und lustigen Aspekten, die Besucher zum Lachen bringen. Beginnen wir mit der berühmten Frauenkirche, einem Meisterwerk der Architektur. Das beeindruckende Gebäude, das nach der Zerstörung im Zweiten Weltkrieg wiederaufgebaut wurde, zieht Besucher aus aller Welt an. Doch wussten Sie, dass die Kuppel der Kirche ein bisschen schief ist? Es heißt, die Architekten hätten absichtlich einen kleinen Scherz eingebaut, um zu zeigen, dass Perfektion nicht immer erstrebenswert ist. Ein schiefes Wahrzeichen - das ist doch zum Schmunzeln! Ein weiterer lustiger Aspekt in Dresden ist die \"Brühlsche Terrasse\", auch bekannt als \"Balkon Europas\". Hier kann man einen herrlichen Blick auf die Elbe und die Altstadt genießen.\"", DrawableObjectType.Text);
            objects.Add(textObject);

            // Create an image object
            var imageObject = new DrawableObject(100, 500, 300, 200, "", DrawableObjectType.Image);
            imageObject.SetImage(SKBitmap.Decode("1.jpeg"));
            objects.Add(imageObject);

            panel.Controls.Add(skControl); // Ensure the control is added back to the form
        }

        private void SetDpiScale()
        {
            using (Graphics g = this.CreateGraphics())
            {
                float dpiX = g.DpiX;
                dpiScale = 96f / dpiX; // Calculate scale as the inverse of the system's DPI scaling
            }
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            canvas.Clear(new SKColor(200, 200, 200));  // Clear the canvas with a light gray background

            // Apply the transformation matrix for everything to be transformed
            canvas.SetMatrix(matrix);

            // Draw a white rectangle for the A4 paper area
            SKRect paperRect = new SKRect(0, 0, e.Info.Width, e.Info.Height);
            canvas.DrawRect(paperRect, new SKPaint { Color = SKColors.White });

            // Draw objects on the canvas
            foreach (var obj in objects)
            {
                obj.Draw(canvas);
            }

            // Draw a border around the A4 paper area
            using (var paint = new SKPaint())
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = SKColors.Black;
                paint.StrokeWidth = 0;  // Border thickness
                canvas.DrawRect(paperRect, paint);
            }
        }

        private void SkControl_MouseDown(object sender, MouseEventArgs e)
        {
            SKPoint transformedPoint = TransformMousePoint(e.Location);
            lastLocation = transformedPoint;
            if (e.Button == MouseButtons.Middle)
            {
                Cursor = Cursors.Hand;
            }
            else if (e.Button == MouseButtons.Left)
            {
                selectedObject = null;
                foreach (var obj in objects)
                {
                    if (obj.HitTest(transformedPoint))
                    {
                        selectedObject = obj;
                        selectedObject.IsSelected = true;
                        currentResizeDirection = obj.GetResizeDirection(transformedPoint);
                        resizing = currentResizeDirection != ResizeDirection.None;
                        break;
                    }
                }
            }
            skControl.Invalidate();
        }

        private void SkControl_MouseMove(object sender, MouseEventArgs e)
        {
            SKPoint transformedPoint = TransformMousePoint(e.Location);

            // Reset hover state for all objects first
            foreach (var obj in objects)
            {
                obj.IsHovered = false;
            }

            if (e.Button == MouseButtons.Middle)
            {
                var dx = transformedPoint.X - lastLocation.X;
                var dy = transformedPoint.Y - lastLocation.Y;
                matrix = matrix.PreConcat(SKMatrix.CreateTranslation(dx, dy));
                skControl.Invalidate();
            }
            else if (e.Button == MouseButtons.Left && selectedObject != null)
            {
                if (resizing)
                {
                    selectedObject.Resize(transformedPoint, currentResizeDirection);
                }
                else
                {
                    var deltaX = transformedPoint.X - lastLocation.X;
                    var deltaY = transformedPoint.Y - lastLocation.Y;
                    selectedObject.Bounds = new SKRect(
                        selectedObject.Bounds.Left + deltaX,
                        selectedObject.Bounds.Top + deltaY,
                        selectedObject.Bounds.Right + deltaX,
                        selectedObject.Bounds.Bottom + deltaY
                    );
                }
                skControl.Invalidate();
            }
            else
            {
                // Check for hover state without any mouse button pressed
                foreach (var obj in objects)
                {
                    if (obj.HitTest(transformedPoint))
                    {
                        obj.IsHovered = true;
                    }
                }
                skControl.Invalidate();
            }

            lastLocation = transformedPoint;
        }

        private void SkControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (selectedObject != null)
            {
                selectedObject.IsSelected = false;
                selectedObject = null;
            }
            if (e.Button == MouseButtons.Middle)
            {
                Cursor = Cursors.Default;
            }
            resizing = false;
            skControl.Invalidate();
        }

        private void SkControl_MouseWheel(object sender, MouseEventArgs e)
        {
            float scale = e.Delta > 0 ? 1.1f : 0.9f;
            var mouseLocation = new SKPoint(e.X, e.Y);
            var translateMatrix = SKMatrix.CreateTranslation(-mouseLocation.X, -mouseLocation.Y);
            var scaleMatrix = SKMatrix.CreateScale(scale, scale);
            var undoTranslationMatrix = SKMatrix.CreateTranslation(mouseLocation.X, mouseLocation.Y);

            matrix = matrix.PreConcat(translateMatrix);
            matrix = matrix.PreConcat(scaleMatrix);
            matrix = matrix.PreConcat(undoTranslationMatrix);

            skControl.Invalidate();
        }

        // Helper method to transform mouse points according to the current matrix
        private SKPoint TransformMousePoint(Point location)
        {
            // Convert Point to SKPoint
            SKPoint point = new SKPoint(location.X, location.Y);
            // Get the inverse matrix
            if (matrix.TryInvert(out SKMatrix inverseMatrix))
            {
                return inverseMatrix.MapPoint(point);
            }
            return point;
        }

        private void PrintCanvas()
        {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += (sender, e) =>
            {
                SKImageInfo imageInfo = new SKImageInfo(2481, 3507); // Dimensions at 300 DPI
                using (var surface = SKSurface.Create(imageInfo))
                {
                    DrawToCanvas(surface.Canvas);
                    using (var image = surface.Snapshot())
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                    using (var stream = data.AsStream())
                    {
                        var bitmap = new Bitmap(stream);
                        e.Graphics.DrawImage(bitmap, new Point(0, 0));
                    }
                }
            };
            printDocument.Print();
        }

        private void DrawToCanvas(SKCanvas canvas)
        {
            canvas.Clear(SKColors.White);
            foreach (var obj in objects)
            {
                obj.Draw(canvas);
            }
        }

        private void ZoomToFitCanvas()
        {
            // Assuming 'matrix' is '_currentMatrix' in your application
            SKMatrix _currentMatrix = SKMatrix.CreateIdentity();

            // Define the viewport as the panel or SKControl bounds
            SKRect viewport = new SKRect(0, 0, skControl.Width, skControl.Height);

            // Assuming 'CanvasView' is equivalent to 'skControl' in your application
            var viewPortRectangle = _currentMatrix.MapRect(viewport);

            // Calculate the scale needed to fit the A4 paper within the viewport
            float scale = Math.Min(skControl.Width / viewPortRectangle.Width, skControl.Height / viewPortRectangle.Height);

            // Reduce the scale to zoom out a bit and show the grey background, explicitly casting the result to float
            scale *= (float)0.95;  // Zoom out to 95% of the calculated fit scale

            // Adjust the matrix for scaling
            _currentMatrix.ScaleX = scale;
            _currentMatrix.ScaleY = scale;

            // Calculate the translations to center the view
            _currentMatrix.TransX = (skControl.Width - viewPortRectangle.Width * scale) / 2;
            _currentMatrix.TransY = (skControl.Height - viewPortRectangle.Height * scale) / 2;

            // Update the transformation matrix
            matrix = _currentMatrix;

            // Debug output to verify correct matrix values
            Debug.WriteLine($"Matrix: Scale({matrix.ScaleX}, {matrix.ScaleY}) Translate({matrix.TransX}, {matrix.TransY})");

            // Invalidate the SKControl to apply new transformations and redraw
            skControl.Invalidate();
        }

        private void SaveCanvasAsA4PDF(string path)
        {
            float displayScaleFactor = GetDisplayScaleFactor();  // Get the scale factor from system settings
            int targetDPI = 300; // Standard DPI for PDF output

            // A4 dimensions in points (1 point = 1/72 inch)
            float widthPoints = 595;  // A4 width in points
            float heightPoints = 842; // A4 height in points

            using (var stream = new SKFileWStream(path))
            {
                using (var document = SKDocument.CreatePdf(stream, targetDPI))
                {
                    var canvas = document.BeginPage(widthPoints, heightPoints);

                    // Apply a scaling factor to adjust for the display scaling
                    float scaleFactor = 1 / displayScaleFactor; // Inverse of display scaling
                    canvas.Scale(scaleFactor);

                    // Draw the content to the canvas here
                    DrawCanvasContents(canvas);

                    // Complete the drawing of the current page
                    document.EndPage();
                    document.Close();
                }
            }
        }

        private float GetDisplayScaleFactor()
        {
            using (Graphics g = CreateGraphics())
            {
                return g.DpiX / 96f; // Standard DPI is 96, so scaling is display DPI divided by 96
            }
        }

        private void DrawCanvasContents(SKCanvas canvas)
        {
            foreach (var obj in objects)
            {
                switch (obj.Type)
                {
                    case DrawableObjectType.Image:
                        if (obj.OriginalImage != null) // Ensure the original high-resolution image is used for exports
                        {
                            SKRect destRect = new SKRect(obj.Bounds.Left, obj.Bounds.Top, obj.Bounds.Left + obj.Bounds.Width, obj.Bounds.Top + obj.Bounds.Height);
                            canvas.DrawBitmap(obj.OriginalImage, destRect);
                        }
                        break;
                    case DrawableObjectType.Text:
                    case DrawableObjectType.Header:
                        // Drawing text or headers (implementation depends on your application's needs)
                        obj.Draw(canvas);
                        break;
                }
            }
        }

        private void buttonprint_Click(object sender, EventArgs e)
        {
            PrintCanvas();
        }

        private void button_zoomoutcenter_Click(object sender, EventArgs e)
        {
            ZoomToFitCanvas();
        }

        private void button_savepdf_Click(object sender, EventArgs e)
        {
            SaveCanvasAsA4PDF("test.pdf");
        }
    }

    public class DrawableObject
    {
        public SKRect Bounds { get; set; }
        public string Text { get; set; }
        public SKBitmap Image { get; private set; } // Display image
        public SKBitmap OriginalImage { get; private set; } // Original high-resolution image
        public DrawableObjectType Type { get; set; }
        public bool IsSelected { get; set; }
        public bool IsHovered { get; set; }

        private const int EdgeThreshold = 30; // Sensitivity near the edges for resizing
        private float AspectRatio;

        public DrawableObject(float x, float y, float width, float height, string text, DrawableObjectType type)
        {
            Bounds = new SKRect(x, y, x + width, y + height);
            Text = text;
            Type = type;
            if (type == DrawableObjectType.Image && !string.IsNullOrEmpty(text))
            {
                SetImage(SKBitmap.Decode(text)); // Load the image at its original resolution
            }
        }

        public void SetImage(SKBitmap image)
        {
            OriginalImage = image; // Keep the original image at full resolution
            AspectRatio = OriginalImage.Width / (float)OriginalImage.Height;
            UpdateDisplayImage();
        }

        private void UpdateDisplayImage()
        {
            // Create a scaled-down version for GUI display based on the bounds
            if (OriginalImage != null)
            {
                var scaledInfo = new SKImageInfo((int)Bounds.Width, (int)Bounds.Height);
                Image = OriginalImage.Resize(scaledInfo, SKFilterQuality.Medium); // Use medium quality for display purposes
            }
        }

        public void Draw(SKCanvas canvas)
        {
            var paint = new SKPaint
            {
                IsAntialias = true,
                Color = IsSelected ? SKColors.Red : SKColors.Black
            };

            switch (Type)
            {
                case DrawableObjectType.Text:
                    paint.Typeface = SKTypeface.FromFamilyName("Old English Text MT");
                    paint.TextSize = 9 * 96f / 72f; // Assuming text size in points, converted to pixels
                    DrawJustifiedText(canvas, paint);
                    break;
                case DrawableObjectType.Header:
                    paint.Typeface = SKTypeface.FromFamilyName("Algerian");
                    paint.TextSize = 48 * 96f / 72f; // Assuming header size in points, converted to pixels
                    DrawLeftAlignedText(canvas, paint);
                    break;
                case DrawableObjectType.Image:
                    DrawImage(canvas);
                    break;
            }

            // Draw selection or hover rectangle
            if (IsSelected || IsHovered)
            {
                var selectionPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Red,
                    StrokeWidth = 2
                };
                canvas.DrawRect(Bounds, selectionPaint);
            }
        }

        private void DrawJustifiedText(SKCanvas canvas, SKPaint paint)
        {
            float x = Bounds.Left;
            float y = Bounds.Top + paint.TextSize;
            string[] words = Text.Split(' ');
            float spaceWidth = paint.MeasureText(" ");
            List<string> lineWords = new List<string>();
            float lineWidth = 0;

            foreach (var word in words)
            {
                float wordWidth = paint.MeasureText(word);
                if (lineWidth + wordWidth + spaceWidth > Bounds.Width)
                {
                    DrawJustifiedLine(canvas, lineWords, x, y, Bounds.Width, paint);
                    lineWords.Clear();
                    lineWidth = 0;
                    y += paint.FontSpacing;
                }

                lineWords.Add(word);
                lineWidth += wordWidth + spaceWidth;
            }

            if (lineWords.Count > 0)
            {
                DrawLeftAlignedLine(canvas, lineWords, x, y, paint);
            }
        }

        private void DrawLeftAlignedText(SKCanvas canvas, SKPaint paint)
        {
            float x = Bounds.Left;
            float y = Bounds.Top + paint.TextSize;
            string[] words = Text.Split(' ');
            float currentX = x;

            foreach (var word in words)
            {
                canvas.DrawText(word, currentX, y, paint);
                currentX += paint.MeasureText(word + " ");
            }
        }

        private void DrawText(SKCanvas canvas, SKPaint paint)
        {
            float x = Bounds.Left;
            float y = Bounds.Top + paint.TextSize;
            string[] words = Text.Split(' ');
            float spaceWidth = paint.MeasureText(" ");
            List<string> lineWords = new List<string>();
            float lineWidth = 0;

            foreach (var word in words)
            {
                float wordWidth = paint.MeasureText(word);
                if (lineWidth + wordWidth + spaceWidth > Bounds.Width)
                {
                    DrawJustifiedLine(canvas, lineWords, x, y, Bounds.Width, paint);
                    lineWords.Clear();
                    lineWidth = 0;
                    y += paint.FontSpacing;
                }

                lineWords.Add(word);
                lineWidth += wordWidth + spaceWidth;
            }

            if (lineWords.Count > 0)
            {
                DrawLeftAlignedLine(canvas, lineWords, x, y, paint);
            }
        }

        private void DrawImage(SKCanvas canvas)
        {
            if (Image != null)
            {
                // Calculate the scaling needed to fit the image within the bounds
                float scaleX = Bounds.Width / Image.Width;
                float scaleY = Bounds.Height / Image.Height;
                float scale = Math.Min(scaleX, scaleY); // Use the smaller scale to ensure the entire image fits

                // Calculate the new width and height
                float scaledWidth = Image.Width * scale;
                float scaledHeight = Image.Height * scale;

                // Calculate top-left corner position to center the image
                float left = Bounds.Left + (Bounds.Width - scaledWidth) / 2;
                float top = Bounds.Top + (Bounds.Height - scaledHeight) / 2;

                // Define the destination rectangle for the scaled image
                SKRect destRect = new SKRect(Bounds.Left, Bounds.Top, Bounds.Left + Bounds.Width, Bounds.Top + Bounds.Height);

                // Draw the image to the canvas using the destination rectangle
                canvas.DrawBitmap(Image, destRect);
            }
        }

        private void DrawJustifiedLine(SKCanvas canvas, List<string> words, float x, float y, float width, SKPaint paint)
        {
            float totalWidth = words.Sum(word => paint.MeasureText(word));
            float extraSpace = (width - totalWidth) / Math.Max(1, words.Count - 1);

            float currentX = x;
            for (int i = 0; i < words.Count; i++)
            {
                string word = words[i];
                canvas.DrawText(word, currentX, y, paint);
                currentX += paint.MeasureText(word) + (i < words.Count - 1 ? extraSpace : 0);
            }
        }

        private void DrawLeftAlignedLine(SKCanvas canvas, List<string> words, float x, float y, SKPaint paint)
        {
            float currentX = x;
            foreach (var word in words)
            {
                canvas.DrawText(word, currentX, y, paint);
                currentX += paint.MeasureText(word + " ");
            }
        }

        public bool HitTest(SKPoint point)
        {
            return Bounds.Contains(point);
        }

        public ResizeDirection GetResizeDirection(SKPoint point)
        {
            if (Math.Abs(point.X - Bounds.Right) <= EdgeThreshold && Math.Abs(point.Y - Bounds.Bottom) <= EdgeThreshold)
                return ResizeDirection.BottomRight;
            if (Math.Abs(point.X - Bounds.Left) <= EdgeThreshold && Math.Abs(point.Y - Bounds.Bottom) <= EdgeThreshold)
                return ResizeDirection.BottomLeft;
            if (Math.Abs(point.X - Bounds.Right) <= EdgeThreshold && Math.Abs(point.Y - Bounds.Top) <= EdgeThreshold)
                return ResizeDirection.TopRight;
            if (Math.Abs(point.X - Bounds.Left) <= EdgeThreshold && Math.Abs(point.Y - Bounds.Top) <= EdgeThreshold)
                return ResizeDirection.TopLeft;

            return ResizeDirection.None;
        }

        public void Resize(SKPoint point, ResizeDirection direction)
        {
            switch (direction)
            {
                case ResizeDirection.BottomRight:
                    Bounds = new SKRect(Bounds.Left, Bounds.Top, point.X, point.Y);
                    if (Type == DrawableObjectType.Image && OriginalImage != null)
                    {
                        ResizeImage(point);
                    }
                    break;
                case ResizeDirection.BottomLeft:
                case ResizeDirection.TopRight:
                case ResizeDirection.TopLeft:
                    // Implement other directions if needed
                    break;
            }
        }

        private void ResizeImage(SKPoint point)
        {
            float newWidth = point.X - Bounds.Left;
            float newHeight = newWidth / AspectRatio;
            if (newHeight + Bounds.Top > point.Y)
            {
                newHeight = point.Y - Bounds.Top;
                newWidth = newHeight * AspectRatio;
            }
            Bounds = new SKRect(Bounds.Left, Bounds.Top, Bounds.Left + newWidth, Bounds.Top + newHeight);

            Image?.Dispose();
            Image = OriginalImage.Resize(new SKImageInfo((int)Bounds.Width, (int)Bounds.Height), SKFilterQuality.High);
            UpdateDisplayImage();
        }
    }

    public enum ResizeDirection
    {
        None, TopLeft, TopRight, BottomLeft, BottomRight
    }
    
    public enum DrawableObjectType
    {
        Text,
        Image,
        Header
    }
}
