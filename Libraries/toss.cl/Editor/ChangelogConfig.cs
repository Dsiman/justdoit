using System.Collections.Generic;
using System.Text.Json.Serialization;
using Editor;
using Sandbox;

namespace Changelog;

/// <summary>
/// Library settings that need to be shared within the project (between users).
/// </summary>
public sealed class ChangelogConfig : ConfigData
{
    /// <summary>
    /// List of the commit hashes that are marked as published.
    /// These will be excluded from generated changelogs.
    /// First hash that git returns will be marked as the latest.
    /// </summary>
    public List<string> Published { get; set; } = [];

    /// <summary>
    /// List of the commit hashes marked as 'hidden'.
    /// These will be excluded from generated changelogs.
    /// </summary>
    public List<string> Hidden { get; set; } = [];

    // Config utilities
    [JsonIgnore] private const string Filename = "Changelog.config";

    [JsonIgnore]
    public static ChangelogConfig Project
    {
        // luckily this returns an empty object if there is no config, how convenient!
        get => EditorUtility.LoadProjectSettings<ChangelogConfig>( Filename );
        set => EditorUtility.SaveProjectSettings( value, Filename );
    }
}