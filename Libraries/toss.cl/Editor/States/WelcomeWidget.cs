using System.Threading.Tasks;
using Changelog.Git;
using Editor;
using Sandbox;

namespace Changelog.States;

public sealed class WelcomeWidget : Widget
{
	private Button DownloadButton = new( "I don't have Git", "download" )
	{
		Tint = Theme.Primary,
		ToolTip = "Opens the download page for Git"
	};

	private Button InitButton = new( "I have Git", "flag" )
	{
		Tint = Theme.Primary,
		ToolTip = "Initialises a Git repository in your project folder"
	};
	
	public WelcomeWidget( Widget parent ) : base( parent )
	{
		Layout = Layout.Column();
		Layout.Alignment = TextFlag.Center;
		Layout.Spacing = 8;
		
		var header = new Label( "Welcome to Changelog!" );
		header.SetStyles( "font-weight: 600; font-size: 23px;" );

		var subtitle = new Label( "This library needs a Git repository to work." );
		subtitle.SetStyles( "font-size: 15px;" );

		var buttons = Layout.Row();
		buttons.Spacing = 6;

		DownloadButton.Clicked = DownloadGit;
		InitButton.Clicked = InitRepo;
		
		buttons.Add( DownloadButton );
		buttons.Add( InitButton );

		var footer =
			new Label(
				"This library interacts with the Git program on your computer" +
				"\nIf you don't like that, please remove this library now." +
				"\nGit isn't related to this library (or even s&box) at all.     \ud83d\ude07" );
		
		footer.SetStyles( "padding-top: 10px;" );

		Layout.Add( header );
		Layout.Add( subtitle );
		Layout.Add( buttons );
		Layout.Add( footer );
	}

	private void DownloadGit()
		=> EditorUtility.OpenFolder( "https://git-scm.com/downloads" );

	// initialise repo if it doesn't exist, or use current repo if it does
	private async void InitRepo()
	{
		InitButton.Enabled = false;
		InitButton.TransparentForMouseEvents = false;
		InitButton.Icon = "pending";
		
		// check if `git status` errors out
		if ( !GitRepo.Exists )
		{
			// run `git init`
			Log.Info( "Initialising git repo..." );
			GitRepo.RunCommand( "init" );
		}

		InitButton.Tint = Theme.Green.Darken( 0.5f );
		InitButton.Icon = "done";

		await Task.Delay( 250 );
		
		ProjectCookie.Set( "changelog.enabled", true );

		GetAncestor<DockWidget>()?.Refresh();
	}
}
