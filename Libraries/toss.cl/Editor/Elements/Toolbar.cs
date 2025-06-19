using System;
using Editor;
using Sandbox;

namespace Changelog.Elements;

public class Toolbar : Widget
{
    public SearchWidget Search { get; private set; }

    public Toolbar( Widget parent ) : base( parent )
    {
        Layout = Layout.Row();
        Layout.Margin = 2;
        Layout.Spacing = 2;
        
        MinimumHeight = Theme.RowHeight;
    }

    public void Add( Widget w, int stretch = 0 )
        => Layout.Add( w, stretch );

    public Button AddButton( string text, string icon )
        => AddButton( text, icon, Theme.Primary );
    
    public Button AddButton( string text, string icon, Color tint )
    {
        var btn = new Button( text, icon );
        
        // every masterpiece (file class AddButton) has its cheap copy
        btn.Tint = tint;

        Add( btn );
        return btn;
    }

    public SearchWidget AddSearch( string placeholder = null )
    {
        if ( Search.IsValid() )
            return Search;
        
        Search = new SearchWidget( this, placeholder );
        
        Add( Search, 1 );
        return Search;
    }

    public ToolButton AddIcon( string icon, string tooltip, Action onClick = null, Action onRightClick = null )
    {
        var btn = new ToolButton( tooltip, icon, this )
        {
            //IconSize = 20f
            MouseClick = onClick,
            MouseRightClick = onRightClick,
        };
        
        Add( btn );
        return btn;
    }

    public ToolButton AddRefresh( Action onClick )
        => AddIcon( "refresh", "Refresh\n(right click: reset library)", onClick, ResetLib );

    private void ResetLib()
        => GetAncestor<DockWidget>()?.Reset();
}