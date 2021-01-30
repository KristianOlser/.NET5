using System;
using System.Management;
using System.Runtime.InteropServices;

namespace SSDLife
{
    class Program
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct Attribute
        {
            public byte AttributeID;
            public ushort Flags;
            public byte Value;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] VendorData;
        }

        static void Main(string[] args)
        {
            try
            {
                Attribute AtributeInfo;
                ManagementScope Scope = new ManagementScope(String.Format("\\\\{0}\\root\\WMI", "localhost"), null);
                Scope.Connect();
                ObjectQuery Query = new ObjectQuery("SELECT VendorSpecific FROM MSStorageDriver_ATAPISmartData");
                ManagementObjectSearcher Searcher = new ManagementObjectSearcher(Scope, Query);
                byte TotalBytesWritten = 0xF6;
                int Delta  = 12;
                foreach (ManagementObject WmiObject in Searcher.Get())
                {
                    byte[] VendorSpecific = (byte[])WmiObject["VendorSpecific"];
                    for (int offset = 2; offset < VendorSpecific.Length; )
                    {
                        if (VendorSpecific[offset] == TotalBytesWritten)
                        {

                            IntPtr buffer = IntPtr.Zero;
                            try
                            {
                                buffer = Marshal.AllocHGlobal(Delta);
                                Marshal.Copy(VendorSpecific, offset, buffer, Delta);
                                AtributeInfo = (Attribute)Marshal.PtrToStructure(buffer, typeof(Attribute));
                                Array.Reverse(AtributeInfo.VendorData);
                                string HexVendorData = BitConverter.ToString(AtributeInfo.VendorData).Replace("-", string.Empty);
                                Console.WriteLine("Cumulative host sectors written: {0} SEKTORY - HEX ", HexVendorData);
                                long DecimalVendorData = Convert.ToInt64(HexVendorData, 16);
                                Console.WriteLine("Cumulative host sectors written: {0} SEKTORY - DECIMAL ", DecimalVendorData);
                                long ByteVendorData = DecimalVendorData * 512;
                                Console.WriteLine("Cumulative host sectors written: {0} SEKTORY - BYTE ", ByteVendorData);
                                var GBVendorData = Math.Round(ByteVendorData / Math.Pow(1024,3),0);
                                Console.WriteLine("Cumulative host sectors written: {0} SEKTORY - GIGABYTE ", GBVendorData);


                            }
                            finally
                            {

                                if (buffer != IntPtr.Zero)
                                {
                                    Marshal.FreeHGlobal(buffer);
                                }
                            }                                                
                        }
                        offset += Delta;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Exception {0} Trace {1}",e.Message,e.StackTrace));
            }
            Console.WriteLine("Press Enter to exit");
            Console.Read();
        }
    }
}
