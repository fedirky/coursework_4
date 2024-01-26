using System.Collections.Concurrent;
using System.Text;
using System.Diagnostics;

namespace BSBI
{
    class BSBI
    {
        private const int BlockSize = 10;
        private ConcurrentDictionary<string, ConcurrentBag<string>> concurrentInvertedIndex = new ConcurrentDictionary<string, ConcurrentBag<string>>();

        public void BSBIIndexConstruction(string sourceDirectory, int maxDegreeOfParallelism = 4)
        {
            var allFiles = GetAllFiles(sourceDirectory);

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < allFiles.Count; i++)
            {
                var block = new List<string> { File.ReadAllText(allFiles[i]) };
    
                var invertedIndexBlock = BSBIInvert(block, allFiles[i]);
                MergeInvertedIndex(invertedIndexBlock);
            }
            stopwatch.Stop();

            Console.WriteLine($"Час виконання: {stopwatch.ElapsedMilliseconds} мс");
        }
        
        private Dictionary<string, HashSet<string>> BSBIInvert(List<string> block, string filePath)
        {
            var invertedIndexBlock = new Dictionary<string, HashSet<string>>();

            foreach (var docText in block)
            {
                var words = docText.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    if (!invertedIndexBlock.ContainsKey(word))
                    {
                        invertedIndexBlock[word] = new HashSet<string>();
                    }

                    invertedIndexBlock[word].Add(filePath);
                }
            }

            return invertedIndexBlock;
        }

        private void MergeInvertedIndex(Dictionary<string, HashSet<string>> invertedIndexBlock)
        {
            foreach (var entry in invertedIndexBlock)
            {
                string word = entry.Key;
                HashSet<string> filePaths = entry.Value;

                if (!concurrentInvertedIndex.ContainsKey(word))
                {
                    concurrentInvertedIndex[word] = new ConcurrentBag<string>();
                }

                foreach (var filePath in filePaths)
                {
                    concurrentInvertedIndex[word].Add(filePath);
                }
            }
        }

        public List<string> Search(string searchTerm)
        {
            if (concurrentInvertedIndex.ContainsKey(searchTerm))
            {
                return concurrentInvertedIndex[searchTerm].ToList();
            }
            else
            {
                Console.WriteLine();
                return new List<string>();
            }
        }
        
        private List<string> GetAllFiles(string rootDirectory)
        {
            var allFiles = new List<string>();
            Stack<string> dirs = new Stack<string>();

            if (!Directory.Exists(rootDirectory))
            {
                throw new ArgumentException("Directory does not exist.");
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

            return allFiles;
        }

        
        public string PerformSearch(string searchTerm)
        {
            List<string> searchResults = Search(searchTerm);

            if (searchResults.Count > 0)
            {
                StringBuilder result = new StringBuilder();
                result.AppendLine($"Результат пошуку для '{searchTerm}':");

                foreach (var filePath in searchResults)
                {
                    result.AppendLine($"Знайдено у файлі: {filePath}");
                }

                return result.ToString();
            }
            else
            {
                return $"Слово '{searchTerm}' не знайдено.";
            }
        }

        /*static void Main()
        {
            var bsbi = new BSBI();
            bsbi.BSBIIndexConstruction(@"C:\Users\fedir\PycharmProjects\files_creator\selected_files");
            Console.WriteLine(bsbi.PerformSearch("dogs"));
        }*/
    }
}



