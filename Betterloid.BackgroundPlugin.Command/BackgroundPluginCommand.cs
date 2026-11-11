using Betterloid;
using System.Windows;
using System.Windows.Controls;
using System;

#if VOCALOID6 || VOCALOID7
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

namespace BackgroundPluginCommand
{
    public enum PitchMode
    {
        Hard,
        Soft,
        Stock
    }


    public class BackgroundPluginCommand : IPlugin
    {
        private static VoiceBank GetVB(WIVSMMidiPart midiPart)
        {
            VoiceBank vb = null;
#if VOCALOID5
                vb = App.DatabaseManager.GetVoiceBankByCompID(midiPart.VoiceBankID);
#elif VOCALOID6 || VOCALOID7
            vb = midiPart.VoiceBank();
#endif
            return vb;
        }

        public void Startup()
        {
            MainWindow window = App.Current.MainWindow as MainWindow;
            var xMusicalEditorDiv = window.FindName("xMusicalEditorDiv") as MusicalEditorDivision;
            if (xMusicalEditorDiv == null)
            {
                throw new NullReferenceException();
            }
            var MusicalEditor = xMusicalEditorDiv.DataContext as MusicalEditorViewModel;
            if (MusicalEditor == null)
            {
                throw new NullReferenceException();
            }
            var activePart = MusicalEditor.ActivePart;
            if (activePart == null)
            {
                return;
            }
            var currentVB = GetVB(activePart);
            if (currentVB == null)
            {
                Betterloid.Wrappers.MessageBoxDeliverer.GeneralError("The current VB is null !");
            }
            string vbInfos = $"Current voicebank Name is \"{currentVB.Name}\". Current voicebank ID is \"{currentVB.CompID}\". Both can be used to set a custom background. Either {currentVB.Name}.json, either {currentVB.CompID}.json. If the name contains characters that cannot be used as filenames, use the ID instead.";

             Betterloid.Wrappers.MessageBoxDeliverer.GeneralInformation(vbInfos);
            
        }
    }
}
