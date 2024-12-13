using CommandExecutor;
using System.Text;

namespace last_commit
{
  internal class Program
  {
    static void Main(string[] args)
    {
      var repos = args.First(x => x.Contains("repos=")).Split('=').Last();
      var date = args.First(x => x.Contains("date=")).Split('=').Last();
      var remote = args.FirstOrDefault(x => x.Contains("remote="))?.Split('=').Last() ?? "origin";
      var path = args.FirstOrDefault(x => x.Contains("output="))?.Split('=').Last() ?? "branches.md";

      File.Delete(path);

      var drive = repos.Split(':').FirstOrDefault();
      var executor = new CommandLineExecutor();
      executor.Execute(drive, $"cd {repos}", $"git fetch --all -p");
      var branches = executor.Execute($"{drive}:", $"cd {repos}", "git branch -r").Split().Where(x => x.Contains(remote));
      foreach (var branch in branches)
      {
        var lastCommit = executor.Execute($"{drive}:", $"cd {repos}", $"git log --pretty=format:\"%ae;%ad\" --date=short -n 1 {branch}").Split(Environment.NewLine)[8].Split(';');
        var message = $"branch:{branch}\tlast commited:{lastCommit[0]}\tat:{lastCommit[^1]}";
        if (DateTime.Parse(lastCommit[^1]) < DateTime.Parse(date))
        {
          Console.ForegroundColor = ConsoleColor.Green;
          File.AppendAllText(path, message + Environment.NewLine, Encoding.UTF8);
        }

        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.White;
      }
    }
  }
}
