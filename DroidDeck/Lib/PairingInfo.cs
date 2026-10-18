using DroidDeck.Auth;
using QRCoder;

namespace DroidDeck.Lib
{
    /// <summary>
    /// Monta os dados de pareamento (URI + QR) compartilhados pela janela de bandeja
    /// e pelo modo de diagnóstico --print-pairing.
    /// </summary>
    public static class PairingInfo
    {
        public const int Port = 4787;

        /// <summary>URI escaneada pelo app: droiddeck://pair?ip=..&port=..&key=..</summary>
        public static string BuildUri()
        {
            var ip = NetworkInfo.GetLanIp();
            var key = ApiKeyProvider.GetKey();
            return $"droiddeck://pair?ip={ip}&port={Port}&key={key}";
        }

        /// <summary>
        /// Endereco do deck no navegador (iPhone, tablet, outro PC), ja com a chave: a camera
        /// abre direto e a pagina guarda a chave e a tira da barra de endereco.
        /// </summary>
        public static string BuildBrowserUrl()
        {
            var ip = NetworkInfo.GetLanIp();
            var key = ApiKeyProvider.GetKey();
            return $"http://{ip}:{Port}/?pair={System.Uri.EscapeDataString(key)}";
        }

        public static byte[] BuildQrPng(string uri, int pixelsPerModule = 10)
        {
            var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
            return new PngByteQRCode(data).GetGraphic(pixelsPerModule);
        }
    }
}
