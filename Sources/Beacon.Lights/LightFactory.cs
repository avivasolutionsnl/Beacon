using System;
using System.Collections.Generic;
using System.Linq;

using Beacon.Core;
using Beacon.Lights.Console;
using Beacon.Lights.Delcom;

namespace Beacon.Lights
{
    public class LightFactory
    {
        private readonly Dictionary<string, IBuildLight> devices = new Dictionary<string, IBuildLight>();

        public LightFactory()
        {
            devices.Add("console", new ConsoleBuildLight());
            devices.Add("delcom", new DelcomLight());
        }

        public string[] SupportedDevices
        {
            get { return devices.Keys.ToArray(); }
        }

        public IBuildLight CreateLight(string deviceName)
        {
            if (devices.ContainsKey(deviceName))
            {
                return devices[deviceName];
            }
            else
            {
                throw new NotSupportedException(deviceName + " is not a support build device");
            }
        }
    }
}