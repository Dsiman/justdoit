using System.Collections.Generic;
using System.Linq;
using Editor;

namespace Changelog.Elements;

public abstract class ListView<T> : ListView where T : IAssetListEntry
{
    public IEnumerable<T> Entries
        => Items.OfType<T>();

    public IEnumerable<T> Selected
        => SelectedItems.OfType<T>();
    
    public ListView( Widget parent ) : base( parent )
    {
        ItemSize = new Vector2( 0, 24 );
        ItemPaint = PaintListItem;
        ItemSpacing = 0;
    }

    private void PaintListItem( VirtualWidget widget )
    {
        if ( widget.Object is not T item ) return;
        OnPaint( item, widget );
    }

    protected abstract void OnPaint( T item, VirtualWidget widget );
}