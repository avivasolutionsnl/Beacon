using System.Text;
using System.Threading;

namespace Beacon.Lights.Delcom
{
    internal class DelcomLed
    {
        public void SetLed(byte led, bool turnItOn, bool flashIt)
        {
            SetLed(led, turnItOn, flashIt, null, false);
        }

        public void SetLed(byte led, bool turnItOn, bool flashIt, int? flashDurationInSeconds)
        {
            SetLed(led, turnItOn, flashIt, flashDurationInSeconds, false);
        }

        public void SetLed(byte led, bool turnItOn, bool flashIt, int? flashDurationInSeconds, bool turnOffAfterFlashing)
        {
            uint deviceHandle = GetDelcomDeviceHandle(); // open the device
            if (deviceHandle == 0) return;

            if (turnItOn)
            {
                if (flashIt)
                {
                    DelcomApis.DelcomLEDControl(deviceHandle, led, DelcomApis.LEDFLASH);
                    if (flashDurationInSeconds.HasValue)
                    {
                        Thread.Sleep(flashDurationInSeconds.Value * 1000);
                        var ledStatus = turnOffAfterFlashing ? DelcomApis.LEDOFF : DelcomApis.LEDON;
                        DelcomApis.DelcomLEDControl(deviceHandle, led, ledStatus);
                    }
                }
                else
                {
                    DelcomApis.DelcomLEDControl(deviceHandle, led, DelcomApis.LEDON);
                }
            }
            else
            {
                DelcomApis.DelcomLEDControl(deviceHandle, led, DelcomApis.LEDOFF);
            }

            DelcomApis.DelcomCloseDevice(deviceHandle);
        }

        private readonly StringBuilder deviceName = new StringBuilder(DelcomApis.MAXDEVICENAMELEN);

        private uint GetDelcomDeviceHandle()
        {
            if (string.IsNullOrEmpty(deviceName.ToString()))
            {
                // Search for the first match USB device, For USB IO Chips use USBIODS
                DelcomApis.DelcomGetNthDevice(DelcomApis.USBDELVI, 0, deviceName);
            }

            var hUsb = DelcomApis.DelcomOpenDevice(deviceName, 0); // open the device
            return hUsb;
        }
    }
}