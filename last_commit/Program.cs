using CommandExecutor;
using System.Text;

namespace last_commit
{
  internal class Program
  {
    private static string _drive;
    private static string _repos;
    private static CommandLineExecutor _executor;

    static void Main(string[] args)
    {
      _repos = args.First(x => x.Contains("repos=")).Split('=').Last();
      var date = args.First(x => x.Contains("date=")).Split('=').Last();
      var remote = args.FirstOrDefault(x => x.Contains("remote="))?.Split('=').Last() ?? "origin";
      var path = args.FirstOrDefault(x => x.Contains("output="))?.Split('=').Last() ?? "branches.md";

      File.Delete(path);

      _drive = _repos.Split(':').FirstOrDefault()!;
      _executor = new CommandLineExecutor();

      ExecuteGitCommand($"git fetch -p -a");
      var branches = ExecuteGitCommand("git branch -r -f").Split().Where(x => x.Contains(remote));
      var noMerghedInMasterBranches = ExecuteGitCommand("git checkout master && git branch -r -f --no-merged").Split().Where(x => x.Contains(remote));

      foreach (var branch in branches)
      {
        var lastCommit = ExecuteGitCommand($"git log --pretty=format:\"%ae;%ad\" --date=short -n 1 {branch}").Split(Environment.NewLine)[8].Split(';');
        var message = $"branch:{branch}\tlast commited:{lastCommit[0]}\tat:{lastCommit[^1]}";
        if (DateTime.Parse(lastCommit[^1]) < DateTime.Parse(date))
        {
          Console.ForegroundColor = ConsoleColor.Green;

          if (noMerghedInMasterBranches.Contains(branch))
            message += "\t NO-MERGED";

          File.AppendAllText(path, message + Environment.NewLine, Encoding.UTF8);
        }

        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.White;
      }
    }

    private static string ExecuteGitCommand(string command) => _executor.Execute($"{_drive}:", $"cd {_repos}", command);
  }
}
