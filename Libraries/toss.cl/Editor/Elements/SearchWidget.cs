using System;
using Editor;

namespace Changelog.Elements;

/// <summary>
/// Minimal replica of <see cref="Editor.SearchWidget" /> without asset-related code
/// </summary>
public sealed class SearchWidget : Widget
{
    public string Value
    {
        get => LineEdit.Value;
        set {
            LineEdit.Value = value;
            Update();
        }
    }
    
    public Action ValueChanged;

    private LineEdit LineEdit;
    private ToolButton ClearBtn;

    public SearchWidget( Widget parent, string placeholder = "Search" ) : base( parent )
    {
        Layout = Layout.Row();

        MinimumHeight = Theme.RowHeight;
        SetStyles( $"background-color: {Theme.ControlBackground.Hex}" );

        LineEdit = new LineEdit( this );
        LineEdit.PlaceholderText = $"\u2315  {placeholder}";
        LineEdit.TextChanged += TextChanged;

        ClearBtn = new ToolButton( "Clear", "clear", this );
        ClearBtn.MouseLeftPress = Clear;
        ClearBtn.Visible = false;

        Layout.Add( LineEdit, 1 );
        Layout.Add( ClearBtn );
    }

    private void Clear()
        => LineEdit.Text = string.Empty;

    private void TextChanged( string value )
    {
        ClearBtn.Visible = !string.IsNullOrEmpty( value );
        ValueChanged?.Invoke();
    }
}