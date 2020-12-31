using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using CSharpe.Marshalling;
using System;
using System.Runtime.InteropServices;

namespace CSharpe
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
    [MemoryDiagnoser]
    [SimpleJob(/*launchCount: 1, warmupCount: 2, targetCount: 5*/)]
    public class AutoMarshalBenchmark
    {
        [GlobalSetup]
        public void Setup()
        {

        }

		//https://docs.microsoft.com/en-us/windows/win32/api/iphlpapi/nf-iphlpapi-gettcptable
		// We can also use v2 if we want to get by process :
		//https://docs.microsoft.com/en-us/windows/win32/api/iphlpapi/nf-iphlpapi-gettcptable2
		[DllImport("iphlpapi.dll", SetLastError = false, ExactSpelling = true)]
		private static extern int GetTcpTable(IntPtr TcpTable, ref int SizePointer, [MarshalAs(UnmanagedType.Bool)] bool Order);

		private IntPtr _buffTable;

		[IterationSetup]
		public unsafe void IterationSetup()
		{
			int bufferSize = 0;
			int x = GetTcpTable(IntPtr.Zero, ref bufferSize, false);
			IntPtr buffTable = Marshal.AllocHGlobal(bufferSize);
			if (GetTcpTable(buffTable, ref bufferSize, false) != 0)
				throw new Exception("Count not get Tcp Table");

			_buffTable = buffTable;
		}

		[Benchmark]
		public unsafe MIB_TCPTABLE GetTcpTable_HardCoded()
		{
			MIB_TCPTABLE table = new MIB_TCPTABLE();

			// Num Entries
			uint* src = (uint*)_buffTable.ToPointer();
			table.dwNumEntries = src[0];
			// Increment
			*src++ = 1;
			// Rows
			table.rows = new MIB_TCPROW[table.dwNumEntries];
			MIB_TCPROW* rowsPtr = (MIB_TCPROW*)src;
			for (int i = 0; i < table.dwNumEntries; i++)
				table.rows[i] = rowsPtr[i];

			return table;
		}

		[Benchmark]
		public unsafe MIB_TCPTABLE GetTcpTable_AutoMarshal1()
		{
			var table = AutoMarshal1.Convert<MIB_TCPTABLE>(_buffTable);

			return table;
		}

		[Benchmark]
		public unsafe MIB_TCPTABLE GetTcpTable_AutoMarshal2()
		{
			var table = AutoMarshal2<MIB_TCPTABLE>.Convert(_buffTable);

			return table;
		}
	}

	[AutoMarshal2(typeof(MarshallerObj<MIB_TCPTABLE>))]
	public struct MIB_TCPTABLE
	{
		[AutoMarshal1ArraySize]
		[AutoMarshal2(typeof(MarshallerBlittableSize<MIB_TCPTABLE>))]
		public uint dwNumEntries;

		[AutoMarshal1Array("dwNumEntries")]
		[AutoMarshal2(typeof(MarshallerBlittableArray<MIB_TCPTABLE, MIB_TCPROW>))]
		public MIB_TCPROW[] rows;
	}

	public struct MIB_TCPROW
	{
		public uint dwState;
		public uint dwLocalAddr;
		public uint dwLocalPort;
		public uint dwRemoteAddr;
		public uint dwRemotePort;
	}
}