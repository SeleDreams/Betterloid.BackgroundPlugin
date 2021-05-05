using Betterloid;
using System.Windows;
using Yamaha.VOCALOID.VOCALOID5;
using System.Reflection;
using Yamaha.VOCALOID.VOCALOID5.MusicalEditor;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;
using Yamaha.VOCALOID.VSM;
using System.IO;
using Yamaha.VOCALOID.VDM;
using Newtonsoft.Json;

namespace BackgroundPlugin
{
    public class BackgroundPlugin : IPlugin
    {
        ImageBrush imageBrush;
        MusicalEditorViewModel musicalEditor;
        MethodInfo drawBG;
        PianorollView view;
        ZoomScrollViewer zoomScrollViewer;
        FastCanvas xCanvasBackgroundNote;

        public void CreateBGConfig()
        {
            BackgroundConfig config = new BackgroundConfig();
            Directory.CreateDirectory("Backgrounds");
            string json = JsonConvert.SerializeObject(config);
            File.WriteAllText("Backgrounds/config.json", json);
        }

        public void LoadBGConfig(string config, ref string filename, ref Stretch stretch, ref AlignmentY alignmentY, ref AlignmentX alignmentX, ref float opacity)
        {
            string json = File.ReadAllText(config);
            BackgroundConfig backgroundConfig = JsonConvert.DeserializeObject<BackgroundConfig>(json);

            if (!string.IsNullOrEmpty(backgroundConfig.CustomBackground))
            {
                filename = AppDomain.CurrentDomain.BaseDirectory + "/Backgrounds/" + backgroundConfig.CustomBackground;
            }

            if (!string.IsNullOrEmpty(backgroundConfig.HorizontalAlignment))
            {
                alignmentX = (AlignmentX)Enum.Parse(typeof(AlignmentX), backgroundConfig.HorizontalAlignment);
            }

            if (!string.IsNullOrEmpty(backgroundConfig.HorizontalAlignment))
            {
                alignmentY = (AlignmentY)Enum.Parse(typeof(AlignmentY), backgroundConfig.VerticalAlignment);
            }

            if (!string.IsNullOrEmpty(backgroundConfig.HorizontalAlignment))
            {
                stretch = (Stretch)Enum.Parse(typeof(Stretch), backgroundConfig.Stretch);
            }

            opacity = backgroundConfig.Opacity;
        }

        public void UpdateBackgroundImage()
        {
            WIVSMMidiPart part = musicalEditor.ActivePart;
            if (part == null)
            {
                return;
            }

            VoiceBank vb = App.DatabaseManager.GetVoiceBankByCompID(part.VoiceBankID);

            string filename = "";
            Stretch stretch = Stretch.UniformToFill;
            AlignmentY alignmentY = AlignmentY.Top;
            AlignmentX alignmentX = AlignmentX.Center;
            float opacity = 0.0f;

            string config = AppDomain.CurrentDomain.BaseDirectory + "/Backgrounds/config.json";
            if (!File.Exists(config))
            {
                CreateBGConfig();
            }
            LoadBGConfig(config, ref filename, ref stretch, ref alignmentY, ref alignmentX, ref opacity);
            config = AppDomain.CurrentDomain.BaseDirectory + "/Backgrounds/" + vb.Name + ".json";
            if (File.Exists(config))
            {
                LoadBGConfig(config, ref filename, ref stretch, ref alignmentY, ref alignmentX, ref opacity);
            }

            if (!File.Exists(filename))
            {
                filename = System.IO.Path.GetDirectoryName(vb.Path) + "/setup.bmp";
                if (!File.Exists(filename))
                {
                    filename = FolderLocation.PathSystemRscVoice + "/" + vb.CompID + "/setup.bmp";
                    if (!File.Exists(filename))
                    {
                        return;
                    }
                }
            }
            imageBrush = new ImageBrush(new BitmapImage(new Uri(filename)))
            {
                Stretch = stretch,
                AlignmentY = alignmentY,
                AlignmentX = alignmentX,
                Opacity = opacity
            };
        }

        public void DrawBG()
        {
            drawBG.Invoke(view, new object[] { musicalEditor });
            Rectangle rectangle = new Rectangle();
            rectangle.Fill = imageBrush;
            rectangle.Width = zoomScrollViewer.ViewportWidth;
            rectangle.Height = zoomScrollViewer.ViewportHeight;
            Canvas.SetLeft(rectangle, zoomScrollViewer.HorizontalOffset);
            Canvas.SetTop(rectangle, zoomScrollViewer.VerticalOffset);
            xCanvasBackgroundNote.Children.Add(rectangle);
        }

        public void Startup()
        {
            
            MainWindow window = Application.Current.MainWindow as MainWindow;
            window.Loaded += (object sender, RoutedEventArgs args) =>
            {
                try
                {
                    MusicalEditorDivision xMusicalEditorDiv = window.FindName("xMusicalEditorDiv") as MusicalEditorDivision;
                    musicalEditor = xMusicalEditorDiv.DataContext as MusicalEditorViewModel;
                    zoomScrollViewer = xMusicalEditorDiv.FindName("xPianorollViewer") as ZoomScrollViewer;
                    view = zoomScrollViewer.Content as PianorollView;
                    drawBG = view.GetType().GetMethod("DrawBackgroundNoteCanvas", BindingFlags.NonPublic | BindingFlags.Instance);
                    xCanvasBackgroundNote = view.FindName("xCanvasBackgroundNote") as FastCanvas;

                    musicalEditor.UpdateViewEvent += (object sendr, Yamaha.VOCALOID.VOCALOID5.MusicalEditor.UpdateViewTypeFlag flag, UpdateObserverNotifyEventArgs notifyargs, object addition) =>
                    {
                        UpdateBackgroundImage();
                        DrawBG();
                    };
                    zoomScrollViewer.SizeChanged += (object obj, SizeChangedEventArgs sizeargs) =>
                    {
                        DrawBG();

                    };
                    zoomScrollViewer.ScrollChanged += (object obj, ScrollChangedEventArgs scrollargs) =>
                    {
                        DrawBG();
                    };
                }
                catch (Exception ex)
                {
                    MessageBoxDeliverer.GeneralError("An error occured while setting the background of the piano roll ! " + ex.GetType().ToString() + " : " + ex.Message);
                }

            };
        }
    }
}
