using System;
using System.Collections.Generic;
using System.Linq;
using Changelog.Git;
using Editor;
using Sandbox;

namespace Changelog.States.Changelog;

public sealed class CommitListContainer : Widget
{
	public List<CommitEntry> Commits { get; } = [];
	public CommitList List { get; }
	public CommitListToolbar Toolbar { get; }
	
	private Label Label { get; set; }

	public CommitListContainer( Widget parent = null ) : base( parent )
	{
		Layout = Layout.Column();

		Toolbar = new CommitListToolbar( this );
		Toolbar.Search.ValueChanged = SearchChanged;

		List = new CommitList( this );

		Layout.Add( Toolbar );
		Layout.AddSeparator();
		Layout.Add( List, 1 );

		Reset();
	}

	private void SearchChanged()
	{
		List.Clear();

		var query = Toolbar.Search.Value;
		if ( string.IsNullOrWhiteSpace( query ) ) {
			List.AddItems( Commits );
			return;
		}

		List.AddItems(
			Commits.Where( c =>
				c.Name.Contains( query, StringComparison.OrdinalIgnoreCase ) ||
				c.Author.Contains( query, StringComparison.OrdinalIgnoreCase ) ) );
	}

	public void Reset()
	{
		var data = GitRepo.Commits;
		// fatal: your current branch 'master' does not have any commits yet
		if ( !data.Any() || data[0].StartsWith( "fatal:" ) ) {
			Label ??= Layout.Add( new Label( "You have no commits yet.", this )
			{
				Margin = 8,
				Alignment = TextFlag.Center,
			} );
			Label.SetStyles( "font-size: 16px;" );
			
			return;
		}
		
		var cfg = ChangelogConfig.Project;

		Label?.Destroy();
		Label = null;
		
		Commits.Clear();
		
		var published = cfg.Published;
		var foundLatest = false;
		foreach ( var line in GitRepo.Commits ) {
			var commit = line.Split( '|' );
			if ( commit.Length < 3 ) {
				var err = "Git returned invalid data while fetching components. " +
				          "Git might not be installed or your folder might not be a Git repo";

				Layout.Add( new Label( err, this ) { Color = Theme.Red } );
				Log.Error( err );

				GetAncestor<DockWidget>()?.Reset();
				return;
			}

			var hash = commit[0];
			var relDate = commit[1];
			var author = commit[2];
			
			// join rest of array up (ensure full commit message just in case you actually use |)
			// commit.Skip https://stackoverflow.com/a/27965285
			// .Aggregate https://stackoverflow.com/a/3575073
			var msg = commit.Skip( 3 ).Aggregate( ( a, b ) => $"{a}|{b}" );

			var entry = new CommitEntry( msg, hash, relDate, author );
			entry.IsPublished = published.Contains( entry.Hash );
			entry.IsHidden = cfg.Hidden.Contains( entry.Hash );

			// published commits might not be in order so mark the 1st one git shows as latest
			if ( !foundLatest && entry.IsPublished ) {
				entry.IsLatest = true;
				foundLatest = true;
			}

			Commits.Add( entry );
		}

		GetAncestor<ChangelogWidget>()?.ChangelogPreview?.UpdatePreview( this );
		SearchChanged();
	}
}
