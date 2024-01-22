using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


class BSBI
{
    private List<string> blockFiles = new List<string>();
    private const int BlockSize = 10;
    private readonly Dictionary<string, HashSet<int>> invertedIndex = new Dictionary<string, HashSet<int>>();

    public void BSBIIndexConstruction(string sourceDirectory)
    {
        var allFiles = GetAllFiles(sourceDirectory);
        int totalFiles = allFiles.Length;
        int fileIndex = 0;
        int blockNumber = 0;

        while (fileIndex < totalFiles)
        {
            blockNumber++;
            var block = ParseNextBlock(allFiles, fileIndex, BlockSize);
            fileIndex += BlockSize;

            var invertedIndexBlock = BSBIInvert(block, blockNumber);
            MergeInvertedIndex(invertedIndexBlock);
        }
    }

    public List<int> Search(string searchTerm)
    {
        if (invertedIndex.ContainsKey(searchTerm))
        {
            return invertedIndex[searchTerm].ToList();
        }
        else
        {
            return new List<int>(); // Повертаємо пустий список, якщо слово не знайдено.
        }
    }
    
    private string[] GetAllFiles(string rootDirectory)
    {
        var allFiles = new List<string>();
        Stack<string> dirs = new Stack<string>();

        if (!Directory.Exists(rootDirectory))
        {
            throw new ArgumentException();
        }

        dirs.Push(rootDirectory);

        while (dirs.Count > 0)
        {
            string currentDir = dirs.Pop();
            string[] subDirs;
            string[] files;

            try
            {
                subDirs = Directory.GetDirectories(currentDir);
                files = Directory.GetFiles(currentDir);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
                continue;
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
                continue;
            }

            foreach (string file in files)
            {
                try
                {
                    allFiles.Add(file);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
            }

            foreach (string str in subDirs)
                dirs.Push(str);
        }

        return allFiles.ToArray();
    }

    private List<string> ParseNextBlock(string[] allFiles, int startIndex, int blockSize)
    {
        var block = new List<string>();
        for (int i = startIndex; i < Math.Min(startIndex + blockSize, allFiles.Length); i++)
        {
            block.Add(File.ReadAllText(allFiles[i]));
        }
        return block;
    }

    private Dictionary<string, HashSet<int>> BSBIInvert(List<string> block, int blockNumber)
    {
        var invertedIndexBlock = new Dictionary<string, HashSet<int>>();

        foreach (var doc in block)
        {
            var docId = blockNumber * BlockSize - (BlockSize - 1);
            var words = doc.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                if (!invertedIndexBlock.ContainsKey(word))
                {
                    invertedIndexBlock[word] = new HashSet<int>();
                }
                if (!invertedIndexBlock[word].Contains(docId))
                {
                    invertedIndexBlock[word].Add(docId);
                }
                docId++;
            }
        }

        return invertedIndexBlock;
    }

    private void MergeInvertedIndex(Dictionary<string, HashSet<int>> invertedIndexBlock)
    {
        foreach (var entry in invertedIndexBlock)
        {
            string word = entry.Key;
            HashSet<int> docIds = entry.Value;

            if (!invertedIndex.ContainsKey(word))
            {
                invertedIndex[word] = new HashSet<int>();
            }

            invertedIndex[word].UnionWith(docIds);
        }
    }
    
    public string PerformSearch(string searchTerm)
    {
        List<int> searchResults = Search(searchTerm);

        if (searchResults.Count > 0)
        {
            var groupedResults = searchResults.GroupBy(docId => (docId - 1) / BlockSize + 1);
            StringBuilder result = new StringBuilder();
            result.AppendLine($"Результат пошуку для '{searchTerm}':");

            foreach (var group in groupedResults)
            {
                int blockNumber = group.Key;
                var docIds = group.ToList();
                result.AppendLine($"Знайдено у блоці {blockNumber} у документах: {string.Join(", ", docIds)}");
            }

            return result.ToString();
        }
        else
        {
            return $"Слово '{searchTerm}' не знайдено.";
        }
    }

    static void Main()
    {
        var bsbi = new BSBI();
        bsbi.BSBIIndexConstruction(@"C:\Users\fedir\PycharmProjects\files_creator\selected_files");
        Console.WriteLine(bsbi.PerformSearch("I"));
    }
}


