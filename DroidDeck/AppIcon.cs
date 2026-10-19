using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace DroidDeck
{
    /// <summary>
    /// O ícone do DroidDeck (grade 3×3) no tamanho certo para cada lugar. O .ico tem
    /// versões desenhadas no pixel para 16–32 px; pedir o tamanho exato evita que o
    /// Windows reduza a de 32 px e borre a grade na bandeja.
    /// </summary>
    internal static class AppIcon
    {
        private static Icon Load(Size size)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DroidDeck.droiddeck.ico")!;
            return new Icon(stream, size);
        }

        /// <summary>Bandeja e barra de título (16 px a 100%, maior com escala do Windows).</summary>
        public static Icon Small => Load(SystemInformation.SmallIconSize);

        /// <summary>Alt+Tab e janelas grandes.</summary>
        public static Icon Large => Load(SystemInformation.IconSize);
    }
}
