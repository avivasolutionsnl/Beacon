using Beacon.Core;

namespace Beacon.Lights.Shelly
{
    public class ShellyLight : IBuildLight
    {
        private readonly ShellyBulb bulb;

        public ShellyLight()
        {
            bulb = new ShellyBulb();
        }

        public void Configure(string url)
        {
            bulb.Configure(url);
        }

        public void Success()
        {
            VerboseThemeChange("GREEN");
            bulb.Set(0, 255, 0);
        }

        public void Fixed()
        {
            VerboseThemeChange("GREEN");
            bulb.Set(0, 255, 0);
        }

        public void Investigate()
        {
            VerboseThemeChange("ORANGE");
            bulb.Set(255, 215, 0);
        }

        public void Fail()
        {
            VerboseThemeChange("RED");
            bulb.Set(255, 0, 0);
        }

        public void NoStatus()
        {
            VerboseThemeChange("OFF");
            bulb.Off();
        }
        
        private static void VerboseThemeChange(string newTheme)
        {
            Logger.Verbose("Switching LED to {0}.", newTheme);
        }
    }
}