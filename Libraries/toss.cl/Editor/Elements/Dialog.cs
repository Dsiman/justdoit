using System;
using Editor;

namespace Changelog.Elements;

public abstract class Dialog : Editor.Dialog
{
    public Action OnCancel;
    
    protected Widget Body;
    protected Widget Footer;
    
    public Dialog()
    {
        Layout = Layout.Column();
        Layout.Margin = 8;
        Layout.Spacing = 4;

        SetModal( true, true );
        Window.SetWindowIcon( "paragliding" );

        MinimumSize = new Vector2( 200, 100 );

        Body = new Widget( this );
        Body.Layout = Layout.Column();
        Body.Layout.Spacing = 1;
        
        Footer = new Widget( this );
        Footer.Layout = Layout.Row();
        Footer.Layout.Spacing = 8;

        Layout.Add( Body, 1 );
        Layout.Add( Footer );
    }

    protected Button AddFooterButton( string text, string icon = null, Action onClick = null )
    {
        var btn = new Button( text, icon, Footer );
        btn.Clicked = onClick;
        
        Footer.Layout.Add( btn );
        return btn;
    }

    protected Button AddCancelButton()
        => AddFooterButton( "Cancel", "close", Cancel );
    
    // ESC to close popup... oh look, it doesn't work!
    [Shortcut( "editor.clear-selection", "ESC" )]
    private void Cancel()
    {
        OnCancel?.Invoke();
        Close();
    }
}