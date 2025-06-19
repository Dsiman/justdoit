using System;
using Editor;
using Sandbox;

namespace Changelog.States.NewCommit;

public sealed class FileListHeader : Widget
{
    public Label Title { get; }
    
    public FileListHeader( string title, Widget parent ) : base( parent )
    {
        Layout = Layout.Row();
        Layout.Spacing = 2;
        Layout.Alignment = TextFlag.CenterVertically;

        Title = new Label( title, this );
        Layout.Add( Title, 1 );
    }

    public IconButton AddButton( string icon, string tooltip, Action onClick )
    {
        var btn = new IconButton( icon, onClick, this );
        btn.IconSize = 16;
        btn.ToolTip = tooltip;

        Layout.Add( btn );
        return btn;
    }
}