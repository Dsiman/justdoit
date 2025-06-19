using Changelog.States;
using Changelog.States.Changelog;
using Editor;
using Sandbox;

namespace Changelog;

[Dock( "Editor", "Changelog", "paragliding" )]
public sealed class DockWidget : Widget
{
	public DockWidget( Widget parent ) : base( parent )
	{
		MinimumSize = new Vector2( 200, 100 );
		
		Refresh();
	}

	[EditorEvent.Hotload]
	public void Refresh()
	{
		Widget layout = ProjectCookie.Get( "changelog.enabled", false )
			? new ChangelogWidget( this )
			: new WelcomeWidget( this );

		if ( Layout.IsValid() )
			Layout.Clear( true );
		else
			Layout = Layout.Row();
		
		Layout.Add( layout );
	}

	public void Reset()
	{
		ProjectCookie.Remove( "changelog.enabled" );
		Refresh();
	}
}
