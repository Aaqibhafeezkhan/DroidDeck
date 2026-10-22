using System;
using System.IO;
using System.Linq;
using DroidDeck.Services;
using Xunit;

namespace DroidDeck.Tests
{
    public class DiskStatsTests
    {
        [Fact]
        public void Discos_TrazAUnidadeDoSistemaComEspacoCoerente()
        {
            var disks = SystemMonitorService.SampleDisks();
            var system = Path.GetPathRoot(Environment.SystemDirectory)!.TrimEnd('\\');
            var c = disks.Single(d => string.Equals(d.Name, system, StringComparison.OrdinalIgnoreCase));
            Assert.True(c.TotalBytes > 0);
            Assert.InRange(c.FreeBytes, 0, c.TotalBytes);
            // Em ordem de letra e sem a barra final ("C:", nao "C:\").
            Assert.Equal(disks.Select(d => d.Name).OrderBy(n => n, StringComparer.OrdinalIgnoreCase), disks.Select(d => d.Name));
            Assert.DoesNotContain(disks, d => d.Name.EndsWith("\\"));
        }
    }
}
