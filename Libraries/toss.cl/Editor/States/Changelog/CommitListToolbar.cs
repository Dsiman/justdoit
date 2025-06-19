using System.Linq;
using Changelog.Elements;
using Editor;
using Sandbox;

namespace Changelog.States.Changelog;

public class CommitListToolbar : Toolbar
{
    private CommitListContainer Container;
    
    public CommitListToolbar( CommitListContainer parent ) : base( parent )
    {
	    Container = parent;

	    var addBtn = AddButton( "Commit", "add" );
	    addBtn.Pressed = AddMenu;
	    
	    AddSearch( "Search commits and authors" );
        var latestBtn = AddIcon( "gps_fixed", "Mark Commit as Latest", MarkLatestDialog );
        var showHideBtn = AddIcon( "visibility_off", "Toggle Commit Show / Hide\n(right click: invert selection)",
	        ShowHide, InvertShowHide );
        AddRefresh( Refresh );

        latestBtn.Bind( "Enabled" ).ReadOnly().From( CommitSelected, null );
        showHideBtn.Bind( "Enabled" ).ReadOnly().From( CommitSelected, null );
    }

    private void AddMenu()
    {
	    var menu = new CommitMenu( this );
	    menu.OpenAtCursor( true );
    }

    private void ShowHide()
	{
		var commit = Container.List.SelectedItems.FirstOrDefault();
		if ( commit is not CommitEntry entry ) return;

		entry.IsHidden = !entry.IsHidden;
		
		var cfg = ChangelogConfig.Project;

		if ( entry.IsHidden ) {
			if ( !cfg.Hidden.Contains( entry.Hash ) )
				cfg.Hidden.Add( entry.Hash );
		}
		else
			cfg.Hidden.Remove( entry.Hash );
		
		ChangelogConfig.Project = cfg;
		GetAncestor<ChangelogWidget>()?.ChangelogPreview?.UpdatePreview();
		Container.List.Update();
	}

	private void InvertShowHide()
	{
		var container = Container?.List;
		if ( !container.IsValid() ) return;

		var commits = container.Entries;
		var cfg = ChangelogConfig.Project;
		
		foreach ( var commit in commits ) {
			commit.IsHidden = !commit.IsHidden;
			
			if ( commit.IsHidden ) {
				if ( !cfg.Hidden.Contains( commit.Hash ) )
					cfg.Hidden.Add( commit.Hash );
			}
			else
				cfg.Hidden.Remove( commit.Hash );
		}
		
		ChangelogConfig.Project = cfg;
		
		GetAncestor<ChangelogWidget>()?.ChangelogPreview?.UpdatePreview();
		container.Update();
	}

	private void MarkLatestDialog()
	{
		var commit = Container.List.SelectedItems.FirstOrDefault();
		if ( commit is not CommitEntry entry ) return;

		EditorUtility.DisplayDialog( "Mark commit as latest - Changelog",
			"All commits up to and including this will be marked as published:\n" +
			$"{entry.Name} ({entry.Hash}, {entry.Date})" +
			"\nAre you sure?", "Cancel", "OK", MarkLatest, "\u2622" );
	}

	private void MarkLatest()
		=> MarkLatest( GetAncestor<CommitListContainer>()?.List.SelectedItems.OfType<CommitEntry>().FirstOrDefault() );

	public void MarkLatest( CommitEntry commit )
	{
		// yes was clicked to above dialog... or it's being forced
		// TODO check if latest commit was actually later on
		var list = Container?.List;
		if ( !list.IsValid() ) return;

		var commits = list.Entries.ToList();
		commit ??= Container.Commits.FirstOrDefault();
		if ( commit is null ) return;

		var idx = commits.IndexOf( commit );
		if ( idx < 0 ) return;

		var cfg = ChangelogConfig.Project;
		for ( int i = idx; i < commits.Count; i++ ) {
			if ( commits[i].IsHidden )
				continue;
			
			commits[i].IsHidden = false;
			commits[i].IsLatest = false;
			commits[i].IsPublished = true;
			cfg.Published.Add( commits[i].Hash );
		}

		commits[idx].IsLatest = true;
		ChangelogConfig.Project = cfg;
		
		GetAncestor<ChangelogWidget>()?.ChangelogPreview?.UpdatePreview();
	}

	private void Refresh()
		=> Container.Reset();

	private bool CommitSelected()
		=> Container.List.SelectedItems.Any();
}