using System.Collections.Generic;

namespace DroidDeck.Models
{
    public class SystemStats
    {
        public float CpuUsage { get; set; }
        public float RamUsage { get; set; }
        public float RamTotal { get; set; }
        public float RamAvailable { get; set; }
        public float NetUpKBps { get; set; }
        public float NetDownKBps { get; set; }
        public float GpuUsage { get; set; }

        /// <summary>Unidades de disco fixas do PC (atualiza a cada 30 s).</summary>
        public List<DiskStat> Disks { get; set; } = new();
    }

    public class DiskStat
    {
        /// <summary>Letra da unidade: "C:".</summary>
        public string Name { get; set; } = "";

        /// <summary>Nome do volume no Windows ("Dados"); vazio quando não tem.</summary>
        public string Label { get; set; } = "";

        public long TotalBytes { get; set; }
        public long FreeBytes { get; set; }
    }
}
