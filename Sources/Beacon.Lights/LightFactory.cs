using System;
using Beacon.Core;
using Beacon.Lights.Console;
using Beacon.Lights.Delcom;

namespace Beacon.Lights
{
    public class LightFactory
    {
        public IBuildLight CreateLight(string deviceName)
        {
            switch (deviceName.ToLower())
            {
                case "console":
                    return new ConsoleBuildLight();

                case "delcom":
                    return new DelcomLight();

                default:
                {
                    throw new NotSupportedException(deviceName + " is not a support build device");
                }
            }
        }
    }
}