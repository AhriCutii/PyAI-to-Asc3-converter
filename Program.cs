using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please drag and drop a file onto the exe.");
            Console.WriteLine("Press any key to continue . . .");
            return;
        }

        // Get the file path from the command-line argument
        string inputFilePath = args[0];

        // Read all lines from the input file into a string array
        string[] PyAIfile = ReadAllLinesFromFile(inputFilePath);
        
        bool removeExtdef = false;
        string scriptId = "";
        string scriptString = "";
        int headerLineCount = 0;
        //now do the thing
        for (int i = 0; i < PyAIfile.Length; i++)
        {
            string line = PyAIfile[i];
            if (IsHeader(line, out bool SecondHeaderLine))
            {
                if (SecondHeaderLine)
                {
                    scriptId = GetHeaderProperties(line, false);

                }
                headerLineCount++;
                continue;
            }
            //Console.WriteLine("Converting line:" + lines[i]);
            PyAIfile[i] = ConvertLineToAsc3(line, scriptId);
            //Console.WriteLine("Line converted to:" + lines[i]);
        }
        //check for lines to remove from the start of the original file, if it has the extdef line
        int lineIndexToRemove = 0;
        for (int i = 0; i < PyAIfile.Length; i++)
        {
            string line = PyAIfile[i];
            //check for the exdef line at the start
            if (i == 0)
            {
                if (line.StartsWith("extdef"))
                {
                    removeExtdef = true;
                    continue;
                }
                else
                {
                    Console.WriteLine("No exdef line");
                    break;
                }
            }
            //calculate the number of lines to remove from the start
            if(removeExtdef)
            {
                if(string.IsNullOrWhiteSpace(line))
                {
                    lineIndexToRemove++;
                }
                else
                {
                    Console.WriteLine(lineIndexToRemove + 1 + " lines to remove");
                    break;
                }
            }
        }
        //now create a new array with the correct length and copy the contents of the old array inside it, skipping
        //the lines we want removed and adding script_name and script_id lines after the first header
        int asc3Index = 0;
        string[] Asc3file = new string[PyAIfile.Length - lineIndexToRemove + headerLineCount];
        int LenghtDifference = Asc3file.Length - PyAIfile.Length; // Indexes are dumb. Just start everything from 1!
        for (int i = 0; i < PyAIfile.Length; i++)
        {
            //skip the extdef line at the start if it exists
            if (removeExtdef && i <= lineIndexToRemove)
            {
                continue;
            }
            //implement header if it exists
            if (IsHeader(PyAIfile[i]))
            {
                //first copy the original header in the first two lines of the file
                Asc3file[asc3Index] = "#" + PyAIfile[i];
                scriptString = GetHeaderProperties(PyAIfile[i], true);
                asc3Index++;
                i++;
                Asc3file[asc3Index] = "#" + PyAIfile[i];
                scriptId = GetHeaderProperties(PyAIfile[i], false);
                asc3Index++;
                i++;
                Asc3file[asc3Index] = "script_name " + scriptString;
                asc3Index++;
                Asc3file[asc3Index] = "script_id " + scriptId;
                asc3Index++;
            }
            Asc3file[asc3Index] = PyAIfile[i];
            asc3Index++;
        }

        // Get the directory and file name of the input file
        string directory = Path.GetDirectoryName(inputFilePath);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFilePath);
        // Create the output file path with .asc3 extension in the same directory
        string outputFilePath = Path.Combine(directory, fileNameWithoutExtension + ".asc3");

        // Write the modified content to the output file
        WriteToFile(outputFilePath, Asc3file);
        Console.WriteLine("Convesion done. Press any key to continue . . .");
        Console.Read();
    }

    static bool IsHeader(string line, out bool isSecondHeaderLine)
    {
        if (line.StartsWith("# stat_txt.tbl entry"))
        {
            isSecondHeaderLine = false;
            return true;
        }
        else
        {
            isSecondHeaderLine = true;
            return line.EndsWith("):");
        }
    }

    static bool IsHeader(string line)
    {
        if (line.StartsWith("# stat_txt.tbl entry"))
        {
            return true;
        }
        else
        {
            return line.EndsWith("):");
        }
    }

    static string ConvertLineToAsc3(string line, string scriptMeow)
    {
            // Remove leading tab spaces
            line = line.TrimStart('\t');

            // Remove commas
            line = line.Replace(",", "");

            // Check if the line contains "--"
            if (line.Contains("--"))
            {
                // If the line contains "--", remove all "--" symbols
                line = line.Replace("--", "");
                // Prepend ":" to the line
                line = ":" + line;
            }

            // Remove the first parenthesis symbol only if the line ends with a parenthesis
            bool endsWithParenthesis = line.EndsWith(")");
            if (endsWithParenthesis)
            {
                int openingIndex = line.IndexOf('(');
                if (openingIndex != -1)
                {
                    line = line.Remove(openingIndex, 1).Insert(openingIndex, " ");
                }
            }

            // If the line ends with a parenthesis, remove the last parenthesis symbol
            if (endsWithParenthesis)
            {
                line = line.Substring(0, line.Length - 1);
            }
            //Remove the space inside blocknames if there is a header
            if (scriptMeow != "" && line.Contains(scriptMeow + " "))
        {
            line = line.Replace(scriptMeow + " ", scriptMeow);
        }
        return line;
    }
    static string GetHeaderProperties(string Meow, bool isFirstLine)
    {
        //extract the string, without the "<0>" at the end
        if (isFirstLine)
        {
            int startIndex = Meow.IndexOf(':');
            int EndIndex = Meow.IndexOf('<');
            Meow = Meow.Substring(startIndex + 2, EndIndex - startIndex - 2);
        }
        //string id is the first four characters
        else
        {
            Meow = Meow.Substring(0, 4);
        }
        return Meow;
    }
    // Method to read all lines from a file into a string array
    static string[] ReadAllLinesFromFile(string filePath)
    {
        string[] lines;
        try
        {
            lines = File.ReadAllLines(filePath);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("File not found: " + filePath);
            throw;
        }
        return lines;
    }
     // Method to write stuff to a file
    static void WriteToFile(string filePath, string[] lines)
    {
        Console.WriteLine("Writing to file");
        try
        {
            string newFilePath = GetUniqueFileName(filePath);
            File.WriteAllLines(newFilePath, lines);
            Console.WriteLine($"Write successful. File saved as: {newFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error writing to file: " + ex.Message);
            throw;
        }
    }

    // Helper method to generate a unique filename
    static string GetUniqueFileName(string filePath)
    {
        string directory = Path.GetDirectoryName(filePath);
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);
        int count = 0;

        string newFilePath = filePath;
        while (File.Exists(newFilePath))
        {
            if (count == 0)
            {
                newFilePath = Path.Combine(directory, fileName + "_asc3" + extension);
            }
            else
            {
                newFilePath = Path.Combine(directory, $"{fileName}_{count}{extension}");
                count++;
            }
            }

        return newFilePath;
    }
}
