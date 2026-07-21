using System.Collections.Concurrent;
using PdfSharp.Fonts;

namespace CareerPilot.Infrastructure.Resumes.Export;

/// <summary>
/// Supplies font data to PDFsharp by locating TrueType files on the host.
/// </summary>
/// <remarks>
/// <para>
/// PDFsharp 6 ships no default font resolver outside Windows: without one, the very
/// first <c>RenderDocument</c> call throws while building its own error font, so PDF
/// export fails entirely on macOS and in any Linux container. This class is what makes
/// PDF export work off Windows at all.
/// </para>
/// <para>
/// It maps the family names used by the template catalogue onto whatever is actually
/// installed, preferring metric-compatible substitutes — Liberation Sans for Arial,
/// Liberation Serif for Georgia — so a substituted font occupies roughly the same space
/// and the layout does not reflow.
/// </para>
/// <para>
/// <b>Deployment note.</b> A minimal container image has no fonts at all. Install a
/// package such as <c>fonts-liberation</c> or <c>fonts-dejavu</c> in the runtime image,
/// or bundle a licensed font and extend <see cref="FontDirectories"/>. If nothing is
/// found this resolver reports it and PDF export degrades to a clear error rather than
/// producing a blank document.
/// </para>
/// </remarks>
internal sealed class SystemFontResolver : IFontResolver
{
    /// <summary>Standard font locations across the platforms this may run on.</summary>
    private static readonly string[] FontDirectories =
    [
        "/usr/share/fonts",                 // Linux
        "/usr/local/share/fonts",           // Linux, local installs
        "/System/Library/Fonts",            // macOS
        "/Library/Fonts",                   // macOS
        "C:\\Windows\\Fonts",               // Windows
    ];

