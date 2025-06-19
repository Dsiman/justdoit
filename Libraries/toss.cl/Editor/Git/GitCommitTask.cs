using System;
using System.Linq;
using Changelog.Elements;
using Sandbox.Diagnostics;

namespace Changelog.Git;

public static class GitCommitTask
{
    /// <summary>
    /// Make a git commit!
    /// </summary>
    /// <param name="message">commit message. User will be asked to input a message if this is null</param>
    /// <param name="onDone">action invoked when the commit is done. Be prepared for this to NOT be invoked if message is null!</param>
    /// <param name="onStart">action invoked when the commit is started. Be prepared for this to NOT be invoked if message is null!</param>
    /// <param name="allFiles">whether to commit all changed/deleted files (that git has stored) or not</param>
    public static void Start( string message = null, Action<bool> onDone = null, Action onStart = null,
        bool allFiles = false )
    {
        if ( message is null ) {
            new StringDialog( "Please enter a commit message:", m => Start( m, onDone, onStart, allFiles ), "Commit!",
                "commit", "New Commit", true ).Show();
            return;
        }
        
        onStart?.Invoke();

        var cmd = "commit ";

        if ( allFiles )
            cmd += "-a ";

        // TODO escape quotes
        cmd = message.Split( '\n' ).Aggregate( cmd, ( current, line ) => current + $"-m \"{line}\" " );

        var output = GitRepo.RunCommand( cmd );

        new Logger( "Changelog" ).Info( output );

        onDone?.Invoke( true );
    }
}