using Octokit;
using Spectre.Console;

namespace syckle
{
    internal class Program
    {
        static void WriteLineClr(object line, ConsoleColor col)
        {
            Console.ForegroundColor = col;
            Console.WriteLine(line);
            Console.ResetColor();
        }

        static void WriteClr(object line, ConsoleColor col)
        {
            Console.ForegroundColor = col;
            Console.Write(line);
            Console.ResetColor();
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Write("Welcome to ");
                WriteClr("syckle\n", ConsoleColor.Red);

                Console.WriteLine("\nCommand-Line Arguments:");
                WriteLineClr("install (installs latest version)", ConsoleColor.Red);
                WriteLineClr("upgrade (upgrades to latest version)", ConsoleColor.Green);
                WriteLineClr("pkg-get <package-name> (Adds a SYPM package globally into Scythe.)", ConsoleColor.Blue);
            }
            else
            {
                if(args.Contains("install"))
                {
                    Console.WriteLine("Preparing to install Scythe.");

                    Console.WriteLine("Fetching latest release of Scythe.");

                    var github = new GitHubClient(new ProductHeaderValue("syckle"));

                    var repo = github.Repository.Get("Fernion-Team", "ScytheLang").Result;

                    var release = github.Repository.Release.GetLatest(repo.Id).Result;

                    var latestRelease = release.ZipballUrl;

                    Console.WriteLine("Downloading Release " + release.Name);

                    using(var client = new System.Net.WebClient())
                    {
                        client.Headers.Add("user-agent", "Fernion-Team");
                        client.DownloadFile(latestRelease, "scythe-latest.zip");
                    }

                    Console.WriteLine("Unzipping downloaded release.");

                    System.IO.Compression.ZipFile.ExtractToDirectory("scythe-latest.zip", "scythe");

                    Console.WriteLine("Compiling Scythe from Source.");

                    var scytheFolder = Directory.GetDirectories("scythe").First();

                    var dotnetInfo = new System.Diagnostics.ProcessStartInfo("dotnet", "publish -c Release");
                    dotnetInfo.WorkingDirectory = scytheFolder;
                    dotnetInfo.CreateNoWindow = true;

                    var proc = System.Diagnostics.Process.Start(dotnetInfo);

                    while (!proc.HasExited)
                    {
                        ;
                    }

                    Console.WriteLine("Copying built Scythe to new directory.");

                    var builtBinaries = Path.Combine(Directory.GetDirectories(Path.Combine(scytheFolder, "bin", "Release", "net6.0")).First(), "publish");

                    Directory.Move(builtBinaries, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "scythe-c"));

                    Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "scythe-c", "libs"));

                    Console.WriteLine("Adding to PATH.");

                    try
                    { 
                        var name = "PATH";
                        var scope = EnvironmentVariableTarget.Machine; // or User
                        var oldValue = Environment.GetEnvironmentVariable(name, scope);
                        var newValue = oldValue + ";" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "scythe-c");
                        Environment.SetEnvironmentVariable(name, newValue, scope);
                    }
                    catch(System.Security.SecurityException e)
                    {
                        WriteLineClr("Please re-run the installer with elevated permissions.", ConsoleColor.Red);

                        Directory.Delete("scythe", true);
                        Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "scythe-c"), true);
                        File.Delete("scythe-latest.zip");

                        Environment.Exit(8008); // lol.
                    }

                    Console.WriteLine("Cleaning...");

                    Directory.Delete("scythe", true);
                    File.Delete("scythe-latest.zip");

                    WriteLineClr("Scythe has been installed!", ConsoleColor.Red);
                }
                else if(args.Contains("upgrade"))
                {
                    try
                    {
                        Console.WriteLine("Preparing to update Scythe.");

                        Console.WriteLine("Fetching latest release of Scythe.");

                        var github = new GitHubClient(new ProductHeaderValue("syckle"));

                        var repo = github.Repository.Get("Fernion-Team", "ScytheLang").Result;

                        var release = github.Repository.Release.GetLatest(repo.Id).Result;

                        var latestRelease = release.ZipballUrl;

                        Console.WriteLine("Downloading Release " + release.Name);

                        using (var client = new System.Net.WebClient())
                        {
                            client.Headers.Add("user-agent", "Fernion-Team");
                            client.DownloadFile(latestRelease, "scythe-latest.zip");
                        }

                        Console.WriteLine("Unzipping downloaded release.");

                        System.IO.Compression.ZipFile.ExtractToDirectory("scythe-latest.zip", "scythe");

                        Console.WriteLine("Compiling Scythe from Source.");

                        var scytheFolder = Directory.GetDirectories("scythe").First();

                        var dotnetInfo = new System.Diagnostics.ProcessStartInfo("dotnet", "publish -c Release");
                        dotnetInfo.WorkingDirectory = scytheFolder;
                        dotnetInfo.CreateNoWindow = true;

                        var proc = System.Diagnostics.Process.Start(dotnetInfo);

                        while (!proc.HasExited)
                        {
                            ;
                        }

                        Console.WriteLine("Copying built Scythe to new directory.");

                        var builtBinaries = Path.Combine(Directory.GetDirectories(Path.Combine(scytheFolder, "bin", "Release", "net6.0")).First(), "publish");

                        Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "scythe-c"), true);

                        Directory.Move(builtBinaries, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "scythe-c"));

                        Console.WriteLine("Cleaning...");

                        Directory.Delete("scythe", true);
                        File.Delete("scythe-latest.zip");
                    }
                    catch
                    {
                        Directory.Delete("scythe", true);
                        File.Delete("scythe-latest.zip");
                    }
                }
            }
        }
    }
}