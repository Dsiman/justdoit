using System.Collections.Generic;
using System.Linq;
using Editor;
using Sandbox;

namespace Changelog.States.NewCommit;

public sealed class FileList : ScrollArea
{
    public IEnumerable<FileEntry> Items
        => Canvas.Children.OfType<FileEntry>();

    public IEnumerable<FileEntry> Selected
        => Items.Where( fe => fe.IsStaged );

    public int SelectedCount
        => Items.Count( fe => fe.IsStaged );
    
    public FileList( Widget parent ) : base( parent )
    {
        Canvas = new Widget( this );
        Canvas.HorizontalSizeMode = SizeMode.Flexible;
        Canvas.Layout = Layout.Column();
        Canvas.Layout.Alignment = TextFlag.LeftTop;
        Canvas.Layout.Spacing = 2;
    }

    public void SelectAll()
    {
        foreach ( var file in Items ) {
            file.IsStaged = true;
        }
    }

    public void InvertSelect()
    {
        foreach ( var file in Items ) {
            file.IsStaged = !file.IsStaged;
        }
    }

    public void AddEntry( FileEntry entry )
    {
        entry.Parent = Canvas;
        Canvas.Layout.Add( entry );
    }

    public void Clear()
        => Canvas.Layout.Clear( true );
}