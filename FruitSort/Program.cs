using System.CommandLine;
using System.Diagnostics;
using FruitSort;

// Build the application options
var inputArgument = new Argument<FileInfo>(
    "input",
    () => new FileInfo("input.txt"),
    "File with lines to sort in the format 'Number. String'. Newline delimited.");

var outputArgument = new Argument<FileInfo>(
    "output",
    () => new FileInfo("output.txt"),
    "File to output. If it exists it will be overwritten. Newline delimited.");

var chunkSizeOption = new Option<int>(
    "--chunkSize",
    () => 5_000_000,
    "Number of lines to split the input in chunks. Adjust this for performance depending on your hardware characteristics (input size, amount of RAM, disk seek times).");

var rootCommand = new RootCommand(
    """
    Sort a file with lines in the format 'Number. String'.
    The lines are sorted first by the String part and then by the Number part.
    It uses a two-pass external sort (https://en.wikipedia.org/wiki/External_sorting#External_merge_sort) to handle very big files.
    """);
rootCommand.AddArgument(inputArgument);
rootCommand.AddArgument(outputArgument);
rootCommand.AddOption(chunkSizeOption);

rootCommand.SetHandler<FileInfo, FileInfo, int>(
    SortFile,
    inputArgument,
    outputArgument,
    chunkSizeOption);

// Start the application
return await rootCommand.InvokeAsync(args);

static void SortFile(FileInfo inputFileInfo, FileInfo outputFileInfo, int chunkSize)
{
    // Prepare directory for chunks
    if (Directory.Exists("tmp"))  
    {  
        Directory.Delete("tmp", true);  
    }
    Directory.CreateDirectory("tmp"); 

    var input = File.ReadLines(inputFileInfo.FullName);
    File.Delete(outputFileInfo.FullName);

    using var output = File.AppendText(outputFileInfo.FullName);

    // Keep chunk filenames for easier access later
    using var chunks = new ChunksCollection();

    var buffer = new List<string>(chunkSize);
    var lineComparer = new LineComparer();

    var sw = Stopwatch.StartNew();

    Console.WriteLine($"{DateTime.Now} Chunking and sorting...");
    // Split the input file in chunks
    input.Chunk(chunkSize)
        .AsParallel()
        .ForAll(chunk => {
            Array.Sort(chunk, lineComparer);
            var chunkName = $"tmp/{System.Guid.NewGuid()}";
            File.WriteAllLines(chunkName, chunk);
            chunks.Add(File.OpenText(chunkName));
        });

    // Perform k-way merge and write to output
    Console.WriteLine($"{DateTime.Now} Merging...");
    var priorityQueue = new PriorityQueue<StreamReader, string>(chunks.Count, lineComparer);

    foreach (var chunk in chunks)
    {
        priorityQueue.Enqueue(chunk, chunk.ReadLine()!);
    }

    while (priorityQueue.TryDequeue(out var sr, out var str))
    {
        if (!string.IsNullOrWhiteSpace(str))
        {
            output.WriteLine(str);
        }

        string? line;
        if (!string.IsNullOrWhiteSpace(line = sr.ReadLine()))
        {
            priorityQueue.Enqueue(sr, line);
        }
    }

    output.Close();
    sw.Stop();
    var seconds = sw.Elapsed.TotalSeconds;
    var outputLength = outputFileInfo.Length;
    Console.WriteLine($"Sorted {outputLength:N} bytes in {seconds:N2} s ({outputLength / seconds:N} bytes/s)");

    chunks.Dispose();
    Directory.Delete("tmp", true);
}
