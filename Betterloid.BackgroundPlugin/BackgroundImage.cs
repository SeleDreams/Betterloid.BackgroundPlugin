using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using WpfAnimatedGif;
using System.Windows.Controls;
using System.Windows.Media;
using System;

using System.Windows;

#if VOCALOID6
using Yamaha.VOCALOID;
using Yamaha.VOCALOID.VDM;
using Yamaha.VOCALOID.MusicalEditor;
#elif VOCALOID5
using Yamaha.VOCALOID.VOCALOID5;
using Yamaha.VOCALOID.VDM;
using Yamaha.VOCALOID.VOCALOID5.MusicalEditor;
#endif
namespace BackgroundPlugin
{
    public class BackgroundImage
    {
        public readonly BackgroundConfig config;
        private BitmapImage bitmapImage;
        public Image Image { get; private set; }
        private string loadedImage;
        private string filename;
        public double Width => Image.ActualWidth;
        public double Height => Image.ActualHeight;
        public TranslateTransform TranslateTransform { get; set; }

        public BackgroundImage()
        {
            config = new BackgroundConfig();
            Image = new Image();
            bitmapImage = new BitmapImage();
            TranslateTransform = new TranslateTransform();
        }

#if VOCALOID5
        private static string GetVoiceImage(VoiceBank vb)
        {
            int major = 0, minor = 0, revision = 0;
            if (!vb.Version(ref major, ref minor, ref revision))
                return string.Empty;

            if (major == 5)
            {
                return $"{Path.GetDirectoryName(vb.Path)}/setup.bmp";
            }
            else
            {
                return $"{FolderLocation.PathSystemRscVoice}/{vb.CompID}/setup.bmp";
            }

        }
#elif VOCALOID6
        private static string GetVoiceImage(VoiceBank vb)
        {
            return vb.GetImagePath();
        }
#endif
        public void UpdateImagePos()
        {
            Image.RenderTransform = TranslateTransform;
        }

        private bool IsBitmapReady()
        {
            return bitmapImage != null
                && bitmapImage?.UriSource != null
                && bitmapImage?.PixelWidth > 0
                && bitmapImage?.PixelHeight > 0;
        }

        private bool TryGetSourceSize(out double srcW, out double srcH)
        {
            srcW = 0;
            srcH = 0;
            try
            {
                if (bitmapImage == null || bitmapImage.UriSource == null)
                    return false;
                srcW = bitmapImage.PixelWidth;
                srcH = bitmapImage.PixelHeight;
                return srcW > 0 && srcH > 0;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private void GetDisplayedSize(double availW, double availH, out double dispW, out double dispH)
        {
            dispW = 0;
            dispH = 0;
            if (!TryGetSourceSize(out double srcW, out double srcH))
                return;

            if (config.Width > 0)
                availW = config.Width;
            if (config.Height > 0)
                availH = config.Height;

            switch (config.Stretch)
            {
                case Stretch.Fill:
                    dispW = availW;
                    dispH = availH;
                    break;
                case Stretch.UniformToFill:
                    {
                        double s = Math.Max(availW / srcW, availH / srcH);
                        dispW = srcW * s;
                        dispH = srcH * s;
                        break;
                    }
                case Stretch.Uniform:
                    {
                        double s = Math.Min(availW / srcW, availH / srcH);
                        dispW = srcW * s;
                        dispH = srcH * s;
                        break;
                    }
                default:
                    dispW = config.Width > 0 ? config.Width : srcW;
                    dispH = config.Height > 0 ? config.Height : srcH;
                    break;
            }

            if (config.VerticalAlignment == VerticalAlignment.Stretch && config.Stretch != Stretch.Fill)
            {
                dispH = availH;
                dispW = srcW * (availH / srcH);
            }
            if (config.HorizontalAlignment == HorizontalAlignment.Stretch && config.Stretch != Stretch.Fill)
            {
                dispW = availW;
                dispH = srcH * (availW / srcW);
            }
        }

        public void RefreshImagePosition(ZoomScrollViewer zoomScrollViewer, MusicalEditorViewModel musicalEditor)
        {
            zoomScrollViewer.UpdateLayout();

            double availW = zoomScrollViewer.ViewportWidth;
            double availH = zoomScrollViewer.ViewportHeight;
            GetDisplayedSize(availW, availH, out double w, out double h);
            if (w <= 0 || h <= 0)
                return;

            if (config.Stretch == Stretch.Fill || config.Stretch == Stretch.UniformToFill)
            {
                Image.Width = config.Width > 0 ? config.Width : availW;
                Image.Height = config.Height > 0 ? config.Height : availH;
            }
            else
            {
                Image.Width = w;
                Image.Height = h;
            }

            double adjustmentOffsetX = 0.0;
            double adjustmentOffsetY = 0.0;

            if (config.HorizontalAlignment == HorizontalAlignment.Right)
                adjustmentOffsetX = availW - w;
            else if (config.HorizontalAlignment == HorizontalAlignment.Center)
                adjustmentOffsetX = (availW - w) * 0.5;

            if (config.VerticalAlignment == VerticalAlignment.Bottom)
                adjustmentOffsetY = availH - h;
            else if (config.VerticalAlignment == VerticalAlignment.Center)
                adjustmentOffsetY = (availH - h) * 0.5;

            TranslateTransform.X = zoomScrollViewer.HorizontalOffset + adjustmentOffsetX;
            TranslateTransform.Y = zoomScrollViewer.VerticalOffset + adjustmentOffsetY;
            Image.RenderTransform = TranslateTransform;
        }

        public void UpdateBackgroundImage(VoiceBank vb, double width, double height)
        {
            if (vb == null)
                return;

            if (loadedImage != vb.CompID)
            {
                ImageBehavior.SetAnimatedSource(Image, null);
                config.SetupConfig(vb.Name);
                filename = string.IsNullOrEmpty(config.CustomBackground) ? GetVoiceImage(vb) : config.CustomBackground;
                if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
                    return;

                loadedImage = vb.CompID;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(filename);
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                ImageBehavior.SetAnimatedSource(Image, bitmapImage);
                ImageBehavior.SetRepeatBehavior(Image, RepeatBehavior.Forever);
                ImageBehavior.SetAutoStart(Image, true);
            }

            Image.Stretch = config.Stretch;
            Image.Opacity = config.Opacity;
            Image.HorizontalAlignment = HorizontalAlignment.Left;
            Image.VerticalAlignment = VerticalAlignment.Top;

            GetDisplayedSize(width, height, out double w, out double h);
            if (w <= 0 || h <= 0)
                return;

            if (config.Stretch == Stretch.Fill || config.Stretch == Stretch.UniformToFill)
            {
                Image.Width = config.Width > 0 ? config.Width : width;
                Image.Height = config.Height > 0 ? config.Height : height;
            }
            else
            {
                Image.Width = w;
                Image.Height = h;
            }

            RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Fant);
        }
    }
}
