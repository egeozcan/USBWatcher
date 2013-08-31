using System.Linq;
using System.Text;

namespace GetDevices
{
    using System;
    using System.Collections.Generic;
    using System.Management; // need to add System.Management to your project references.

    class Program
    {
        static void Main()
        {
            var list = GetUsbDevices();
            var list2 = GetUsbDevices();
            while (list.Count() == list2.Count() && !list2.Except(list).Any() && !list.Except(list2).Any())
            {
                Console.WriteLine("seems to be the same");
                list2 = GetUsbDevices();
                System.Threading.Thread.Sleep(1000);
                Console.WriteLine("got list...");
            }
            Console.WriteLine("DIFFERENCE!!!");
            Console.WriteLine("--------------------------");
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(GetDeviceListAsText(list));
            Console.ResetColor();
            Console.WriteLine("--------------------------");
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(GetDeviceListAsText(list2));
            Console.ResetColor();
            Console.WriteLine("--------------------------");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(GetDeviceListAsText(list2.Except(list)));
            Console.WriteLine(GetDeviceListAsText(list.Except(list2)));
            Console.ReadLine();
        }

        static string GetDeviceListAsText(IEnumerable<UsbDeviceInfo> usbDevices)
        {
            var list = new StringBuilder();
            foreach (var usbDevice in usbDevices)
            {
                list.AppendFormat("Device ID: {0}, PNP Device ID: {1}, Description: {2}\n",
                    usbDevice.DeviceId, usbDevice.PnpDeviceId, usbDevice.Description);
            }
            return list.ToString();
        }

        static List<UsbDeviceInfo> GetUsbDevices()
        {
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
                collection = searcher.Get();

            var devices = 
                from ManagementBaseObject device in collection 
                 select new UsbDeviceInfo(
                     (string) device.GetPropertyValue("DeviceID"),
                     (string) device.GetPropertyValue("PNPDeviceID"),
                     (string) device.GetPropertyValue("Description"));
            var deviceList = devices.ToList();
            collection.Dispose();
            return deviceList;
        }
    }

    class UsbDeviceInfo
    {
        protected bool Equals(UsbDeviceInfo other)
        {
            return string.Equals(DeviceId, other.DeviceId) && string.Equals(PnpDeviceId, other.PnpDeviceId) && string.Equals(Description, other.Description);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (DeviceId != null ? DeviceId.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (PnpDeviceId != null ? PnpDeviceId.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Description != null ? Description.GetHashCode() : 0);
                return hashCode;
            }
        }

        public UsbDeviceInfo(string deviceId, string pnpDeviceId, string description)
        {
            DeviceId = deviceId;
            PnpDeviceId = pnpDeviceId;
            Description = description;
        }
        public string DeviceId { get; private set; }
        public string PnpDeviceId { get; private set; }
        public string Description { get; private set; }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((UsbDeviceInfo) obj);
        }
    }
}
