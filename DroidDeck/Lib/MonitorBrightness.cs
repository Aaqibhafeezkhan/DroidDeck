using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace DroidDeck.Lib
{
    /// <summary>
    /// Brilho dos monitores, de 0 a 100. Monitor externo: DDC/CI (o mesmo canal dos botões
    /// do próprio monitor; alguns vêm com DDC/CI desligado no menu OSD). Tela de notebook:
    /// WMI. Mexe em todos os monitores juntos; ler devolve a média.
    /// </summary>
    public static class MonitorBrightness
    {
        /// <summary>Brilho atual (média dos monitores que respondem), ou null se nenhum responde.</summary>
        public static int? Get()
        {
            var values = new List<int>();
            ForEachDdc((h, min, cur, max) =>
            {
                if (max > min) values.Add((int)Math.Round((cur - min) * 100.0 / (max - min)));
                return false;
            });
            var wmi = GetWmi();
            if (wmi.HasValue) values.Add(wmi.Value);
            return values.Count == 0 ? null : (int)Math.Round(values.Average());
        }

        /// <summary>Muda o brilho de todos os monitores. False se nenhum aceitou.</summary>
        public static bool Set(int percent)
        {
            percent = Math.Clamp(percent, 0, 100);
            var any = false;
            ForEachDdc((h, min, cur, max) =>
            {
                var target = (uint)Math.Round(min + (max - min) * percent / 100.0);
                if (SetMonitorBrightness(h, target)) any = true;
                return true;
            });
            if (SetWmi(percent)) any = true;
            return any;
        }

        // ---- DDC/CI (dxva2) ----

        private delegate bool MonitorAction(IntPtr physical, uint min, uint cur, uint max);

        private static void ForEachDdc(MonitorAction action)
        {
            var handles = new List<IntPtr>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMon, IntPtr hdc, ref Rect r, IntPtr data) =>
            {
                handles.Add(hMon);
                return true;
            }, IntPtr.Zero);

            foreach (var hMon in handles)
            {
                if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMon, out var count) || count == 0) continue;
                var physical = new PhysicalMonitor[count];
                if (!GetPhysicalMonitorsFromHMONITOR(hMon, count, physical)) continue;
                try
                {
                    foreach (var pm in physical)
                    {
                        // Ler antes também serve de teste: monitor sem DDC/CI falha aqui.
                        if (GetMonitorBrightness(pm.Handle, out var min, out var cur, out var max))
                            action(pm.Handle, min, cur, max);
                    }
                }
                finally
                {
                    DestroyPhysicalMonitors(count, physical);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect { public int Left, Top, Right, Bottom; }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct PhysicalMonitor
        {
            public IntPtr Handle;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Description;
        }

        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdc, ref Rect rect, IntPtr data);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr clip, MonitorEnumProc proc, IntPtr data);

        [DllImport("dxva2.dll", SetLastError = true)]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint count);

        [DllImport("dxva2.dll", SetLastError = true)]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint count, [Out] PhysicalMonitor[] monitors);

        [DllImport("dxva2.dll", SetLastError = true)]
        private static extern bool DestroyPhysicalMonitors(uint count, PhysicalMonitor[] monitors);

        [DllImport("dxva2.dll", SetLastError = true)]
        private static extern bool GetMonitorBrightness(IntPtr monitor, out uint min, out uint current, out uint max);

        [DllImport("dxva2.dll", SetLastError = true)]
        private static extern bool SetMonitorBrightness(IntPtr monitor, uint brightness);

        // ---- WMI (tela interna do notebook) ----

        private static int? GetWmi()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT CurrentBrightness FROM WmiMonitorBrightness");
                foreach (ManagementObject o in searcher.Get())
                    using (o) return Convert.ToInt32(o["CurrentBrightness"]);
            }
            catch { }
            return null;
        }

        private static bool SetWmi(int percent)
        {
            try
            {
                var ok = false;
                using var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM WmiMonitorBrightnessMethods");
                foreach (ManagementObject o in searcher.Get())
                {
                    using (o) o.InvokeMethod("WmiSetBrightness", new object[] { 1u, (byte)percent });
                    ok = true;
                }
                return ok;
            }
            catch
            {
                return false;
            }
        }
    }
}
