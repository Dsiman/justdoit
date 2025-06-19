using System.Diagnostics;
using Sandbox;

namespace Changelog.Git;

// pov: no nuget
// https://blog.somewhatabstract.com/2015/06/22/getting-information-about-your-git-repository-with-c/
public static class GitRepo
{
	private static readonly Process _process = new()
	{
		StartInfo = new ProcessStartInfo
		{
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true,
			FileName = "git",
			WorkingDirectory = Project.Current.RootDirectory.FullName,
		},
	};

	// https://git-scm.com/docs/git-branch#Documentation/git-branch.txt---show-current
	public static string Branch
		=> RunCommand( "branch --show-current" );

	public static string LatestCommit
		=> RunCommand( "rev-parse HEAD" );

	public static bool HasUncommittedChanges
		=> string.IsNullOrWhiteSpace( RunCommand( "status --porcelain" ) );

	public static bool Exists
		=> !RunCommand( "status" ).StartsWith( "fatal:" );

	// --no-pager https://stackoverflow.com/a/2183920
	// --pretty... https://git-scm.com/book/en/v2/Git-Basics-Viewing-the-Commit-History
	/// <summary>
	/// List of commits in the format "[abbreviated hash]|[relative date]|[author]|[message]"
	/// </summary>
	public static string[] Commits
		=> RunCommand( "--no-pager log --pretty=format:\"%h|%ar|%an|%s\"" ).Split( '\n' );

	internal static string RunCommand( string args )
	{
		_process.StartInfo.Arguments = args;
		_process.Start();
		var output = _process.StandardOutput.ReadToEnd();
		var error = _process.StandardError.ReadToEnd();
		_process.WaitForExit();
		return ( _process.ExitCode == 0 ? output : error ).Trim();
	}
}
