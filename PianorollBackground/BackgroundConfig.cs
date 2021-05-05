using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundPlugin
{
    public class BackgroundConfig
    {
        public string CustomBackground { get; set; } = "";
        public string VerticalAlignment { get; set; } = "Center";
        public string HorizontalAlignment { get; set; } = "Center";
        public string Stretch { get; set; } = "UniformToFill";
        public float Opacity { get; set; } = 0.5f;
    }
}
