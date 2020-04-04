using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Globalization;

namespace rlgn
{
    class EngineSettings
    {
        private string[] cfile;

        private string ga;
        private int screenX;
        private int screenY;
        private int texture;
        private int light;
        private int glow;
        private int shadow;
        private decimal gamma;
        private bool mouse;
        private bool texturedetail;
        private bool fullscreen;
       
        public EngineSettings()
        {
            try
            {
                
                cfile = File.ReadAllLines(System.AppDomain.CurrentDomain.BaseDirectory + "R3Engine.ini");
                readsettings();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Load Settings: Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string GraphicsAdapter
        {
            get
            {
                return ga;
            }
            set
            {
                this.ga = value.Replace(' ', '%');
            }
        }
        public string Resolution
        {
            get
            {
                return screenX.ToString() + 'x' + screenY.ToString();
            }
            set
            {
                string[] val = value.Split(new char[] { 'x' });
                screenX = Convert.ToInt32(val[0]);
                screenY = Convert.ToInt32(val[1]);
            }
        }
        public int TextureDetail
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;
                if (texture > 3)
                    texture = 3;
                if (texture < 0)
                    texture = 0;
            }
        }

        public int DynamicLight
        {
            get
            {
                return light;
            }
            set
            {
                light = value;
                if (light > 3)
                    light = 3;
                if (light < 0)
                    light = 0;
            }
        }

        public int GlowEffect
        {
            get
            {
                return glow;
            }
            set
            {
                glow = value;
                if (glow > 2)
                    glow = 2;
                if (glow < 0)
                    glow = 0;
            }
        }

        public int ShadowDetail
        {
            get
            {
                return shadow;
            }
            set
            {
                shadow = value;
                if (shadow > 3)
                    shadow = 3;
                if (shadow < 0)
                    shadow = 0;
            }
        }

        public decimal Gamma
        {
            get
            {
                return gamma;
            }
            set
            {
                gamma = value;
                if (gamma < (decimal)0.8 || gamma > (decimal)1.8 || (gamma != (decimal)0.8 && gamma != (decimal)1.0 && gamma != (decimal)1.2 && gamma != (decimal)1.4 && gamma != (decimal)1.6 && gamma != (decimal)1.8))
                {
                    gamma = (decimal)1.0;
                }
            }
        }

        public bool MouseAcceleration
        {
            get
            {
                return mouse;
            }
            set
            {
                mouse = value;
            }
        }

        public bool DetailedTextures
        {
            get
            {
                return texturedetail;
            }
            set
            {
                texturedetail = value;
            }
        }

        public bool FullScreen
        {
            get
            {
                return fullscreen;
            }
            set
            {
                fullscreen = value;
            }
        }

        public void SaveSettings()
        {
            string str;
            
            for (int i = 0; i < cfile.Length; i++)
            {
                str = cfile[i];
                if (str.StartsWith("ScreenXSize"))
                {
                    cfile[i] = "ScreenXSize=" + screenX.ToString();
                }
                else if (str.StartsWith("ScreenYSize"))
                {
                    cfile[i] = "ScreenYSize=" + screenY.ToString();
                }
                else if (str.StartsWith("Gamma"))
                {
                    cfile[i] = "Gamma=" + gamma.ToString("0.0", CultureInfo.InvariantCulture);
                }
                else if (str.StartsWith("BboShasi"))
                {
                    cfile[i] = "BboShasit=" + glow.ToString();
                }
                else if (str.StartsWith("TextureDetail"))
                {
                    cfile[i] = "TextureDetail=" + texture.ToString();
                }
                else if (str.StartsWith("DynamicLight"))
                {
                    cfile[i] = "DynamicLight=" + light.ToString();
                }
                else if (str.StartsWith("ShadowDetail"))
                {
                    cfile[i] = "ShadowDetail=" + shadow.ToString();
                }
                else if (str.StartsWith("bMouseAccelation"))
                {
                    cfile[i] = "bMouseAccelation=" + (mouse ? "TRUE" : "FALSE");
                }
                else if (str.StartsWith("bDetailTexture"))
                {
                    cfile[i] = "bDetailTexture=" + (texturedetail ? "TRUE" : "FALSE");
                }
                else if (str.StartsWith("bFullScreen"))
                {
                    cfile[i] = "bFullScreen=" + (fullscreen ? "TRUE" : "FALSE");
                }
                else if (str.StartsWith("Adapter"))
                {
                    cfile[i] = "Adapter=" + ga;
                }
            }
            File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + "R3Engine.ini");
            File.WriteAllLines(System.AppDomain.CurrentDomain.BaseDirectory + "R3Engine.ini", cfile);
        }

        private void readsettings()
        {
            string str;
            string[] val;
            for (int i = 0; i < cfile.Length; i++)
            {
                str = cfile[i];
                if (str.StartsWith("ScreenXSize"))
                {
                    val = str.Split(new char[] { '=' });
                    screenX = Convert.ToInt32(val[1]);
                }
                else if (str.StartsWith("ScreenYSize"))
                {
                    val = str.Split(new char[] { '=' });
                    screenY = Convert.ToInt32(val[1]);
                }
                else if (str.StartsWith("Gamma"))
                {
                    val = str.Split(new char[] { '=' });
                    gamma = Convert.ToDecimal(val[1], CultureInfo.InvariantCulture);
                }
                else if (str.StartsWith("BboShasi"))
                {
                    val = str.Split(new char[] { '=' });
                    glow = Convert.ToInt32(val[1]);
                }
                else if (str.StartsWith("TextureDetail"))
                {
                    val = str.Split(new char[] { '=' });
                    texture = Convert.ToInt32(val[1]);
                }
                else if (str.StartsWith("DynamicLight"))
                {
                    val = str.Split(new char[] { '=' });
                    light = Convert.ToInt32(val[1]);
                }
                else if (str.StartsWith("ShadowDetail"))
                {
                    val = str.Split(new char[] { '=' });
                    shadow = Convert.ToInt32(val[1]);
                }
                else if (str.StartsWith("bMouseAccelation"))
                {
                    val = str.Split(new char[] { '=' });
                    mouse = Convert.ToBoolean(val[1]);
                }
                else if (str.StartsWith("bDetailTexture"))
                {
                    val = str.Split(new char[] { '=' });
                    texturedetail = Convert.ToBoolean(val[1]);
                }
                else if (str.StartsWith("bFullScreen"))
                {
                    val = str.Split(new char[] { '=' });
                    fullscreen = Convert.ToBoolean(val[1]);
                }
                else if (str.StartsWith("Adapter"))
                {
                    val = str.Split(new char[] { '=' });
                    ga = val[1].Replace('%', ' ');
                }
            }
        }
    }
}
