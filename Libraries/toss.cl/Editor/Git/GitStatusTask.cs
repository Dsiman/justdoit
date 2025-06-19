using System.Collections.Generic;
using System.Linq;

namespace Changelog.Git;

public static class GitStatusTask
{
    public static IEnumerable<GitChange> Start()
    {
        // https://git-scm.com/docs/git-status
        var std = GitRepo.RunCommand( "--no-optional-locks status -z" );

        if ( std.StartsWith( "fatal:" ) )
            return null; // todo throw some errors

        // last item will be empty https://stackoverflow.com/a/70672739
        var status = std.Split( '\0' )[..^1];

        return status.Select( s => new GitChange( s.Substring( 0, 2 ), s.Substring( 2 ).Trim() ) );
    }
}