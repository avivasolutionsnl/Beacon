using Beacon.Core;

namespace Beacon.Lights.Delcom
{
    internal class DelcomLight : IBuildLight
    {
        private readonly DelcomLed delcomLed = new DelcomLed();

        public void Success()
        {
            VerboseThemeChange("GREEN");
            delcomLed.SetLed(DelcomApis.REDLED, false, false);
            delcomLed.SetLed(DelcomApis.GREENLED, true, false);
            delcomLed.SetLed(DelcomApis.BLUELED, false, false);
        }

        public void Fixed()
        {
            VerboseThemeChange("GREEN");
            delcomLed.SetLed(DelcomApis.REDLED, false, false);
            delcomLed.SetLed(DelcomApis.GREENLED, true, false);
            delcomLed.SetLed(DelcomApis.BLUELED, false, false);
        }

        public void Investigate()
        {
            VerboseThemeChange("ORANGE");
            delcomLed.SetLed(DelcomApis.REDLED, false, false);
            delcomLed.SetLed(DelcomApis.GREENLED, false, false);
            delcomLed.SetLed(DelcomApis.BLUELED, true, false);
        }

        public void Fail()
        {
            VerboseThemeChange("RED");
            delcomLed.SetLed(DelcomApis.REDLED, true, false);
            delcomLed.SetLed(DelcomApis.GREENLED, false, false);
            delcomLed.SetLed(DelcomApis.BLUELED, false, false);
        }

        public void NoStatus()
        {
            VerboseThemeChange("OFF");
            delcomLed.SetLed(DelcomApis.REDLED, false, false);
            delcomLed.SetLed(DelcomApis.GREENLED, false, false);
            delcomLed.SetLed(DelcomApis.BLUELED, false, false);
        }

        private static void VerboseThemeChange(string newTheme)
        {
            Logger.Verbose("Switching LED to {0}.", newTheme);
        }
    }
}