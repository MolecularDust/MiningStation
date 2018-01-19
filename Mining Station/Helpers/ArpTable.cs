using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    public class ArpTable
    {
        // Define the MIB_IPNETROW structure.
        [StructLayout(LayoutKind.Sequential)]
        struct MIB_IPNETROW
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwIndex;
            [MarshalAs(UnmanagedType.U4)]
            public int dwPhysAddrLen;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac0;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac1;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac2;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac3;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac4;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac5;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac6;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac7;
            [MarshalAs(UnmanagedType.U4)]
            public int dwAddr;
            [MarshalAs(UnmanagedType.U4)]
            public int dwType;
        }

        // Declare the GetIpNetTable function.
        [DllImport("IpHlpApi.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int GetIpNetTable(
           IntPtr pIpNetTable,
           [MarshalAs(UnmanagedType.U4)]
           ref int pdwSize,
           bool bOrder);

        [DllImport("IpHlpApi.dll", SetLastError = true,
                                   CharSet = CharSet.Auto)]
        internal static extern int FreeMibTable(IntPtr plpNetTable);

        // The insufficient buffer error.
        const int ERROR_INSUFFICIENT_BUFFER = 122;

        public static List<(string IpAddress, string Mac)> GetArpTable()
        {
            // The number of bytes needed.
            int bytesNeeded = 0;

            // The result from the API call.
            int result = GetIpNetTable(IntPtr.Zero, ref bytesNeeded, false);

            if (result != ERROR_INSUFFICIENT_BUFFER)
            {
                // Throw an exception.
                throw new Win32Exception(result);
            }

            IntPtr buffer = IntPtr.Zero;

            try
            {
                // Allocate the memory, do it in a try/finally block, to ensure
                // that it is released.
                buffer = Marshal.AllocCoTaskMem(bytesNeeded);

                // Make the call again. If it did not succeed, then
                // raise an error.
                result = GetIpNetTable(buffer, ref bytesNeeded, false);

                // If the result is not 0 (no error), then throw an exception.
                if (result != 0)
                {
                    // Throw an exception.
                    throw new Win32Exception(result);
                }

                // Now we have the buffer, we have to marshal it. We can read
                // the first 4 bytes to get the length of the buffer.
                int entries = Marshal.ReadInt32(buffer);

                // Increment the memory pointer by the size of the int.
                IntPtr currentBuffer = new IntPtr(buffer.ToInt64() +
                   Marshal.SizeOf(typeof(int)));

                // Allocate an array of entries.
                MIB_IPNETROW[] table = new MIB_IPNETROW[entries];

                // Cycle through the entries.
                for (int index = 0; index < entries; index++)
                {
                    // Call PtrToStructure, getting the structure information.
                    table[index] = (MIB_IPNETROW)Marshal.PtrToStructure(new
                     IntPtr(currentBuffer.ToInt64() + (index *
                     Marshal.SizeOf(typeof(MIB_IPNETROW)))), typeof(MIB_IPNETROW));
                }

                var list = new List<(string IpAddress, string Mac)>();

                for (int index = 0; index < entries; index++)
                {
                    MIB_IPNETROW row = table[index];

                    //Obtain IP address
                    System.Net.IPAddress ip =
                     new System.Net.IPAddress(BitConverter.GetBytes(table[index].dwAddr));

                    //Build MAC
                    string mac = row.mac0.ToString("X2") + '-' + row.mac1.ToString("X2") + '-' +
                        row.mac2.ToString("X2") + '-' + row.mac3.ToString("X2") + '-' +
                        row.mac4.ToString("X2") + '-' + row.mac5.ToString("X2");

                    //Add return value
                    list.Add((ip.ToString(), mac));
                }
                return list;
            }
            finally
            {
                // Release the memory.
                FreeMibTable(buffer);
            }
        }

        public static List<(string IpAddress, string Mac)> GetIpAddresses()
        {
            List<(string IpAddress, string Mac)> ipList = null;
            try
            {
                ipList = ArpTable.GetArpTable();
            }
            catch (Exception)
            {
                return null;
            }
            return ipList;
        }

        public static async Task<List<string>> GetHostNames(List<(string IpAddress, string Mac)> ipList, bool removeDomain = true)
        {
            var hostNames = new List<string>();
            var tasks = new List<Task>();

            foreach (var entry in ipList)
            {
                // Filter out non-canonic LAN addresses
                var first = entry.IpAddress.Split('.').FirstOrDefault();
                if (first == null || (first != "192" && first != "172" && first != "169" && first != "10"))
                {
                    continue;
                }

                Func<Task> function = async () =>
                {
                    try
                    {
                        Task<IPHostEntry> resolve = Dns.GetHostEntryAsync(entry.IpAddress);
                        await Task.WhenAny(resolve, Task.Delay(2500)).ConfigureAwait(false);
                        if (resolve.IsCompleted)
                        {
                            var hostEntry = resolve.Result;
                            string hostName = null;
                            if (removeDomain && hostEntry.HostName.Contains('.'))
                                hostName = hostEntry.HostName.Split('.')[0];
                            else hostName = hostEntry.HostName;
                            hostNames.Add(hostName);
                        }
                    }
                    catch (Exception)
                    {
                    }
                };

                tasks.Add(function());
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            hostNames = hostNames.Distinct().ToList();
            hostNames.Sort();
            return hostNames;
        }
    }
}
