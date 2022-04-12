using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// A Client for TextAnalyzer
/// </summary>
namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a client to connect to the remote host
            Client textAnalyzerClient = new Client("net.tcp://localhost:8080/TextAnalyzer");

            if (args.Length > 0)
            {
                string fileName = args[0];

                Console.WriteLine("Sending file: '{0}' to text analyzer server", fileName);
                KeyValuePair<string, int> frequentWord = textAnalyzerClient.TextAnalyzerClient.AnalyzeText(File.ReadAllText(fileName));
                Console.WriteLine("The most repeated word is: '{0}' and it appeared {1} times in the text.", frequentWord.Key, frequentWord.Value);

                Console.WriteLine("Sending file: '{0}' to text analyzer server", fileName);
                Dictionary<string, HashSet<string>> typos = textAnalyzerClient.TextAnalyzerClient.FindTyposForWordsInText(File.ReadAllText(fileName));
                Console.WriteLine("There are {0} typos in the text:", typos.Count);
                foreach (var item in typos)
                {
                    Console.WriteLine("'{0}' has the following similare typos: {1}", item.Key, string.Join(",", item.Value));
                }

                Console.Write("Press any key to exit...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("No text file name was provided");
                Console.WriteLine("Usage: Client.exe [file name]");
            }
        }
    }
}
