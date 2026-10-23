using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;
using DroidDeck.Lib;

namespace DroidDeck
{
    /// <summary>
    /// Janela invisível que serve apenas de host para o ícone de bandeja (NotifyIcon).
    /// O DroidDeck roda como app de bandeja: o servidor web sobe em paralelo (ver Program.cs)
    /// e o usuário interage pelo menu do ícone. Não há janela visível.
    /// </summary>
    public partial class frmPrincipal : Form
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppRunName = "DroidDeck";
        private const string WebUrl = "http://localhost:4787/";

        private ToolStripMenuItem _autoStartItem = null!;
        private ToolStripMenuItem _firewallItem = null!;
        private ToolStripMenuItem _usbItem = null!;
        private bool _usbHooked;
        private ToolStripMenuItem _updateItem = null!;

        public frmPrincipal()
        {
            InitializeComponent();

            // Nunca aparece na barra de tarefas nem como janela; só o ícone de bandeja.
            ShowInTaskbar = false;
            ntiTray.Icon = AppIcon.Small;
            Icon = AppIcon.Large;
            BuildTrayMenu();
            ntiTray.Visible = true;
        }

        // A janela nunca é exibida, mas o handle precisa existir para o message loop
        // (e o ícone de bandeja) continuarem vivos. Sem CreateHandle, o Application.Run
        // encerraria de imediato por não haver janela principal realizada.
        protected override void SetVisibleCore(bool value)
        {
            if (!IsHandleCreated) CreateHandle();
            base.SetVisibleCore(false);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ntiTray.ShowBalloonTip(3000, "DroidDeck", L.Ui("Rodando na bandeja — servidor em {url}", ("url", WebUrl)), ToolTipIcon.Info);

            if (Program.FirstRunAfterInstall)
                BeginInvoke(FirstRunSetup);

            Program.Updater.UpdateReady += version => BeginInvoke(() => OnUpdateReady(version));
            Program.Updater.StartBackgroundChecks(_updateCts.Token);
        }

        private readonly System.Threading.CancellationTokenSource _updateCts = new();

        /// <summary>
        /// Logo depois de instalar: liga o "Iniciar com o Windows" (o deck precisa do PC
        /// ligado e o usuario quase sempre quer isso) e oferece liberar o firewall.
        /// </summary>
        private void FirstRunSetup()
        {
            SetAutoStart(true);
            _autoStartItem.Checked = true;

            if (Lib.FirewallSetup.RulesExist()) return;
            var answer = MessageBox.Show(
                L.Ui("Para o celular encontrar e conectar no DroidDeck, o Firewall do Windows precisa liberar as portas {tcp} (TCP) e {udp} (UDP), só para a sua rede local.\n\nLiberar agora? O Windows vai pedir permissão de administrador.",
                    ("tcp", Lib.PairingInfo.Port), ("udp", Lib.FirewallSetup.DiscoveryPort)),
                "DroidDeck", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == DialogResult.Yes) AllowFirewall();
        }

        private void AllowFirewall()
        {
            if (Lib.FirewallSetup.AddRules())
                ntiTray.ShowBalloonTip(3000, "DroidDeck", L.Ui("Firewall liberado para a rede local."), ToolTipIcon.Info);
            else
                ntiTray.ShowBalloonTip(5000, "DroidDeck",
                    L.Ui("O firewall não foi alterado. Você pode tentar de novo pelo menu da bandeja."), ToolTipIcon.Warning);
            _firewallItem.Checked = Lib.FirewallSetup.RulesExist();
        }

