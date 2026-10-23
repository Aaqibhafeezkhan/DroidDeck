using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
                    if (rel.StartsWith("obj/") || rel.StartsWith("bin/")) continue;
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
                source.AddDartLiterals(Path.Combine(dir.FullName, "app", "lib"));
                return source;
            }

            private const string DartLiteral = @"(?:r?'(?:[^'\\\n]|\\.)*'|r?""(?:[^""\\\n]|\\.)*"")";
            private static readonly Regex DartSequence = new("(?:" + DartLiteral + @"\s*)+");
            private static readonly Regex DartSingle = new(DartLiteral);
            private static readonly Regex DartEscape = new(@"\\(.)");
            private static readonly Regex DartTr = new(@"\btr\(\s*(" + DartLiteral + ")");

            /// <summary>
            /// O mesmo arquivo de idioma traduz o app: string do Dart (sozinha ou varias juntas,
            /// 'a' 'b') tambem conta como uso.
            /// </summary>
            private void AddDartLiterals(string root)
            {
                static string Value(string token)
                {
                    var raw = token.StartsWith('r');
                    var body = (raw ? token[1..] : token)[1..^1];
                    return raw ? body : DartEscape.Replace(body, m => m.Groups[1].Value switch { "n" => "\n", "t" => "\t", var c => c });
                }
                foreach (var file in Directory.EnumerateFiles(root, "*.dart", SearchOption.AllDirectories))
                {
                    var code = File.ReadAllText(file);
                    foreach (Match seq in DartSequence.Matches(code))
                    {
                        var parts = DartSingle.Matches(seq.Value).Select(m => Value(m.Value)).ToList();
                        foreach (var part in parts) Literals.Add(part);
                        if (parts.Count > 1) Literals.Add(string.Concat(parts));
                    }
                    // tr('...') dentro de ${...} de outra string: a passada acima pega a de fora.
                    foreach (Match call in DartTr.Matches(code)) Literals.Add(Value(call.Groups[1].Value));
                }
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
                "Sem tradução em app/assets/i18n/en.json ou es.json:\n" + string.Join("\n", missing));
        }

        [Fact]
        public void NenhumaTraducao_SobrouDeTextoQueNaoExisteMais()
        {
            // Texto mudou no codigo e a chave antiga ficou para tras.
            var orphans = L.StringsOf("en").Keys.Where(k => !Code.Value.Literals.Contains(k)).OrderBy(x => x).ToList();
            Assert.True(orphans.Count == 0,
                "Tradução de um texto que não aparece mais no código (o texto em português mudou?):\n" + string.Join("\n", orphans));
        }
    }
}
