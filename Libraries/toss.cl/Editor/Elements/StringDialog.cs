using System;
using Editor;
using Sandbox;

namespace Changelog.Elements;

public sealed class StringDialog : Dialog
{
    public Action<string> OnDone;
    
    private Label Label { get; }
    private TextEdit TextEdit { get; }
    private LineEdit LineEdit { get; }

    public StringDialog( string msg, Action<string> onDone, string confirmText = "Okay", string confirmIcon = "",
        string title = null, bool multiLine = false )
    {
        Body.Layout.Spacing = 0;
        
        Window.Size = new Vector2( 400, multiLine ? 150 : 100 );
        Window.SetModal( true, true );
        Window.SetWindowIcon( "question_mark" );
        Window.Title = title is null
            ? "Changelog"
            : $"{title} - Changelog";

        Label = new Label.Body( msg, Body );
        Label.SetSizeMode( SizeMode.Default, SizeMode.Expand );
        Body.Layout.Add( Label );

        OnDone = onDone;

        var confirm = AddFooterButton( confirmText, confirmIcon, Confirm );
        confirm.Tint = Theme.Primary;
        
        var bind = confirm.Bind( "Enabled" ).ReadOnly();
        
        if ( multiLine ) {
            TextEdit = new TextEdit( this );
            Body.Layout.Add( TextEdit, 1 );
            bind.From( () => !string.IsNullOrWhiteSpace( TextEdit.PlainText ), null );
        }
        else {
            LineEdit = new LineEdit( this );
            Body.Layout.Add( LineEdit, 1 );
            bind.From( () => !string.IsNullOrWhiteSpace( LineEdit.Value ), null );
        }
        
        AddCancelButton();
    }

    public override void Show()
    {
        base.Show();
        
        if ( LineEdit.IsValid() ) {
            LineEdit.Focus();
            LineEdit.SelectAll();
        } else if ( TextEdit.IsValid() ) {
            TextEdit.Focus();
            TextEdit.SelectAll();
        }
    }

    private void Confirm()
    {
        OnDone?.Invoke( LineEdit?.Value ?? TextEdit?.PlainText );

        Close();
    }
}