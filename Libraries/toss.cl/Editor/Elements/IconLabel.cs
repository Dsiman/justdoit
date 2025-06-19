using Editor;

namespace Changelog.Elements;

public sealed class IconLabel : IconButton
{
    public IconLabel( string icon, Widget parent ) : base( icon, null, parent )
    {
        Background = Color.Transparent;
        TransparentForMouseEvents = true;
    }
}