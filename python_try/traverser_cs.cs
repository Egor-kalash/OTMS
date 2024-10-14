/* -------------------- INSTALL LibGit2Sharp -------------------- */

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using LibGit2Sharp;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Define the Git repository URL and the local directory to clone into
        string repoUrl = "https://github.com/yuriyOtomakeit/LOMS_TIA.git";  // Replace with your repository URL
        string cloneDir = "cloned_repo";  // This will be the directory where the repo is cloned
        string currentTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        // If the Repo was already cloned in this folder it will be replaced by its newest version
        if (Directory.Exists(cloneDir))
        {
            Directory.Delete(cloneDir, true);
            Console.WriteLine($"{cloneDir} has been removed.");
        }
        else
        {
            Console.WriteLine($"{cloneDir} does not exist or is not a directory.");
        }

        // Clone the repository if it hasn't been cloned already
        Console.WriteLine($"Cloning repository from {repoUrl} into {cloneDir}...");
        try
        {
            Repository.Clone(repoUrl, cloneDir);
            Console.WriteLine($"Repository cloned into {cloneDir}.");
        }
        catch (LibGit2SharpException e)
        {
            Console.WriteLine($"Error during cloning: {e.Message}");
            Environment.Exit(1);
        }

        // Define the subdirectory you want to specifically target
        string targetSubdir = Path.Combine(cloneDir, "TIA_Portal_Library", "Classes");

        // Define the number of characters to remove from the start of each line
        int charsToRemove = 16;  // Adjust this number as needed
        int charsRmBack = -2;

        // Define the output file paths
        string outputFile = "output.txt";
        string logsFolder = "logs";
        if (!Directory.Exists(logsFolder))
        {
            Directory.CreateDirectory(logsFolder);
        }
        string outputFileLog = Path.Combine(logsFolder, $"output_{currentTime}.txt");

        // Check if the target subdirectory exists
        if (Directory.Exists(targetSubdir))
        {
            Console.WriteLine($"Processing .scl files in the subdirectory: {targetSubdir}");

            // Find all .scl files recursively in the target subdirectory and its subdirectories
            string[] sclFiles = Directory.GetFiles(targetSubdir, "*.scl", SearchOption.AllDirectories);

            // Open the output file for writing
            using (StreamWriter outputFileWriter = new StreamWriter(outputFile))
            {
                foreach (string sclFile in sclFiles)
                {
                    Console.WriteLine($"Reading file: {sclFile}");
                    // Read and process each file
                    using (StreamReader sr = new StreamReader(sclFile))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            // Check if the keyword "FUNCTION_BLOCK" is in the line (case-sensitive)
                            if (line.Contains("FUNCTION_BLOCK"))
                            {
                                Console.WriteLine($"Match found in {sclFile}: {line.Trim()}");

                                // Extract the function name between quotes
                                int start = line.IndexOf('"') + 1;
                                int end = line.IndexOf('"', start);
                                if (start > 0 && end > start)
                                {
                                    string functionName = line.Substring(start, end - start).Trim();

                                    // Write the modified line to the output file, including the file name for context
                                    string relativePath = sclFile.Substring(cloneDir.Length);  // Shortened path
                                    string result = $"{functionName} : {relativePath}";

                                    outputFileWriter.WriteLine(result);

                                    // Also write to log file
                                    using (StreamWriter logWriter = new StreamWriter(outputFileLog, append: true))
                                    {
                                        logWriter.WriteLine(result);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"Modified lines containing 'FUNCTION_BLOCK' have been saved to {outputFile}.");
        }
        else
        {
            Console.WriteLine($"Subdirectory {targetSubdir} does not exist. Please check the path.");
        }
    }
}
