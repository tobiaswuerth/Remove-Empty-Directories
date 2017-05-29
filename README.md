# Remove-Empty-Directories
A CLI program written in C# to remove empty directories in a defined directory

## Usage
```
> .exe --help

Options:
--help         Provides information for commands
-d <dir>       Specify root directory to search through
-s             Include subdirectories
-v <level>     Show output info, levels = [
                    0 - No extra logging (default)
                    1 - ERR = Displays errors
                    2 - WAR = Also displays warnings
                    4 - DEL = Also displays success message if directory is deleted
                    8 - INF = Also displays start and stop of working directory
               ]
```

You must specify a root directory.
Optionally you can include all subdirectories and change the verbosity level

As an example, to remove all empty directories and subdirectories in the C:\ drive and display a decent amount of information you can use the following command:
```
> .exe -d C:\ -s -v 4
```
