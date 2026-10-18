using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DroidDeck.Auth;
using DroidDeck.Lib;

namespace DroidDeck
{
    /// <summary>
    /// Janela de pareamento: exibe um QR com ip+porta+chave para o app escanear, ou o
    /// endereco do deck no navegador (iPhone, tablet) para a camera abrir.
    /// A chave de API só fica visível quando o usuário abre esta janela (pelo menu da bandeja).
    /// </summary>
    public class frmPairing : Form
    {
        private readonly Label _info;
        private readonly PictureBox _pic;
        private readonly RadioButton _appMode;
        private readonly LinkLabel _url;

        public frmPairing()
        {
            Text = L.Ui("DroidDeck — Parear dispositivo");
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(360, 528);

            var ip = NetworkInfo.GetLanIp();
            var key = ApiKeyProvider.GetKey();

            _appMode = new RadioButton
            {
                Text = L.Ui("App Android"),
                Location = new Point(40, 8),
                Size = new Size(130, 24),
                Checked = true,
            };
            var browserMode = new RadioButton
            {
                Text = L.Ui("Abrir no navegador"),
                Location = new Point(180, 8),
                Size = new Size(150, 24),
            };

            _info = new Label
            {
                Location = new Point(10, 36),
                Size = new Size(340, 44),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            _pic = new PictureBox
            {
                Location = new Point(20, 82),
                Size = new Size(320, 320),
                SizeMode = PictureBoxSizeMode.Zoom,
            };

            FormClosed += (_, _) => SetQr(null);

            _url = new LinkLabel
            {
                Location = new Point(10, 406),
                Size = new Size(340, 20),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            _url.LinkClicked += (_, _) =>
            {
                try { Clipboard.SetText(PairingInfo.BuildBrowserUrl()); _url.Text = L.Ui("Endereço copiado"); }
                catch { }
            };

            var details = new TextBox
            {
                Text = L.Ui("IP: {ip}    Chave: {key}", ("ip", $"{ip}:{PairingInfo.Port}"), ("key", key)),
                Location = new Point(10, 432),
                Size = new Size(340, 44),
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = SystemColors.Control,
                TextAlign = HorizontalAlignment.Center,
            };

            var close = new Button
            {
                Text = L.Ui("Fechar"),
                Location = new Point(140, 488),
                Size = new Size(80, 30),
            };
            close.Click += (_, _) => Close();
            AcceptButton = close;

            _appMode.CheckedChanged += (_, _) => ShowMode();

            Controls.Add(_appMode);
            Controls.Add(browserMode);
            Controls.Add(_info);
            Controls.Add(_pic);
            Controls.Add(_url);
            Controls.Add(details);
            Controls.Add(close);
            ShowMode();
        }

        private void ShowMode()
        {
            if (_appMode.Checked)
            {
                _info.Text = L.Ui("No app DroidDeck, vá em Configurar → Parear (QR)\ne escaneie este código para conectar com segurança.");
                _url.Visible = false;
                SetQr(PairingInfo.BuildUri());
            }
            else
            {
                _info.Text = L.Ui("Aponte a câmera do iPhone, iPad ou tablet para este código:\no deck abre no navegador, sem instalar nada.");
                _url.Text = L.Ui("Copiar o endereço (para abrir em outro PC)");
                _url.Visible = true;
                SetQr(PairingInfo.BuildBrowserUrl());
            }
        }

        private void SetQr(string? content)
        {
            // O PictureBox não descarta a própria Image: sem isto vaza um HBITMAP por troca.
            var old = _pic.Image;
            _pic.Image = null;
            try { old?.Dispose(); } catch { }
            if (content == null) return;
            try
            {
                // Um Bitmap construído a partir de um stream exige que o stream continue vivo
                // pelo resto da vida da imagem — daí a cópia independente, que permite fechar
                // o MemoryStream aqui mesmo.
                using var ms = new MemoryStream(PairingInfo.BuildQrPng(content));
                using var qr = new Bitmap(ms);
                _pic.Image = new Bitmap(qr);
            }
            catch (Exception ex)
            {
                Log.Error($"Falha ao gerar QR de pareamento: {ex.Message}");
            }
        }
    }
}
