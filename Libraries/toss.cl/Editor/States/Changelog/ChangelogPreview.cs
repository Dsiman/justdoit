using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Editor;
using Sandbox;
using Sandbox.Diagnostics;

namespace Changelog.States.Changelog;

public sealed class ChangelogPreview : Widget
{
	private TextEdit TextEdit;
	private Button CopyBtn;
	private Button PublishBtn;
	
	private readonly Logger Log = new( "Changelog" );
	
	public ChangelogPreview( Widget parent = null ) : base( parent )
	{
		Layout = Layout.Column();

		TextEdit = new TextEdit( this )
		{
			PlaceholderText = "No changes yet",
			ReadOnly = true,
		};

		Layout.Add( TextEdit, 3 );

		var buttons = new Widget( this );
		buttons.Layout = Layout.Row();

		CopyBtn = new Button( "Copy", "copy_all", buttons )
		{
			Clicked = Copy,
			MouseRightPress = CopyMenu,
			ToolTip = "Copies the above changelog\n(right click: copy as format)",
			Tint = Theme.Primary,
		};

		PublishBtn = new Button( "Publish", "publish", buttons )
		{
			Clicked = Publish,
			ToolTip = "Copies the changelog, opens the publish window and marks all non-hidden commits as published",
			Tint = Theme.Primary,
		};
		
		buttons.Layout.Add( CopyBtn );
		buttons.Layout.Add( PublishBtn );

		Layout.Add( buttons );
		
		UpdatePreview();
	}

	private void Copy() => Copy( true );
	private async void Copy( bool show )
	{
		EditorUtility.Clipboard.Copy( TextEdit.PlainText );
		
		if ( show ) {
			CopyBtn.Icon = "done";
			CopyBtn.Tint = Theme.Green.Darken( 0.5f );
		}

		await Task.Delay( 300 );
		if ( !show ) return;
		
		CopyBtn.Icon = "copy_all";
		CopyBtn.Tint = Theme.Primary;
	}

	private async void Publish()
	{
		Copy( false );

		if ( TryPublish() ) {
			GetAncestor<ChangelogWidget>()?.CommitListContainer.Toolbar.MarkLatest( null );
			PublishBtn.Icon = "done";
			PublishBtn.Tint = Theme.Green.Darken( 0.5f );
		}
		else {
			PublishBtn.Icon = "error_outline";
			PublishBtn.Tint = Theme.Red.Darken( 0.5f );
		}
		await Task.Delay( 300 );
		PublishBtn.Icon = "publish";
		PublishBtn.Tint = Theme.Primary;
	}

	private bool TryPublish()
	{
		if ( string.IsNullOrEmpty( TextEdit.PlainText ) ) {
			Log.Error( "No changes to publish" );
			return false;
		}
		
		var project = Project.Current;
		if ( project is null ) {
			Log.Error( "Current project is null, copied to clipboard only" );
			return false;
		}
		
		if ( project.Config.IsWhitelistDisabled ) {
			Log.Error( "Enable whitelist in project settings to publish" );
			return false;
		}
		
		if ( project.Package.Org.Ident == "local" ) {
			Log.Error( "Select an organisation in project settings" );
			_ = ProjectSettingsWindow.OpenForProject( project );
			return false;
		}
		
		_ = ProjectSettingsWindow.OpenForProject( project, "upload" );
		return true;
	}

	public void UpdatePreview( CommitListContainer list = null )
	{
		list ??= GetAncestor<ChangelogWidget>()?.CommitListContainer;

		TextEdit.PlainText = GeneratePreview( list );
	}

	private string GeneratePreview( CommitListContainer list )
	{
		var output = new StringBuilder();
		
		foreach (var commit in GetChanges( list )) {
			output.AppendLine( $" - {commit}" );
		}

		return output.ToString();
	}

	private IEnumerable<string> GetChanges( CommitListContainer list = null )
	{
		list ??= GetAncestor<ChangelogWidget>()?.CommitListContainer;
		
		return list?.Commits
			.Where( x => !( x.IsHidden || x.IsPublished ) )
			.Select( x => x.Name );
	}

	private void CopyMenu()
	{
		var menu = new ContextMenu();
		menu.AddHeading( "Copy As ..." );
		var text = menu.AddOption( "Text / Markdown", "description", Copy );
		menu.AddOption( "HTML Bullets", "code", CopyHtml);
		menu.AddOption( "JS-style Array", "data_array", CopyArray );
		menu.OpenAtCursor( true );
	}

	private void CopyHtml()
	{
		var output = new StringBuilder( "<ul>\n" );

		foreach ( var change in GetChanges() ) {
			output.AppendLine( $"<li>{HttpUtility.HtmlEncode( change )}</li>" );
		}

		output.Append( "</ul>" );
		
		Copy( true );
		EditorUtility.Clipboard.Copy( output.ToString() );
	}

	private void CopyArray()
	{
		var output = new StringBuilder( "[\n" );

		foreach ( var change in GetChanges() ) {
			output.AppendLine( $"\"{HttpUtility.JavaScriptStringEncode( change )}\"," );
		}

		output.Append( "]" );
		
		Copy( true );
		EditorUtility.Clipboard.Copy( output.ToString() );
	}
}
