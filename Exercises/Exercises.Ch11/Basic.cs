// Chapter 11 — Modern C# in Practice — Capstone — BASIC
// ----------------------------------------------------------------
// Exercise: Pick one pattern from this chapter and apply it to a
//   small piece of code in your current project. Measure the
//   effect: line count before vs after, readability as reviewed by
//   a teammate, any test changes. The recommended starting point
//   is Immutable Data on a DTO class — it's the lowest-effort win.
// ----------------------------------------------------------------
//
// This exercise is intentionally open — the answer is whatever DTO
// in YOUR codebase benefits most. The example below shows the
// transformation on a realistic shape so you can see the diff and
// the line-count delta side-by-side.

namespace KeepUpCs.Exercises.Ch11;

// ── BEFORE — typical mutable DTO with manual Equals + ToString ──
public sealed class UserSettingsBefore
{
    public string ThemeName     { get; set; } = "system";
    public string Language      { get; set; } = "en-US";
    public bool   ShowAvatars   { get; set; } = true;
    public int    PageSize      { get; set; } = 50;
    public string TimeZone      { get; set; } = "UTC";

    public UserSettingsBefore() { }

    public UserSettingsBefore(string theme, string language, bool showAvatars, int pageSize, string timeZone)
    {
        ThemeName = theme; Language = language; ShowAvatars = showAvatars;
        PageSize = pageSize; TimeZone = timeZone;
    }

    public override bool Equals(object? obj) =>
        obj is UserSettingsBefore u &&
        ThemeName == u.ThemeName && Language == u.Language &&
        ShowAvatars == u.ShowAvatars && PageSize == u.PageSize &&
        TimeZone == u.TimeZone;

    public override int GetHashCode() =>
        HashCode.Combine(ThemeName, Language, ShowAvatars, PageSize, TimeZone);

    public override string ToString() =>
        $"UserSettings {{ Theme={ThemeName}, Lang={Language}, Avatars={ShowAvatars}, PageSize={PageSize}, TZ={TimeZone} }}";

    // ~31 effective lines (without comments)
}

// ── AFTER — Immutable Data Pattern: record + required + with-expr ─
public record UserSettingsAfter
{
    public string ThemeName   { get; init; } = "system";
    public string Language    { get; init; } = "en-US";
    public bool   ShowAvatars { get; init; } = true;
    public int    PageSize    { get; init; } = 50;
    public string TimeZone    { get; init; } = "UTC";
    // ~6 effective lines — Equals, GetHashCode, ToString, deconstruct, with-expr ALL generated
}

public static class BasicDemo
{
    public static void Run()
    {
        Console.WriteLine("Ch11 Basic — Immutable Data Pattern applied to a DTO");
        Console.WriteLine(new string('─', 60));

        var before1 = new UserSettingsBefore { ThemeName = "dark", Language = "fr-FR", PageSize = 25 };
        var before2 = new UserSettingsBefore { ThemeName = "dark", Language = "fr-FR", PageSize = 25 };

        var after1 = new UserSettingsAfter  { ThemeName = "dark", Language = "fr-FR", PageSize = 25 };
        var after2 = new UserSettingsAfter  { ThemeName = "dark", Language = "fr-FR", PageSize = 25 };

        Console.WriteLine($"  before1.Equals(before2)? {before1.Equals(before2)}");
        Console.WriteLine($"  after1.Equals(after2)?   {after1.Equals(after2)}");

        // Non-destructive mutation — only with the record version.
        var after3 = after1 with { PageSize = 100 };
        Console.WriteLine($"  after1.PageSize = {after1.PageSize}, after3.PageSize = {after3.PageSize}");
        Console.WriteLine($"  (after1 unchanged — value semantics)");

        Console.WriteLine();
        Console.WriteLine("  Line count: ~31 before  →  ~6 after");
        Console.WriteLine("  Tests: equality-based assertions just work — no more 'override Equals' bugs.");
    }
}
