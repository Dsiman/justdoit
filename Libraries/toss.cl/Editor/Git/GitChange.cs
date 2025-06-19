using System.Linq;
using Sandbox;

namespace Changelog.Git;

public enum ChangeType
{
    [Icon( "add_circle" ), Tint( EditorTint.Green )]
    Untracked = '?',

    [Icon( "block" ), Tint( EditorTint.White )]
    Ignored = '!',

    [Icon( "edit" ), Tint( EditorTint.Yellow )]
    Modified = 'M',

    [Icon( "add_box" ), Tint( EditorTint.Green )]
    Added = 'A',

    [Icon( "delete" ), Tint( EditorTint.Red )]
    Deleted = 'D',

    [Icon( "drive_file_rename_outline" ), Tint( EditorTint.Pink )]
    Renamed = 'R',

    [Icon( "content_copy" ), Tint( EditorTint.Blue )]
    Copied = 'C',

    [Icon( "device_unknown" ), Tint( EditorTint.White )]
    Unknown = 'X'
}

public class GitChange
{
    public string File;
    public ChangeType Type = ChangeType.Unknown;

    public GitChange( string type, string file )
    {
        File = file;
        //Type = Enum.Parse<ChangeType>( (int)(char)type.Trim()[..1] );

        var typeChar = type.Trim().ToCharArray().FirstOrDefault();
        Type = (ChangeType)typeChar;
    }
}