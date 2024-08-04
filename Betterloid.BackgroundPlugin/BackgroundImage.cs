using System.IO;
using System.Windows.Media.Imaging;
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
        public void RefreshImagePosition(ZoomScrollViewer zoomScrollViewer, MusicalEditorViewModel musicalEditor)
        {
            zoomScrollViewer.UpdateLayout();
            double adjustmentOffsetX = 0.0;
            double adjustmentOffsetY = 0.0;
            if (config.Stretch == Stretch.None || config.Stretch == Stretch.Uniform)
            {
                if (config.HorizontalAlignment == HorizontalAlignment.Right)
                {
                    adjustmentOffsetX = zoomScrollViewer.ViewportWidth - Width;
                }
                else if (config.HorizontalAlignment == HorizontalAlignment.Center)
                {
                    adjustmentOffsetX = (zoomScrollViewer.ViewportWidth * 0.5) - (Width * 0.5);
                }
                if (config.VerticalAlignment == VerticalAlignment.Bottom)
                {
                    adjustmentOffsetY = zoomScrollViewer.ViewportHeight - Height;
                }
                else if (config.VerticalAlignment == VerticalAlignment.Center)
                {
                    adjustmentOffsetY = (zoomScrollViewer.ViewportHeight * 0.5) - (Height * 0.5);
                }
            }
            TranslateTransform.X = zoomScrollViewer.HorizontalOffset + adjustmentOffsetX;
            TranslateTransform.Y = zoomScrollViewer.VerticalOffset + adjustmentOffsetY;
            Image.RenderTransform = TranslateTransform;
        }

        public void UpdateImagePos()
        {
            Image.RenderTransform = TranslateTransform;
        }

        public void UpdateBackgroundImage(VoiceBank vb, double width, double height)
        {
            if (vb == null)
            {
                return;
            }
            if (loadedImage != vb.CompID)
            {
                config.SetupConfig(vb.Name);
                filename = string.IsNullOrEmpty(config.CustomBackground) ? GetVoiceImage(vb) : config.CustomBackground;
                loadedImage = vb.CompID;
                bitmapImage = new BitmapImage(new Uri(filename));
                Image.Source = bitmapImage;
            }
            Image.Stretch = config.Stretch;
            Image.Opacity = config.Opacity;
            Image.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            Image.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            if (config.Width > 0)
            {
                Image.Width = config.Width;
            }
            else if (config.Width == 0)
            {
                Image.Width = bitmapImage.Width;
            }
            else if (config.Width < 0)
            {
                Image.Width = width;
            }
            if (config.Height > 0)
            {
                Image.Height = config.Height;
            }
            else if (config.Height == 0)
            {
                Image.Height = bitmapImage.Height;
            }
            else if (config.Height < 0)
            {
                Image.Height = height;
            }
            RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Fant);
        }
    }
}