    /// <summary>
    /// Candidate file names per requested family, in preference order.
    /// </summary>
    /// <remarks>
    /// Metric-compatible substitutes come first so that a resume laid out for Arial
    /// keeps its line breaks when rendered with Liberation Sans.
    /// </remarks>
    private static readonly Dictionary<string, string[]> FamilyCandidates =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["arial"] = ["Arial", "LiberationSans", "DejaVuSans", "Helvetica"],
            ["helvetica"] = ["Helvetica", "LiberationSans", "Arial", "DejaVuSans"],
            ["calibri"] = ["Calibri", "Carlito", "LiberationSans", "DejaVuSans", "Arial"],
            ["verdana"] = ["Verdana", "DejaVuSans", "LiberationSans", "Arial"],
            ["georgia"] = ["Georgia", "LiberationSerif", "DejaVuSerif", "Times New Roman", "Times"],
            ["times new roman"] = ["Times New Roman", "LiberationSerif", "DejaVuSerif", "Times"],
        };

    private static readonly Lazy<IReadOnlyList<string>> AvailableFonts = new(DiscoverFonts);

    private static readonly ConcurrentDictionary<string, byte[]?> FontDataCache = new();

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // Style is folded into the face name so bold and italic resolve to their own
        // files where those exist; PDFsharp synthesises the style when they do not.
        var key = $"{familyName}|{(isBold ? "b" : string.Empty)}{(isItalic ? "i" : string.Empty)}";

        return GetFontData(key, familyName, isBold, isItalic) is null
            ? null
            : new FontResolverInfo(key);
    }

    public byte[]? GetFont(string faceName)
    {
        var parts = faceName.Split('|');
        var family = parts[0];
        var style = parts.Length > 1 ? parts[1] : string.Empty;

        return GetFontData(faceName, family, style.Contains('b'), style.Contains('i'));
    }

    private static byte[]? GetFontData(string cacheKey, string familyName, bool isBold, bool isItalic) =>
        FontDataCache.GetOrAdd(cacheKey, _ => LoadFont(familyName, isBold, isItalic));

    private static byte[]? LoadFont(string familyName, bool isBold, bool isItalic)
    {
        var candidates = FamilyCandidates.TryGetValue(familyName.Trim(), out var mapped)
            ? mapped
            // Unknown family: try it by name, then fall back to the two families most
            // likely to exist on any system that has fonts at all.
            : [familyName, "LiberationSans", "DejaVuSans", "Arial", "Helvetica"];

        foreach (var candidate in candidates)
        {
            var path = FindFontFile(candidate, isBold, isItalic);

            if (path is not null)
            {
                try
                {
                    return File.ReadAllBytes(path);
                }
                catch (IOException)
                {
                    // Unreadable file: keep looking rather than failing the render.
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the best file for a family and style.
    /// </summary>
    /// <remarks>
    /// Matching is on a normalised file name because font files are named
    /// inconsistently — <c>LiberationSans-Bold.ttf</c>, <c>ariblk.ttf</c>,
    /// <c>DejaVuSans-BoldOblique.ttf</c>. Stripping separators makes those comparable.
    /// </remarks>
    private static string? FindFontFile(string family, bool isBold, bool isItalic)
    {
        var normalizedFamily = Normalize(family);

        var matches = AvailableFonts.Value
            .Where(path => Normalize(Path.GetFileNameWithoutExtension(path)).StartsWith(normalizedFamily, StringComparison.Ordinal))
            .ToList();

        if (matches.Count == 0)
        {
            return null;
        }

        bool HasStyle(string path, string marker) =>
            Normalize(Path.GetFileNameWithoutExtension(path)).Contains(marker, StringComparison.Ordinal);

        // Exact style match first, then progressively looser, then anything from the
        // family — a regular face is a far better outcome than no font.
        return (isBold, isItalic) switch
        {
            (true, true) => matches.FirstOrDefault(p => HasStyle(p, "bold") && (HasStyle(p, "italic") || HasStyle(p, "oblique")))
                            ?? matches.FirstOrDefault(p => HasStyle(p, "bold"))
                            ?? Regular(matches),
            (true, false) => matches.FirstOrDefault(p => HasStyle(p, "bold") && !HasStyle(p, "italic") && !HasStyle(p, "oblique"))
                             ?? matches.FirstOrDefault(p => HasStyle(p, "bold"))
                             ?? Regular(matches),
            (false, true) => matches.FirstOrDefault(p => (HasStyle(p, "italic") || HasStyle(p, "oblique")) && !HasStyle(p, "bold"))
                             ?? Regular(matches),
            _ => Regular(matches),
        };

        static string? Regular(List<string> candidates) =>
            candidates.FirstOrDefault(p =>
            {
                var name = Normalize(Path.GetFileNameWithoutExtension(p));
                return !name.Contains("bold", StringComparison.Ordinal)
                       && !name.Contains("italic", StringComparison.Ordinal)
                       && !name.Contains("oblique", StringComparison.Ordinal)
                       && !name.Contains("light", StringComparison.Ordinal);
            }) ?? candidates.FirstOrDefault();
    }

    private static string Normalize(string value) =>
        value.Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

    /// <summary>
    /// Enumerates TrueType files once, on first use.
    /// </summary>
    /// <remarks>
    /// Only <c>.ttf</c> is collected. PDFsharp cannot read <c>.ttc</c> collections or
    /// macOS <c>.dfont</c>, and including them would produce resolver hits that fail
    /// later at render time instead of falling through to a usable substitute here.
    /// </remarks>
    private static IReadOnlyList<string> DiscoverFonts()
    {
        var fonts = new List<string>();

        foreach (var directory in FontDirectories.Where(Directory.Exists))
        {
            try
            {
                fonts.AddRange(Directory.EnumerateFiles(directory, "*.ttf", SearchOption.AllDirectories));
            }
            catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
            {
                // A directory that cannot be read is skipped; others may still yield a
                // usable font.
            }
        }

        return fonts;
    }

    /// <summary>
    /// True when at least one usable font was found. Checked at startup so a host with
    /// no fonts is reported plainly rather than failing on the first export.
    /// </summary>
    public static bool HasAnyFont => AvailableFonts.Value.Count > 0;

    public static int DiscoveredFontCount => AvailableFonts.Value.Count;
}