        /// <summary>Liga/desliga a conexao pelo cabo USB (baixa o adb do Google se faltar).</summary>
        private async void ToggleUsb()
        {
            if (Lib.UsbBridge.Enabled)
            {
                Lib.UsbBridge.Enabled = false;
                UpdateUsbItem();
                return;
            }
            if (Lib.UsbBridge.FindAdb() == null)
            {
                var answer = MessageBox.Show(
                    L.Ui("Para conectar pelo cabo, o DroidDeck usa o adb, a ferramenta oficial do Google para falar com celulares Android (uns 7 MB).\n\nBaixar agora do site do Google?\n\nNo celular, ligue a \"Depuração USB\" nas opções do desenvolvedor."),
                    L.Ui("DroidDeck — Conexão USB"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer != DialogResult.Yes) return;
                _usbItem.Enabled = false;
                _usbItem.Text = L.Ui("Baixando o adb…");
                try
                {
                    await Lib.UsbBridge.DownloadAdbAsync();
                }
                catch (Exception ex)
                {
                    Lib.Log.Error($"Falha ao baixar o adb: {ex.Message}");
                    ntiTray.ShowBalloonTip(5000, "DroidDeck", L.Ui("Não foi possível baixar o adb. Tente de novo mais tarde."), ToolTipIcon.Warning);
                    _usbItem.Enabled = true;
                    UpdateUsbItem();
                    return;
                }
                _usbItem.Enabled = true;
            }
            Lib.UsbBridge.Enabled = true;
            UpdateUsbItem();
            ntiTray.ShowBalloonTip(5000, "DroidDeck",
                L.Ui("Conexão USB ligada. Ligue a depuração USB no celular e aceite o aviso que aparecer nele."), ToolTipIcon.Info);
        }

        private void UpdateUsbItem()
        {
            if (InvokeRequired) { BeginInvoke(UpdateUsbItem); return; }
            var count = Lib.UsbBridge.Instance?.Devices.Count ?? 0;
            _usbItem.Checked = Lib.UsbBridge.Enabled;
            _usbItem.Text = !Lib.UsbBridge.Enabled ? L.Ui("Conexão USB (cabo)")
                : count == 0 ? L.Ui("Conexão USB (cabo): aguardando celular")
                : count == 1 ? L.Ui("Conexão USB (cabo): 1 celular conectado")
                : L.Ui("Conexão USB (cabo): {n} celulares conectados", ("n", count));
        }

        private void OnUpdateReady(string version)
        {
            _updateItem.Text = L.Ui("Reiniciar para atualizar (v{version})", ("version", version));
            ntiTray.ShowBalloonTip(5000, "DroidDeck",
                L.Ui("A versão {version} foi baixada e entra na próxima vez que o DroidDeck abrir. Para atualizar agora, use o menu da bandeja.", ("version", version)), ToolTipIcon.Info);
        }

        private async void OnUpdateClick()
        {
            var updater = Program.Updater;
            if (!updater.IsInstalled)
            {
                OpenUrl(Lib.AppUpdater.RepoUrl + "/releases/latest");
                return;
            }
            if (updater.PendingVersion != null)
            {
                updater.ApplyAndRestart();
                return;
            }

            _updateItem.Enabled = false;
            _updateItem.Text = L.Ui("Procurando atualização…");
            var version = await updater.CheckAndDownloadAsync();
            _updateItem.Enabled = true;
            if (version == null)
            {
                _updateItem.Text = L.Ui("Procurar atualização");
                ntiTray.ShowBalloonTip(3000, "DroidDeck",
                    L.Ui("Você está na versão mais recente ({version}).", ("version", updater.CurrentVersion)), ToolTipIcon.Info);
            }
        }

        private void BuildTrayMenu()
        {
            var menu = new ContextMenuStrip();

            var openItem = new ToolStripMenuItem(L.Ui("Abrir no navegador"));
            openItem.Click += (_, _) => OpenWebUi();

            var pairItem = new ToolStripMenuItem(L.Ui("Parear dispositivo (QR)…"));
            pairItem.Click += (_, _) =>
            {
                using var f = new frmPairing();
                f.ShowDialog();
            };

            _autoStartItem = new ToolStripMenuItem(L.Ui("Iniciar com o Windows"))
            {
                CheckOnClick = true,
                Checked = IsAutoStartEnabled(),
            };
            _autoStartItem.Click += (_, _) => SetAutoStart(_autoStartItem.Checked);

            _firewallItem = new ToolStripMenuItem(L.Ui("Liberar no firewall (rede local)"))
            {
                Checked = Lib.FirewallSetup.RulesExist(),
            };
            _firewallItem.Click += (_, _) => AllowFirewall();

            _usbItem = new ToolStripMenuItem(L.Ui("Conexão USB (cabo)"));
            _usbItem.Click += (_, _) => ToggleUsb();
            // O servico sobe junto com o servidor web, as vezes depois da bandeja.
            menu.Opening += (_, _) =>
            {
                if (Lib.UsbBridge.Instance is { } usb && !_usbHooked)
                {
                    usb.Changed += UpdateUsbItem;
                    _usbHooked = true;
                }
                UpdateUsbItem();
            };

            // Copia instalada: procura e aplica atualizacoes. Zip/dev: abre a pagina da release.
            var version = Program.Updater.CurrentVersion;
            var versionItem = new ToolStripMenuItem(version != null ? $"DroidDeck v{version}" : L.Ui("DroidDeck (sem instalador)"))
            {
                Enabled = false,
            };
            _updateItem = new ToolStripMenuItem(Program.Updater.IsInstalled ? L.Ui("Procurar atualização") : L.Ui("Baixar versão mais recente…"));
            _updateItem.Click += (_, _) => OnUpdateClick();

            var exitItem = new ToolStripMenuItem(L.Ui("Encerrar"));
            exitItem.Click += (_, _) => Application.Exit();

            menu.Items.Add(openItem);
            menu.Items.Add(pairItem);
            menu.Items.Add(_autoStartItem);
            menu.Items.Add(_firewallItem);
            menu.Items.Add(_usbItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(versionItem);
            menu.Items.Add(_updateItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitItem);

            ntiTray.ContextMenuStrip = menu;
            ntiTray.DoubleClick += (_, _) => OpenWebUi();
        }

        private static void OpenWebUi() => OpenUrl(WebUrl);

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Lib.Log.Error($"Falha ao abrir a Web UI: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Remove o ícone para não deixar "fantasma" na bandeja.
            ntiTray.Visible = false;
            _updateCts.Cancel();
            base.OnFormClosing(e);
        }

        private static bool IsAutoStartEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
            return key?.GetValue(AppRunName) != null;
        }

        internal static void SetAutoStart(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey, true)
                    ?? Registry.CurrentUser.CreateSubKey(RunKey);
                if (key == null) return;

                if (enable)
                    key.SetValue(AppRunName, $"\"{Application.ExecutablePath}\"");
                else
                    key.DeleteValue(AppRunName, throwOnMissingValue: false);
            }
            catch (Exception ex)
            {
                Lib.Log.Error($"Falha ao configurar autostart: {ex.Message}");
            }
        }

        /// <summary>
        /// Desinstalacao: tira o autostart so se ele aponta para esta copia. Outra copia do
        /// DroidDeck (um .zip antigo, um build de dev) continua iniciando com o Windows.
        /// </summary>
        internal static void RemoveAutoStartIfOurs()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
                var current = key?.GetValue(AppRunName) as string;
                if (current != null &&
                    current.Trim('"').Equals(Application.ExecutablePath, StringComparison.OrdinalIgnoreCase))
                    key!.DeleteValue(AppRunName, throwOnMissingValue: false);
            }
            catch (Exception ex)
            {
                Lib.Log.Error($"Falha ao remover autostart: {ex.Message}");
            }
        }
    }
}
