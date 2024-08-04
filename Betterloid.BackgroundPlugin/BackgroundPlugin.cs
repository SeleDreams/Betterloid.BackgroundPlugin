using Betterloid;
using System.Windows;
using System.Windows.Controls;
using System;

#if VOCALOID6
using Yamaha.VOCALOID;
using Yamaha.VOCALOID.MusicalEditor;
using Yamaha.VOCALOID.VSM;
using Yamaha.VOCALOID.VDM;
using VOCALOID = Yamaha.VOCALOID;
#elif VOCALOID5
using Yamaha.VOCALOID.VOCALOID5;
using Yamaha.VOCALOID.VOCALOID5.MusicalEditor;
using Yamaha.VOCALOID.VSM;
using Yamaha.VOCALOID.VDM;
using VOCALOID = Yamaha.VOCALOID.VOCALOID5;
#endif

namespace BackgroundPlugin
{
    public class BackgroundPlugin : IPlugin
    {
        
        Panel midiEditorPanel;
        BackgroundImage image;

        private static VoiceBank GetVB(MusicalEditorViewModel musicalEditor)
        {
            WIVSMMidiPart part = musicalEditor.ActivePart;
            if (part == null)
            {
                return null;
            }
            VoiceBank vb;
#if VOCALOID5
                vb = App.DatabaseManager.GetVoiceBankByCompID(musicalEditor.ActivePart.VoiceBankID);
#elif VOCALOID6
            vb = musicalEditor.ActivePart.VoiceBank();
#endif
            return vb;
        }

        private void RefreshImage(ZoomScrollViewer zoomScrollViewer, MusicalEditorViewModel musicalEditor)
        {
            
            image.UpdateBackgroundImage(GetVB(musicalEditor), zoomScrollViewer.ViewportWidth, zoomScrollViewer.ViewportHeight);
            image.RefreshImagePosition(zoomScrollViewer, musicalEditor);
        }


        public void Startup()
        {
            image = new BackgroundImage();
            MainWindow window = Application.Current.MainWindow as MainWindow;
            try
            {
                var xMusicalEditorDiv = window.FindName("xMusicalEditorDiv") as MusicalEditorDivision;
                var musicalEditor = xMusicalEditorDiv.DataContext as MusicalEditorViewModel;
                var zoomScrollViewer = xMusicalEditorDiv.FindName("xPianorollViewer") as ZoomScrollViewer;
                var view = zoomScrollViewer.Content as PianorollView;

                midiEditorPanel = view.FindName("xPanel") as Panel;

                zoomScrollViewer.Loaded += (object s, RoutedEventArgs args) =>
                {
                    if (!midiEditorPanel.Children.Contains(image.Image))
                    {
                        midiEditorPanel.Children.Insert(1, image.Image);
                    }
                    RefreshImage(zoomScrollViewer, musicalEditor);
                };
                
                musicalEditor.UpdateViewEvent += (object sender2, VOCALOID.MusicalEditor.UpdateViewTypeFlag typeFlags, UpdateObserverNotifyEventArgs observer, object addition) =>
                {
                    if (!midiEditorPanel.Children.Contains(image.Image))
                    {
                        midiEditorPanel.Children.Insert(1, image.Image);
                        RefreshImage(zoomScrollViewer, musicalEditor);
                    }
                    RefreshImage(zoomScrollViewer, musicalEditor);
                };


                zoomScrollViewer.SizeChanged += (object sender3, SizeChangedEventArgs e) =>
                {
                    RefreshImage(zoomScrollViewer, musicalEditor);
                };

                zoomScrollViewer.ScrollChanged += (object sender4, ScrollChangedEventArgs e) =>
                {
                    image.RefreshImagePosition(zoomScrollViewer, musicalEditor);
                };
            }
            catch (Exception ex)
            {
                MessageBoxDeliverer.GeneralError("An error occured while setting the background of the piano roll ! " + ex.GetType().ToString() + " : " + ex.Message);
            }
        }
    }
}
