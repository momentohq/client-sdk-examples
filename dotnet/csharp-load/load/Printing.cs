using HdrHistogram;

namespace Printing;

public static class Printing
{
    public record class Column(
        string Title,
        string[] Lines
    )
    {
        public readonly int Width = Lines.Max(l => l.Length);

        public string Line(int line)
        {
            return line < Lines.Length ? Lines[line] : "";
        }
    }

    public static void PrintColumns(params Column[] columns)
    {
        var lines = columns.Max(column => column.Lines.Length);
        const string delimiter = "  |  ";
        var separator = new string('-', delimiter.Length * Math.Max(0, columns.Length - 1) + columns.Sum(c => c.Width));

        Console.WriteLine(separator);
        var title = string.Join(delimiter, columns.Select(c => string.Format($"{{0,-{c.Width}}}", c.Title)));
        Console.WriteLine(title);
        Console.WriteLine(separator);

        for (var line = 0; line < lines; ++line)
        {
            var row = string.Join(delimiter, columns.Select(c => string.Format($"{{0,-{c.Width}}}", c.Line(line))));
            Console.WriteLine(row);
        }
        Console.WriteLine(separator);
    }

    public static string PercentileDistributionToString(this LongHistogram histogram, double scalingFactor)
    {
        var writer = new StringWriter();
        histogram.OutputPercentileDistribution(
            writer,
            outputValueUnitScalingRatio: scalingFactor,
            percentileTicksPerHalfDistance: 1
        );
        return writer.ToString();
    }

    public static void PrintDictionaryIfNotEmpty(this Dictionary<string, uint> dictionary, string title)
    {
        if (0 < dictionary.Count)
        {
            Console.WriteLine($"{title}:");
            var width = dictionary.Max(e => e.Key.Length);
            foreach (var (message, count) in dictionary)
            {
                Console.WriteLine($"  {{0,-{width}}}  |  {count,6}  |", message);
            }
        }
    }
}
