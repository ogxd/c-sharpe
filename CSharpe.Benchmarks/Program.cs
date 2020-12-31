using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using System;
using System.Linq;

namespace CSharpe
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            Stopwatch sw = Stopwatch.StartNew();
            MIB_TCPTABLE table = WindowsInterop.GetTcpTable();
            sw.Stop();
            Console.WriteLine($"done in {sw.ElapsedMilliseconds} ms");

            Thread.Sleep(3000);

            sw.Restart();
            table = WindowsInterop.GetTcpTable();
            sw.Stop();
            Console.WriteLine($"done in {sw.ElapsedMilliseconds} ms");

            Console.WriteLine("rows = " + table.rows.Length);
            */

            AppDomain.MonitoringIsEnabled = true;

            ManualConfig conf = new ManualConfig();
            conf.AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
            conf.AddLogger(DefaultConfig.Instance.GetLoggers().ToArray());

            conf.AddColumnProvider(DefaultColumnProviders.Metrics);
            conf.AddColumnProvider(DefaultColumnProviders.Job);
            conf.AddColumnProvider(DefaultColumnProviders.Descriptor);
            conf.AddColumnProvider(DefaultColumnProviders.Statistics);

            conf.AddDiagnoser(MemoryDiagnoser.Default);

            var switcher = new BenchmarkSwitcher(new[] {
                typeof(AutoMarshalBenchmark),
            });

            switcher.Run(args, config: conf);

            Console.ReadKey();
        }
    }
}