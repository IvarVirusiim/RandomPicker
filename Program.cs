﻿using Shell32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RandomPicker
{
    class Program
    {
        static String[] extensions = { ".mov", ".mp4", ".avi", ".mpeg", ".mpg", ".wmv", ".mkv", ".m4v", ".flv" };
        static Random random = new Random();
        enum UserChoice
        {
            Cancel = 0,
            Yes = 1,
            Next = 2,
            Previous = 3,
            Details = 4
        }

        [STAThread]
        static void Main(string[] args)
        {
            var files = getAllMediaFiles(Directory.GetCurrentDirectory());

            UserChoice choice;
            String fileName;

            // Stop if no files
            if (!files.Any())
                return;

            //Create a randomized list of indexes
            List<int> indexes = new List<int>();
            for (int i = 0; i < files.Count; i++)
                indexes.Add(i);

            // Randomize list (Fisher-Yates)
            int n = indexes.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                int value = indexes[k];
                indexes[k] = indexes[n];
                indexes[n] = value;
            }

            int picker = 0;
            do
            {
                // Chooses an index using the randomized list
                var index = indexes[picker];
                fileName = files[index];
                if (picker == 0)
                    Console.WriteLine(generatePromptString(fileName, true, false));
                else if (picker == indexes.Count - 1)
                    Console.WriteLine(generatePromptString(fileName, false, true));
                else
                    Console.WriteLine(generatePromptString(fileName, true, true));

                // Continue to loop, play or stop
                choice = readInput();
                if (choice == UserChoice.Next)
                {
                    if (picker == indexes.Count - 1)
                        continue;
                    else
                        picker++;
                }
                else if (choice == UserChoice.Previous)
                {
                    if (picker == 0)
                        picker++;
                    else
                        picker--;
                }
                else if (choice == UserChoice.Details)
                {
                    printDetails(fileName);
                }

            } while ((int)choice > 1);

            if (choice == UserChoice.Cancel)
                return;
            else
                System.Diagnostics.Process.Start(fileName);
        }

        private static String generatePromptString(String fileName, bool next, bool previous)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("\nPlay \"{0}\"? [Y]es, ", fileName.Split('\\').Last()));

            if (next)
                sb.Append(" [N]ext,");

            if (previous)
                sb.Append(" [P]revious,");

            sb.Append(" [C]ancel, [D]etails.");

            return sb.ToString();
        }

        static UserChoice readInput()
        {
            // Read key and apply choice
            var key = Console.ReadKey();
            switch (key.KeyChar)
            {
                case 'y':
                    return UserChoice.Yes;
                case 'c':
                    return UserChoice.Cancel;
                case 'p':
                    return UserChoice.Previous;
                case 'd':
                    return UserChoice.Details;
                default:
                    return UserChoice.Next;
            }
        }

        static List<String> getAllMediaFiles(String path)
        {
            // List to return
            var files = new List<String>();

            try
            {
                // Get all files in current dir
                files.AddRange(Directory.GetFiles(path).Where(f => extensions.Any(e => f.EndsWith(e))).ToList());
            }
            catch (Exception)
            {
                //Skip places with exceptions
            }

            String[] dirs = null;
            try
            {
                // Get all dirs in current dir
                dirs = Directory.GetDirectories(path);
            }
            catch (Exception)
            {
                //Skip places with exceptions
            }

            if (dirs != null)
            {
                // Do all this to the subdirs
                foreach (var dir in dirs)
                {
                    files.AddRange(getAllMediaFiles(dir));
                }
            }

            // Return all files collected
            return files;
        }

        private static void printDetails(String filePath)
        {
            List<string> arrHeaders = new List<string>();
            var folderPath = Path.GetDirectoryName(filePath);

            Shell shell = new Shell();
            Folder shellFolder = shell.NameSpace(folderPath);
            FolderItem shellFile = shellFolder.ParseName(Path.GetFileName(filePath));
            // Video length id 27

            // Get all possible headers
            for (int i = 0; i < short.MaxValue; i++)
            {
                string header = shellFolder.GetDetailsOf(null, i);
                if (String.IsNullOrEmpty(header))
                    break;
                arrHeaders.Add(header);
            }

            // To look more pretty
            Console.WriteLine();

            // Print only values available
            for (int i = 0; i < arrHeaders.Count; i++)
            {
                var header = arrHeaders[i];
                var value = shellFolder.GetDetailsOf(shellFile, i).Trim();
                if (!String.IsNullOrWhiteSpace(value))
                {
                    System.Diagnostics.Debug.WriteLine($"{i}\t{header}: {value}");

                    if (header.Length > 7)
                        Console.WriteLine($"{header}:\t {value}");
                    else
                        Console.WriteLine($"{header}:\t\t {value}");

                }
            }
        }
    }
}
