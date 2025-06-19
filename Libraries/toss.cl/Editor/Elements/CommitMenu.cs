using Changelog.Git;
using Changelog.States.NewCommit;
using Editor;

namespace Changelog.Elements;

public sealed class CommitMenu : ContextMenu
{
    public CommitMenu( Widget parent ) : base( parent )
    {
        AddOption( "All Modified Files", "select_all", CommitAllDialog );
        AddOption( "Select Files \u2026", "highlight_alt", SelectCommit );
    }

    private void SelectCommit()
    {
        var popup = new NewCommitWidget();
        popup.SetModal( true, true );
        popup.Hide();
        popup.Show();
    }

    private void CommitAllDialog()
        => new StringDialog( "Please enter a commit message:", CommitAll, "Commit Modified Files!", "commit",
            "Commit Modified Files", true ).Show();

    private void CommitAll( string msg )
        => GitCommitTask.Start( msg, allFiles: true );
}