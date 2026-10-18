using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DroidDeck.Actions;
using DroidDeck.Actions.Providers;
using DroidDeck.Models;
using DroidDeck.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DroidDeck.Tests
{
    public class ScriptProviderTests
    {
        [Theory]
        [InlineData("", new string[0])]
        [InlineData("a b  c", new[] { "a", "b", "c" })]
        [InlineData("\"meu jogo\" 2", new[] { "meu jogo", "2" })]
        [InlineData("x \"\" y", new[] { "x", "", "y" })]
        public void SplitArguments_ComoALinhaDeComando(string text, string[] expected) =>
            Assert.Equal(expected, ScriptProvider.SplitArguments(text));

        [Fact]
        public void StartInfo_EscolheOInterpretadorPelaExtensao()
        {
            Assert.Equal("powershell.exe", ScriptProvider.StartInfo(@"C:\x\a.ps1", "1")!.FileName);
            Assert.Equal("cmd.exe", ScriptProvider.StartInfo(@"C:\x\a.bat", "")!.FileName);
            Assert.Contains("-File", ScriptProvider.StartInfo(@"C:\x\a.ps1", "")!.ArgumentList);
            Assert.Null(ScriptProvider.StartInfo(@"C:\x\a.exe", ""));
        }

        [Fact]
        public void Script_EPrivilegiado() =>
            Assert.True(ActionPolicy.IsPrivileged(new DeckAction { Type = "script" }));

        [Fact]
        [Trait("Category", "RequiresDesktop")]
        public async Task PowerShell_RecebeOsArgumentos()
        {
            var output = Path.Combine(Path.GetTempPath(), $"dd-script-{System.Guid.NewGuid():N}.txt");
            try
            {
                var action = new DeckAction
                {
                    Type = "script",
                    Parameters = new Dictionary<string, string>
                    {
                        ["source"] = "inline",
                        ["language"] = "powershell",
                        ["code"] = "Set-Content -Path $args[0] -Value \"$($args[1])-ção\" -Encoding UTF8",
                        ["arguments"] = $"\"{output}\" olá",
                    },
                };
                var provider = new ScriptProvider(NullLogger<ScriptProvider>.Instance);
                await provider.ExecuteAsync(new ActionContext(action, null, (_, _) => Task.CompletedTask));

                Assert.Equal("olá-ção", File.ReadAllText(output).Trim());
            }
            finally
            {
                File.Delete(output);
            }
        }
    }
}
