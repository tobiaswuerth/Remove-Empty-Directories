namespace ch.wuerth.tobias.RemoveEmptyDirectories
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal class Program
    {
        private readonly Dictionary<VerbosityLevel, ConsoleColor> _verbosityColors = new Dictionary<VerbosityLevel, ConsoleColor>
                                                                                     {
                                                                                         {VerbosityLevel.ERROR, ConsoleColor.Red},
                                                                                         {VerbosityLevel.DELETION, ConsoleColor.Green},
                                                                                         {VerbosityLevel.INFORMATION, ConsoleColor.White},
                                                                                         {VerbosityLevel.WARNING, ConsoleColor.Yellow}
                                                                                     };

        private Int32 _dirDeleted;
        private String _directoryPath;
        private Int32 _dirIgnored;
        private Int32 _errors;
        private Boolean _includeSubDirs;
        private Boolean _initialized;
        private Int32 _messagesIgnored;
        private VerbosityLevel _verbosity;

        private Program(String[] args)
        {
            HandleArgs(args.ToList());
            if (!_initialized)
            {
                return;
            }

            Console.WriteLine(String.Empty);
            Console.WriteLine("Starting program...");
            Console.WriteLine(String.Empty);
            WorkDir(_directoryPath, 1);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(String.Empty);
            Console.WriteLine("Deleted directories:              {0}", _dirDeleted);
            Console.WriteLine("Ignored directories:              {0}", _dirIgnored);
            Console.WriteLine("Errors occurred:                  {0}", _errors);
            Console.WriteLine("Logmessages ignored (verbosity):  {0}", _messagesIgnored);
        }

        private void Log(VerbosityLevel level, Int32 layer, String message, params Object[] args)
        {
            if (level > _verbosity && (level & VerbosityLevel.FORCE_LOG) != VerbosityLevel.FORCE_LOG)
            {
                _messagesIgnored++;
                return;
            }

            level = level & ~VerbosityLevel.FORCE_LOG;
            Console.ForegroundColor = _verbosityColors[level];
            Console.WriteLine("[" + level.ToString().Substring(0, 3) + "] " + new String(' ', layer) + message, args);
        }

        private void WorkDir(String dir, Int32 layer)
        {
            try
            {
                Log(VerbosityLevel.INFORMATION, layer, "BEGIN {0}", new String('\t', layer), dir);
                if (!Directory.Exists(dir))
                {
                    Log(VerbosityLevel.ERROR, layer, "Directory does not exist '{0}'", dir);
                    _errors++;
                    return;
                }

                String[] directories = Directory.GetDirectories(dir);
                if (_includeSubDirs && 0 < directories.Length)
                {
                    foreach (String d in directories)
                    {
                        WorkDir(d, layer + 1);
                    }
                }

                String[] strings = Directory.GetFiles(dir);
                directories = Directory.GetDirectories(dir);
                if (strings.Length.Equals(0) && directories.Length.Equals(0))
                {
                    // folder is empty -> delete
                    Directory.Delete(dir);
                    Log(VerbosityLevel.DELETION, layer, "DELETED {0}", dir);
                    _dirDeleted++;
                }
                else
                {
                    Log(VerbosityLevel.WARNING, layer, "IGNORED {0}", dir);
                    _dirIgnored++;
                }
                Log(VerbosityLevel.INFORMATION, layer, "FINISHED {0}", dir);
            }
            catch (Exception ex)
            {
                Log(VerbosityLevel.ERROR, layer, "'{0}' @ '{1}'", ex.Message, dir);
                _errors++;
            }
        }

        private void HandleArgs(IList<String> args)
        {
            if (args.Count.Equals(0) || args.Contains("--help"))
            {
                DisplayHelp();
                return;
            }

            for (Int32 i = 0; i < args.Count; i++)
            {
                switch (args[i])
                {
                    case "-d":
                        if (++i >= args.Count)
                        {
                            Log(VerbosityLevel.ERROR | VerbosityLevel.FORCE_LOG, 0, "Directory is undefined");
                            DisplayHelp();
                            return;
                        }

                        String dir = args[i];
                        if (String.IsNullOrWhiteSpace(dir))
                        {
                            Log(VerbosityLevel.ERROR | VerbosityLevel.FORCE_LOG, 0, "Invalid directory");
                            DisplayHelp();
                            return;
                        }

                        _directoryPath = dir;
                        break;
                    case "-s":
                        _includeSubDirs = true;
                        break;
                    case "-v":
                        if (++i >= args.Count)
                        {
                            Log(VerbosityLevel.ERROR | VerbosityLevel.FORCE_LOG, 0, "No verbosity level provided");
                            DisplayHelp();
                            return;
                        }

                        if (args[i] == "0")
                        {
                            // ignore
                            continue;
                        }

                        try
                        {
                            _verbosity = (VerbosityLevel) Enum.Parse(typeof(VerbosityLevel), args[i]);
                        }
                        catch (Exception)
                        {
                            Log(VerbosityLevel.ERROR | VerbosityLevel.FORCE_LOG, 0, "Invalid verbosity level");
                            return;
                        }

                        break;
                    default:
                        Log(VerbosityLevel.ERROR | VerbosityLevel.FORCE_LOG, 0, "Unknown option '{0}'", args[i]);
                        DisplayHelp();
                        return;
                }
            }

            if (null != _directoryPath)
            {
                _initialized = true;
            }
            else
            {
                Log(VerbosityLevel.ERROR | VerbosityLevel.FORCE_LOG, 0, "Directory is not defined");
                DisplayHelp();
            }
        }

        private void DisplayHelp()
        {
            List<String> lines = new List<String>
                                 {
                                     "Options: ",
                                     "--help         Provides information for commands",
                                     "-d <dir>       Specify root directory to search through",
                                     "-s             Include subdirectories",
                                     "-v <level>     Show output info, levels = [ ",
                                     "                    0 - No extra logging (default)",
                                     "                    1 - ERR = Displays errors",
                                     "                    2 - WAR = Also displays warnings",
                                     "                    4 - DEL = Also displays success message if directory is deleted",
                                     "                    8 - INF = Also displays start and stop of working directory",
                                     "               ]"
                                 };
            lines.ForEach(x => Log(VerbosityLevel.INFORMATION | VerbosityLevel.FORCE_LOG, 0, x));
        }

        private static void Main(String[] args) { new Program(args); }

        [Flags]
        private enum VerbosityLevel
        {
            ERROR = 1 << 0,
            WARNING = 1 << 1,
            DELETION = 1 << 2,
            INFORMATION = 1 << 3,
            FORCE_LOG = 1 << 4
        }
    }
}