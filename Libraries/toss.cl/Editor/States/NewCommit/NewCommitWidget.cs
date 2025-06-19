using System.Threading.Tasks;
using Changelog.Git;
using Editor;
using Dialog = Changelog.Elements.Dialog;

namespace Changelog.States.NewCommit;

public sealed class NewCommitWidget : Dialog
{
    private FileList Files;
    private Button CommitBtn;
    private uint NumChanges;
    
    public NewCommitWidget()
    {
        Window.Title = "New Commit - Changelog";
        Window.SetModal( true, true );

        var title = new FileListHeader( "Loading ...", Body );
        title.Title.Bind( "Text" ).ReadOnly()
            .From( () => $"{NumChanges} {( NumChanges == 1 ? "file" : "files" )} changed:", null );
        
        Files = new FileList( Body );

        Body.Layout.Add( title );
        Body.Layout.Add( Files, 1 );
        
        var refresh = title.AddButton( "select_all", "Select All", Files.SelectAll );
        refresh.MouseRightClick = Files.InvertSelect;
        
        title.AddButton( "refresh", "Refresh", RefreshLists );

        CommitBtn = AddFooterButton( "Commit", "commit", Commit );
        CommitBtn.Tint = Theme.Primary;
        CommitBtn.Bind( "Enabled" ).ReadOnly().From( () => NumChanges > 0 && Files.SelectedCount > 0, null );
        CommitBtn.Bind( "Text" ).ReadOnly()
            .From( () =>
            {
                var count = Files.SelectedCount;
                return $"Commit {count} {( count == 1 ? "File" : "Files" )}";
            }, null );
        
        AddCancelButton();
        RefreshLists();
    }

    private void RefreshLists()
    {
        Files.Clear();
        NumChanges = 0;

        var files = GitStatusTask.Start();
        
        foreach ( var file in files ) {
            Files.AddEntry( new FileEntry( file.File, file.Type ) );
            NumChanges++;
        }
    }

    private void Commit()
        => GitCommitTask.Start( null, Committed, Committing );

    private void Committing()
    {
        CommitBtn.Enabled = false;
        CommitBtn.Icon = "pending";
        
        // https://github.com/desktop/desktop/blob/f24287ed5d25d92fe61abb3ee85f8cc8d433ad3e/app/src/lib/git/commit.ts#L15
        // yes i'm doing it in here
        GitRepo.RunCommand( "reset -- ." );
        foreach ( var file in Files.Selected ) {
            GitRepo.RunCommand( $"add -- {file.Name}" );
        }
    }

    private async void Committed( bool done )
    {
        if ( !done ) {
            CommitBtn.Enabled = true;
            CommitBtn.Icon = "add";
            CommitBtn.Tint = Theme.Red.Darken( 0.5f );
            return;
        }
        CommitBtn.TransparentForMouseEvents = true;
        CommitBtn.Icon = "done";
        CommitBtn.Tint = Theme.Green.Darken( 0.5f );
        CommitBtn.Enabled = true;
        await Task.Delay( 300 );
        // TODO refresh main commit list!
        Close();
    }
}