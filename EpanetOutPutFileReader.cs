using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EpanetOutputReader
{
    // Enum to represent different result types
    public enum ResultType
    {
        Node,
        Link,
        SystemWide
    }

    // Base class for network element results
    public abstract class NetworkElementResult
    {
        public string Id { get; set; }
        public Dictionary<string, double> ResultValues { get; set; } = new Dictionary<string, double>();
    }

    // Specific result classes
    public class NodeResult : NetworkElementResult
    {
        public double Elevation { get; set; }
        public double Demand { get; set; }
        public double Head { get; set; }
        public double Pressure { get; set; }
    }

    public class LinkResult : NetworkElementResult
    {
        public double Flow { get; set; }
        public double Velocity { get; set; }
        public double HeadLoss { get; set; }
        public double Status { get; set; }
    }

    public class SystemWideResult : NetworkElementResult
    {
        public double TotalDemand { get; set; }
        public double AverageEfficiency { get; set; }
        public double TotalEnergyConsumption { get; set; }
    }

    // EPANET Output File Parser
    public class EpanetOutputParser
    {
        public List<NodeResult> NodeResults { get; private set; } = new List<NodeResult>();
        public List<LinkResult> LinkResults { get; private set; } = new List<LinkResult>();
        public SystemWideResult SystemResult { get; private set; }

        // Main parsing method
        public void ParseOutputFile(string rptFilePath, string outFilePath)
        {
            if (!File.Exists(rptFilePath) || !File.Exists(outFilePath))
            {
                throw new FileNotFoundException("EPANET output or report file not found.");
            }

            // Parse RPT file for textual results
            ParseReportFile(rptFilePath);

            // Parse OUT file for detailed simulation results
            ParseBinaryOutputFile(outFilePath);
        }

        // Parse RPT file (text-based report)
        private void ParseReportFile(string rptFilePath)
        {
            var lines = File.ReadAllLines(rptFilePath);
            var currentSection = "";

            foreach (var line in lines)
            {
                // Detect section headers
                if (line.Trim().StartsWith("**"))
                {
                    currentSection = line.Trim();
                    continue;
                }

                // Parse different sections
                if (currentSection.Contains("Node Results"))
                {
                    ParseNodeResultLine(line);
                }
                else if (currentSection.Contains("Link Results"))
                {
                    ParseLinkResultLine(line);
                }
                else if (currentSection.Contains("System Wide"))
                {
                    ParseSystemWideLine(line);
                }
            }
        }

        // Parse binary OUT file (requires understanding EPANET's binary format)
        private void ParseBinaryOutputFile(string outFilePath)
        {
            using (var fileStream = new FileStream(outFilePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fileStream, Encoding.ASCII))
            {
                // Read magic number and version
                int magicNumber = reader.ReadInt32();
                int version = reader.ReadInt32();

                // Validate file
                if (magicNumber != 516114521) // EPANET specific magic number
                {
                    throw new InvalidOperationException("Invalid EPANET output file format.");
                }

                // Read simulation parameters
                int numPeriods = reader.ReadInt32();
                int numNodes = reader.ReadInt32();
                int numLinks = reader.ReadInt32();

                // Read time parameters
                double reportStart = reader.ReadSingle();
                double reportStep = reader.ReadSingle();

                // Parse results for each time period
                for (int period = 0; period < numPeriods; period++)
                {
                    ParseTimePeriodResults(reader, numNodes, numLinks, period);
                }
            }
        }

        private void ParseTimePeriodResults(BinaryReader reader, int numNodes, int numLinks, int period)
        {
            // Nodes results
            for (int i = 0; i < numNodes; i++)
            {
                var nodeResult = new NodeResult
                {
                    Id = $"Node_{i}",
                    Head = reader.ReadSingle(),
                    Pressure = reader.ReadSingle(),
                    Demand = reader.ReadSingle()
                };
                NodeResults.Add(nodeResult);
            }

            // Links results
            for (int i = 0; i < numLinks; i++)
            {
                var linkResult = new LinkResult
                {
                    Id = $"Link_{i}",
                    Flow = reader.ReadSingle(),
                    Velocity = reader.ReadSingle(),
                    HeadLoss = reader.ReadSingle(),
                    Status = reader.ReadSingle()
                };
                LinkResults.Add(linkResult);
            }
        }

        // Parsing helper methods for RPT file
        private void ParseNodeResultLine(string line)
        {
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5) return;

            try
            {
                var nodeResult = new NodeResult
                {
                    Id = parts[0],
                    Elevation = double.Parse(parts[1]),
                    Demand = double.Parse(parts[2]),
                    Head = double.Parse(parts[3]),
                    Pressure = double.Parse(parts[4])
                };
                NodeResults.Add(nodeResult);
            }
            catch (Exception)
            {
                // Silent catch or log parsing errors
            }
        }

        private void ParseLinkResultLine(string line)
        {
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5) return;

            try
            {
                var linkResult = new LinkResult
                {
                    Id = parts[0],
                    Flow = double.Parse(parts[1]),
                    Velocity = double.Parse(parts[2]),
                    HeadLoss = double.Parse(parts[3]),
                    Status = double.Parse(parts[4])
                };
                LinkResults.Add(linkResult);
            }
            catch (Exception)
            {
                // Silent catch or log parsing errors
            }
        }

        private void ParseSystemWideLine(string line)
        {
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4) return;

            try
            {
                SystemResult = new SystemWideResult
                {
                    TotalDemand = double.Parse(parts[1]),
                    AverageEfficiency = double.Parse(parts[2]),
                    TotalEnergyConsumption = double.Parse(parts[3])
                };
            }
            catch (Exception)
            {
                // Silent catch or log parsing errors
            }
        }

        // Result visualization method
        public void DisplayResults()
        {
            Console.WriteLine("EPANET Simulation Results");
            Console.WriteLine("========================");

            // Display Node Results
            Console.WriteLine("\nNode Results:");
            foreach (var node in NodeResults)
            {
                Console.WriteLine($"Node ID: {node.Id}");
                Console.WriteLine($"  Elevation: {node.Elevation:F2}");
                Console.WriteLine($"  Demand: {node.Demand:F2}");
                Console.WriteLine($"  Head: {node.Head:F2}");
                Console.WriteLine($"  Pressure: {node.Pressure:F2}");
            }

            // Display Link Results
            Console.WriteLine("\nLink Results:");
            foreach (var link in LinkResults)
            {
                Console.WriteLine($"Link ID: {link.Id}");
                Console.WriteLine($"  Flow: {link.Flow:F2}");
                Console.WriteLine($"  Velocity: {link.Velocity:F2}");
                Console.WriteLine($"  Head Loss: {link.HeadLoss:F2}");
                Console.WriteLine($"  Status: {link.Status:F2}");
            }

            // Display System-Wide Results
            if (SystemResult != null)
            {
                Console.WriteLine("\nSystem-Wide Results:");
                Console.WriteLine($"Total Demand: {SystemResult.TotalDemand:F2}");
                Console.WriteLine($"Average Efficiency: {SystemResult.AverageEfficiency:F2}");
                Console.WriteLine($"Total Energy Consumption: {SystemResult.TotalEnergyConsumption:F2}");
            }
        }
    }
}