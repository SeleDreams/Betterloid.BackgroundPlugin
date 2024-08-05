
using System.IO;
using System.Windows.Media;
using System.Windows;
using System;
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

        public string CustomBackground { get; private set; }
        public VerticalAlignment VerticalAlignment { get; private set; }
        public HorizontalAlignment HorizontalAlignment { get; private set; }
        public Stretch Stretch { get; private set; }
        public float Opacity { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public static void CreateBGConfig()
        {
            BackgroundConfigJson config = new BackgroundConfigJson();
            Directory.CreateDirectory(Betterloid.Betterloid.BasePath + "/Backgrounds");
            string json = Betterloid.Betterloid.SerializeObject(config);
            File.WriteAllText(Betterloid.Betterloid.BasePath + "/Backgrounds/config.json", json);
        }

        public void LoadBGConfig(string config)
        {
            string json = File.ReadAllText(config);
            BackgroundConfigJson backgroundConfig = Betterloid.Betterloid.DeserializeObject<BackgroundConfigJson>(json);

            if (!string.IsNullOrEmpty(backgroundConfig.CustomBackground))
            {
                CustomBackground = Betterloid.Betterloid.BasePath + "/Backgrounds/" + backgroundConfig.CustomBackground;
            }
            else
            {
                CustomBackground = "";
            }

            if (!string.IsNullOrEmpty(backgroundConfig.HorizontalAlignment))
            {
                HorizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), backgroundConfig.HorizontalAlignment);
            }
            else
            {
                HorizontalAlignment = HorizontalAlignment.Left;
            }

            if (!string.IsNullOrEmpty(backgroundConfig.VerticalAlignment))
            {
                VerticalAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), backgroundConfig.VerticalAlignment);
            }
            else
            {
                VerticalAlignment = VerticalAlignment.Top;
            }

            if (!string.IsNullOrEmpty(backgroundConfig.HorizontalAlignment))
            {
                Stretch = (Stretch)Enum.Parse(typeof(Stretch), backgroundConfig.Stretch);
            }

            if (!string.IsNullOrEmpty(backgroundConfig.Width) && backgroundConfig.Width != "auto")
            {
                Width = int.Parse(backgroundConfig.Width);
            }
            else if (backgroundConfig.Width == "auto")
            {
                Width = -1;
            }
            if (!string.IsNullOrEmpty(backgroundConfig.Height) && backgroundConfig.Height != "auto")
            {
                Height = int.Parse(backgroundConfig.Height);
            }
            else if (backgroundConfig.Height == "auto")
            {
                Height = -1;
            }
            Opacity = backgroundConfig.Opacity;
        }

        public void SetupConfig(string vbName)
        {
            string config = Betterloid.Betterloid.BasePath + "/Backgrounds/config.json";
            if (!File.Exists(config))
            {
                CreateBGConfig();
            }
            LoadBGConfig(config);

            config = Betterloid.Betterloid.BasePath + "/Backgrounds/" + vbName + ".json";
            if (File.Exists(config))
            {
                LoadBGConfig(config);
            }
        }
    }
}
