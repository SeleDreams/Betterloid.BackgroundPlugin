
using System.IO;
using System.Windows.Media;
using System.Windows;
using System;
using System.Linq;
namespace BackgroundPlugin
{
    
    public class BackgroundConfig {
        private class BackgroundConfigJson
        {
            public string CustomBackground { get; set; } = "";
            public string VerticalAlignment { get; set; } = "Top";
            public string HorizontalAlignment { get; set; } = "Left";
            public string Stretch { get; set; } = "UniformToFill";
            public float Opacity { get; set; } = 0.4f;
            public string Width { get; set; } = "auto";
            public string Height { get; set; } = "auto";
        }

        public static string[] BasePaths = { Betterloid.Betterloid.BasePath, Betterloid.Betterloid.DataPath };
        public string CustomBackground { get; private set; }
        public VerticalAlignment VerticalAlignment { get; private set; }
        public HorizontalAlignment HorizontalAlignment { get; private set; }
        public Stretch Stretch { get; private set; }
        public float Opacity { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public static string FindFile(string path)
        {
            foreach (string basePath in BasePaths)
            {
                string p = basePath + path;
                if (File.Exists(p))
                {
                    return p;
                }
            }
            return string.Empty;
        }

        public static void CreateBGConfig()
        {
            BackgroundConfigJson config = new BackgroundConfigJson();
            Directory.CreateDirectory(Betterloid.Betterloid.DataPath + "/Backgrounds");
            string json = Betterloid.Betterloid.SerializeObject(config);
            File.WriteAllText(Betterloid.Betterloid.DataPath + "/Backgrounds/config.json", json);
        }

        public void LoadBGConfig(string config)
        {
            string json = File.ReadAllText(config);
            BackgroundConfigJson backgroundConfig = null;
            try
            {
                backgroundConfig = Betterloid.Betterloid.DeserializeObject<BackgroundConfigJson>(json);
            }
            catch
            {
                Betterloid.Wrappers.MessageBoxDeliverer.GeneralError($"An error occurred loading {config}. Verify there are no syntax errors and that the Opacity value is correct (the opacity value must not be in quotation marks).");
                backgroundConfig = new BackgroundConfigJson();
            }
            finally
            {
                if (!string.IsNullOrEmpty(backgroundConfig.CustomBackground))
                {
                    CustomBackground = FindFile("/Backgrounds/" + backgroundConfig.CustomBackground);
                }
                else
                {
                    CustomBackground = "";
                }
                try
                {
                    if (!string.IsNullOrEmpty(backgroundConfig.HorizontalAlignment))
                    {
                        HorizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), backgroundConfig.HorizontalAlignment,true);
                    }
                    else
                    {
                        HorizontalAlignment = HorizontalAlignment.Left;
                    }
                }
                catch
                {
                    Betterloid.Wrappers.MessageBoxDeliverer.GeneralError($"The HorizontalAlignment value of {config} is not valid ! Valid values are {String.Join(", ", Enum.GetNames(typeof(HorizontalAlignment)))}");
                    HorizontalAlignment = HorizontalAlignment.Left;
                }
                try
                {
                    if (!string.IsNullOrEmpty(backgroundConfig.VerticalAlignment))
                    {
                        VerticalAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), backgroundConfig.VerticalAlignment,true);
                    }
                    else
                    {
                        VerticalAlignment = VerticalAlignment.Top;
                    }
                }
                catch
                {
                    Betterloid.Wrappers.MessageBoxDeliverer.GeneralError($"The VerticalAlignment value of {config} is not valid ! Valid values are {String.Join(", ", Enum.GetNames(typeof(VerticalAlignment)))}");
                    VerticalAlignment = VerticalAlignment.Top;
                }
                try
                {
                    if (!string.IsNullOrEmpty(backgroundConfig.Stretch))
                    {
                        Stretch = (Stretch)Enum.Parse(typeof(Stretch), backgroundConfig.Stretch,true);
                    }
                    else
                    {
                        Stretch = Stretch.Uniform;
                    }
                }
                catch
                {
                    Betterloid.Wrappers.MessageBoxDeliverer.GeneralError($"The Stretch value of {config} is not valid ! Valid values are {String.Join(", ", Enum.GetNames(typeof(Stretch)))}");
                    Stretch = Stretch.Uniform;
                }

                if (!string.IsNullOrEmpty(backgroundConfig.Width) && string.Compare(backgroundConfig.Width,"auto",true) != 0)
                {
                    try
                    {
                        Width = int.Parse(backgroundConfig.Width);
                    }
                    catch
                    {
                        Betterloid.Wrappers.MessageBoxDeliverer.GeneralError($"The Width value of {config} ({backgroundConfig.Width}) is not valid !");
                        Width = -1;
                    }
                }
                else
                {
                    Width = -1;
                }
                if (!string.IsNullOrEmpty(backgroundConfig.Height) && string.Compare(backgroundConfig.Height, "auto", true) != 0)
                {
                    try
                    {
                        Height = int.Parse(backgroundConfig.Height);
                    }
                    catch
                    {
                        Betterloid.Wrappers.MessageBoxDeliverer.GeneralError($"The Height value of {config} ({backgroundConfig.Height}) is not valid !");
                        Height = -1;
                    }
                }
                else
                {
                    Height = -1;
                }
                Opacity = backgroundConfig.Opacity;
            }
        }

        public static void InitializeConfig()
        {
            string config = FindFile("/Backgrounds/config.json");
            if (!File.Exists(config))
            {
                CreateBGConfig();
            }
        }

        public void SetupConfig(string vbName)
        {
            InitializeConfig();
            string config = FindFile("/Backgrounds/config.json");
            if (string.IsNullOrEmpty(config))
            {
                return;
            }
            LoadBGConfig(config);

            config = FindFile("/Backgrounds/" + vbName + ".json");
            if (File.Exists(config))
            {
                LoadBGConfig(config);
            }
        }
    }
}
