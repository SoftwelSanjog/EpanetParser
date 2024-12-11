// Example usage class
using EpanetOutputReader;
using NsEpanetInpParser;
public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            string inpFilePath = @"C:\Users\Sanjog Shakya\Downloads\epanet\Net2.inp";

            var parser = new EpanetInpParser();
            parser.ParseInpFile(inpFilePath);

            if (parser.Validate())
            {
                parser.PrintSummary();

                // Additional processing can be done here
                var firstPipe = parser.Pipes.FirstOrDefault();
                if (firstPipe != null)
                {
                    Console.WriteLine($"First Pipe ID: {firstPipe.Id}");
                    Console.WriteLine($"Pipe Length: {firstPipe.Length}");
                }
            }
            else
            {
                Console.WriteLine("Invalid or incomplete INP file.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing INP file: {ex.Message}");
        }
    }
    public static void ReadOutputFile()
    {
        try
        {
            string rptFilePath = @"C:\path\to\your\simulation.rpt";
            string outFilePath = @"C:\path\to\your\simulation.out";

            var outputParser = new EpanetOutputParser();
            outputParser.ParseOutputFile(rptFilePath, outFilePath);

            // Display results
            outputParser.DisplayResults();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing EPANET output: {ex.Message}");
        }
    }
}