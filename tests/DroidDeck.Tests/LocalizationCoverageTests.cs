using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DroidDeck.Lib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace DroidDeck.Tests
{
    /// <summary>
    /// Le o codigo do backend (Roslyn) e confere as traducoes. A chave e o proprio texto em
    /// portugues, entao mudar uma virgula no codigo sem mudar a chave faz o texto voltar em
    /// portugues sem aviso -- estes testes sao o aviso.
    /// </summary>
    public class LocalizationCoverageTests
    {
        private static readonly Lazy<Source> Code = new(Source.Load);

        private sealed class Source
        {
            /// <summary>Texto (literal) passado a L.T, L.Ui, L.TOrNull, new L.Text ou new LocalizedException.</summary>
            public readonly Dictionary<string, string> Translated = new();

            /// <summary>Toda string literal do backend (fora dos arquivos de traducao).</summary>
            public readonly HashSet<string> Literals = new(StringComparer.Ordinal);

            private static readonly HashSet<string> Calls = new() { "L.T", "L.Ui", "L.TOrNull" };
            private static readonly HashSet<string> Creations = new() { "L.Text", "LocalizedException" };

            public static Source Load()
            {
                var dir = new DirectoryInfo(AppContext.BaseDirectory);
                while (dir != null && !File.Exists(Path.Combine(dir.FullName, "DroidDeck", "DroidDeck.csproj")))
                    dir = dir.Parent;
                Assert.NotNull(dir);
                var root = Path.Combine(dir!.FullName, "DroidDeck");

                var source = new Source();
                foreach (var file in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
                {
                    var rel = Path.GetRelativePath(root, file).Replace('\\', '/');
                    if (rel.StartsWith("obj/") || rel.StartsWith("bin/") || rel.StartsWith("Lib/Localization")) continue;
                    var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
                    var nodes = tree.GetRoot().DescendantNodes().ToList();
                    foreach (var lit in nodes.OfType<LiteralExpressionSyntax>().Where(l => l.IsKind(SyntaxKind.StringLiteralExpression)))
                        source.Literals.Add(lit.Token.ValueText);
                    // "a" + "b": o texto que vale e o junto.
                    foreach (var concat in nodes.OfType<BinaryExpressionSyntax>())
                        if (Constant(concat) is { } joined) source.Literals.Add(joined);

                    foreach (var node in nodes)
                    {
                        ArgumentListSyntax? args = node switch
                        {
                            InvocationExpressionSyntax inv when Calls.Contains(inv.Expression.ToString()) => inv.ArgumentList,
                            ObjectCreationExpressionSyntax c when Creations.Contains(c.Type.ToString()) => c.ArgumentList,
                            _ => null,
                        };
                        if (args?.Arguments.FirstOrDefault()?.Expression is { } first && Constant(first) is { } text)
                        {
                            var line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                            source.Translated.TryAdd(text, $"{rel}:{line}");
                        }
                    }
                }
                return source;
            }

            /// <summary>Texto fixo: uma string literal ou literais somados com +.</summary>
            private static string? Constant(ExpressionSyntax e) => e switch
            {
                LiteralExpressionSyntax l when l.IsKind(SyntaxKind.StringLiteralExpression) => l.Token.ValueText,
                ParenthesizedExpressionSyntax p => Constant(p.Expression),
                BinaryExpressionSyntax b when b.IsKind(SyntaxKind.AddExpression)
                    && Constant(b.Left) is { } left && Constant(b.Right) is { } right => left + right,
                _ => null,
            };
        }

        [Fact]
        public void TodoTextoTraduzivel_TemTraducao()
        {
            Assert.NotEmpty(Code.Value.Translated);
            var missing = Code.Value.Translated
                .Where(kv => !L.Has(kv.Key, "en") || !L.Has(kv.Key, "es"))
                .Select(kv => $"{kv.Value}  \"{kv.Key}\"")
                .OrderBy(x => x)
                .ToList();
            Assert.True(missing.Count == 0,
                "Sem tradução em Localization.en.cs/.es.cs (rode add_l10n ou acrescente à mão):\n" + string.Join("\n", missing));
        }

        [Fact]
        public void NenhumaTraducao_SobrouDeTextoQueNaoExisteMais()
        {
            // Texto mudou no codigo e a chave antiga ficou para tras.
            var orphans = L.TranslatedKeys.Where(k => !Code.Value.Literals.Contains(k)).OrderBy(x => x).ToList();
            Assert.True(orphans.Count == 0,
                "Tradução de um texto que não aparece mais no código (o texto em português mudou?):\n" + string.Join("\n", orphans));
        }
    }
}
