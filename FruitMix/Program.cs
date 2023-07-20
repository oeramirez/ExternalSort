using System.CommandLine;
using System.Diagnostics;

// Build the application options
var dictionaryOption = new Option<FileInfo>(
    "--dictionary",
    () => new FileInfo("fruits.txt"),
    "File with a dictionary words to be used in the mixer. Newline delimited.");

var outputArgument = new Argument<FileInfo>(
    "output",
    () => new FileInfo("output.txt"),
    "File to be output. If it exists it will be overwritten. Newline delimited.");

var linesOption = new Option<long>(
    "--lines",
    () => 50_000_000,
    "Number of lines to mix.");

var minValueOption = new Option<long>(
    "--minValue",
    () => 1,
    "The minimum number to be used in each line (inclusive).");

var maxValueOption = new Option<long>(
    "--maxValue",
    () => 20_000_000,
    "The maximum number to be used in each line (exclusive).");

var maxWordsPerLineOption = new Option<int>(
    "--maxWordsPerLine",
    () => 3,
    "The maximum number of words to be combined in each line.");

var rootCommand = new RootCommand("Create a file with lines in the format 'RandomNumber. RandomString'.");
rootCommand.AddOption(dictionaryOption);
rootCommand.AddArgument(outputArgument);
rootCommand.AddOption(linesOption);
rootCommand.AddOption(minValueOption);
rootCommand.AddOption(maxValueOption);
rootCommand.AddOption(maxWordsPerLineOption);

rootCommand.SetHandler<FileInfo, FileInfo, long, long, long, int>(
    MixFile,
    dictionaryOption,
    outputArgument,
    linesOption,
    minValueOption,
    maxValueOption,
    maxWordsPerLineOption);

// Start the application
return await rootCommand.InvokeAsync(args);

// Produce a file with random numbers and words
static void MixFile(
    FileInfo dictionaryInfo,
    FileInfo output,
    long lines,
    long minValue,
    long maxValue,
    int maxWordsPerLine)
{
    var dictionary = File.ReadAllLines(dictionaryInfo.FullName);
    File.Delete(output.FullName);
    using var streamWriter = File.AppendText(output.FullName);

    var sw = Stopwatch.StartNew();

    for (long line = 0; line < lines; line++)
    {
        streamWriter.WriteLine($"{GetRandomNumber(minValue, maxValue)}. {GetRandomString(dictionary, maxWordsPerLine)}");
    }

    streamWriter.Close();
    var outputLength = output.Length;
    sw.Stop();
    var seconds = sw.Elapsed.TotalSeconds;
    Console.WriteLine($"Mixed {outputLength:N} bytes in {seconds:N2} s ({outputLength / seconds:N} bytes/s)");
}

// Get the next random number
static long GetRandomNumber(long minValue, long maxValue) => Random.Shared.NextInt64(minValue, maxValue);

// Get the next random string
static string GetRandomString(string[] dictionary, int maxWordsPerLine)
{
    var lineWords = Random.Shared.Next(1, maxWordsPerLine + 1);
    var result = new List<string>();

    for (int i = 0; i < lineWords; i++)
    {
        result.Add(dictionary[Random.Shared.Next(dictionary.Length)]);
    }

    return string.Join(' ', result);
}