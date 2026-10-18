using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace DroidDeck.Integrations
{
    /// <summary>O que o provedor usa do Voicemeeter (a DLL de verdade ou um falso nos testes).</summary>
    public interface IVoicemeeterApi
    {
        /// <summary>Le um parametro numerico (ex.: "Strip[0].Mute"). Lanca se o Voicemeeter nao estiver aberto.</summary>
        float Get(string parameter);

        void Set(string parameter, float value);

        /// <summary>Muda um botao do Macro Buttons (1 = apertado, 0 = solto).</summary>
        void SetMacroButton(int button, float value);
    }

    /// <summary>
    /// Voicemeeter Remote API (VoicemeeterRemote64.dll), carregada da pasta onde o Voicemeeter
    /// foi instalado -- ela nao vem com o DroidDeck. O login e feito uma vez e vale ate o app
    /// fechar (logar de novo sem logout a API recusa).
    /// </summary>
    public sealed class VoicemeeterRemote : IVoicemeeterApi, IDisposable
    {
        private const string UninstallKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\VB:Voicemeeter {17359A74-1236-5467}";

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int NoArgs();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetTypeFn(out int type);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate int GetFloatFn([MarshalAs(UnmanagedType.LPStr)] string name, out float value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate int SetFloatFn([MarshalAs(UnmanagedType.LPStr)] string name, float value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int MacroSetFn(int button, float value, int bitmode);

        private readonly object _lock = new();
        private IntPtr _dll;
        private bool _loggedIn;
        private NoArgs? _logout;
        private NoArgs? _isDirty;
        private GetTypeFn? _getType;
        private GetFloatFn? _getFloat;
        private SetFloatFn? _setFloat;
        private MacroSetFn? _macroSet;

        public float Get(string parameter)
        {
            lock (_lock)
            {
                EnsureReady();
                // A API so atualiza os valores lidos depois de perguntar se mudou algo.
                _isDirty!();
                if (_getFloat!(parameter, out var value) != 0)
                    throw new InvalidOperationException("Parâmetro do Voicemeeter desconhecido.");
                return value;
            }
        }

        public void Set(string parameter, float value)
        {
            lock (_lock)
            {
                EnsureReady();
                if (_setFloat!(parameter, value) != 0)
                    throw new InvalidOperationException("Parâmetro do Voicemeeter desconhecido.");
            }
        }

        public void SetMacroButton(int button, float value)
        {
            lock (_lock)
            {
                EnsureReady();
                // bitmode 0: aperta/solta o botao como um clique (roda os comandos dele).
                _macroSet!(button, value, 0);
            }
        }

        private void EnsureReady()
        {
            if (!_loggedIn)
            {
                Load();
                var login = Fn<NoArgs>("VBVMR_Login")();
                if (login < 0) throw new InvalidOperationException("Não foi possível falar com o Voicemeeter.");
                _loggedIn = true;
            }
            // Logado mas o Voicemeeter fechado: a consulta do tipo falha.
            if (_getType!(out _) != 0) throw new InvalidOperationException("O Voicemeeter não está aberto.");
        }

        private void Load()
        {
            if (_dll != IntPtr.Zero) return;
            var path = FindDll() ?? throw new InvalidOperationException("Voicemeeter não está instalado neste PC.");
            if (!NativeLibrary.TryLoad(path, out _dll))
                throw new InvalidOperationException("Voicemeeter não está instalado neste PC.");
            _logout = Fn<NoArgs>("VBVMR_Logout");
            _isDirty = Fn<NoArgs>("VBVMR_IsParametersDirty");
            _getType = Fn<GetTypeFn>("VBVMR_GetVoicemeeterType");
            _getFloat = Fn<GetFloatFn>("VBVMR_GetParameterFloat");
            _setFloat = Fn<SetFloatFn>("VBVMR_SetParameterFloat");
            _macroSet = Fn<MacroSetFn>("VBVMR_MacroButton_SetStatus");
        }

        private T Fn<T>(string name) where T : Delegate =>
            Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(_dll, name));

        /// <summary>Caminho da DLL: pasta do desinstalador no registro, senao a pasta padrao.</summary>
        public static string? FindDll()
        {
            var file = Environment.Is64BitProcess ? "VoicemeeterRemote64.dll" : "VoicemeeterRemote.dll";
            string? fromRegistry = null;
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(UninstallKey);
                fromRegistry = DirFromUninstallString(key?.GetValue("UninstallString") as string);
            }
            catch { }
            foreach (var dir in new[]
            {
                fromRegistry,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "VB", "Voicemeeter"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "VB", "Voicemeeter"),
            })
            {
                if (string.IsNullOrEmpty(dir)) continue;
                var candidate = Path.Combine(dir, file);
                if (File.Exists(candidate)) return candidate;
            }
            return null;
        }

        /// <summary>Pasta do executavel citado no UninstallString (com ou sem aspas e argumentos).</summary>
        public static string? DirFromUninstallString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var s = value.Trim();
            string exe;
            if (s.StartsWith('"'))
            {
                var end = s.IndexOf('"', 1);
                exe = end > 0 ? s[1..end] : s.Trim('"');
            }
            else
            {
                var dotExe = s.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
                exe = dotExe > 0 ? s[..(dotExe + 4)] : s;
            }
            try { return Path.GetDirectoryName(exe); }
            catch { return null; }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_loggedIn) { try { _logout?.Invoke(); } catch { } }
                _loggedIn = false;
            }
        }
    }
}
