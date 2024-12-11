using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NsEpanetInpParser
{
    // Base class for network elements
    public abstract class NetworkElement
    {
        public string Id { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    }

    // Specific network element classes
    public class Node : NetworkElement
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class Pipe : NetworkElement
    {
        public string StartNode { get; set; }
        public string EndNode { get; set; }
        public double Length { get; set; }
        public double Diameter { get; set; }
    }

    public class Pump : NetworkElement
    {
        public string StartNode { get; set; }
        public string EndNode { get; set; }
    }

    public class Tank : Node
    {
        public double InitialLevel { get; set; }
        public double MinimumLevel { get; set; }
        public double MaximumLevel { get; set; }
        public double Diameter { get; set; }
    }

    public class Reservoir : Node
    {
        public double TotalHead { get; set; }
    }

    // Parser for EPANET INP file
    public class EpanetInpParser
    {
        public List<Node> Nodes { get; private set; } = new List<Node>();
        public List<Pipe> Pipes { get; private set; } = new List<Pipe>();
        public List<Pump> Pumps { get; private set; } = new List<Pump>();
        public List<Tank> Tanks { get; private set; } = new List<Tank>();
        public List<Reservoir> Reservoirs { get; private set; } = new List<Reservoir>();

        private enum ParseSection
        {
            None,
            Junctions,
            Tanks,
            Reservoirs,
            Pipes,
            Pumps
        }

        public void ParseInpFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("EPANET INP file not found.", filePath);
            }

            var lines = File.ReadAllLines(filePath);
            ParseInpContent(lines);
        }

        private void ParseInpContent(string[] lines)
        {
            ParseSection currentSection = ParseSection.None;

            foreach (var line in lines)
            {
                // Skip comments and empty lines
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith(";"))
                    continue;

                // Detect section headers
                if (line.TrimStart().StartsWith("["))
                {
                    currentSection = ParseSectionHeader(line);
                    continue;
                }

                // Parse content based on current section
                switch (currentSection)
                {
                    case ParseSection.Junctions:
                        ParseJunction(line);
                        break;
                    case ParseSection.Tanks:
                        ParseTank(line);
                        break;
                    case ParseSection.Reservoirs:
                        ParseReservoir(line);
                        break;
                    case ParseSection.Pipes:
                        ParsePipe(line);
                        break;
                    case ParseSection.Pumps:
                        ParsePump(line);
                        break;
                }
            }
        }

        private ParseSection ParseSectionHeader(string line)
        {
            line = line.Trim().ToLower();
            return line switch
            {
                "[junctions]" => ParseSection.Junctions,
                "[tanks]" => ParseSection.Tanks,
                "[reservoirs]" => ParseSection.Reservoirs,
                "[pipes]" => ParseSection.Pipes,
                "[pumps]" => ParseSection.Pumps,
                _ => ParseSection.None
            };
        }

        private void ParseJunction(string line)
        {
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return;

            Nodes.Add(new Node
            {
                Id = parts[0],
                Attributes = CreateAttributeDictionary(parts.Skip(1).ToArray())
            });
        }

        private void ParseTank(string line)
        {
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5) return;

            Tanks.Add(new Tank
            {
                Id = parts[0],
                X = double.Parse(parts[1]),
                Y = double.Parse(parts[2]),
                InitialLevel = double.Parse(parts[3]),
                MinimumLevel = double.Parse(parts[4]),
                MaximumLevel = parts.Length > 5 ? double.Parse(parts[5]) : 0,
                Diameter = parts.Length > 6 ? double.Parse(parts[6]) : 0
            });
        }

        private void ParseReservoir(string line)
        {
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return;

            Reservoirs.Add(new Reservoir
            {
                Id = parts[0],
                X = double.Parse(parts[1]),
                Y = double.Parse(parts[2]),
                TotalHead = parts.Length > 3 ? double.Parse(parts[3]) : 0
            });
        }

        private void ParsePipe(string line)
        {
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5) return;

            Pipes.Add(new Pipe
            {
                Id = parts[0],
                StartNode = parts[1],
                EndNode = parts[2],
                Length = double.Parse(parts[3]),
                Diameter = double.Parse(parts[4]),
                Attributes = CreateAttributeDictionary(parts.Skip(5).ToArray())
            });
        }

        private void ParsePump(string line)
        {
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return;

            Pumps.Add(new Pump
            {
                Id = parts[0],
                StartNode = parts[1],
                EndNode = parts[2],
                Attributes = CreateAttributeDictionary(parts.Skip(3).ToArray())
            });
        }

        private Dictionary<string, string> CreateAttributeDictionary(string[] additionalAttributes)
        {
            var attributes = new Dictionary<string, string>();
            for (int i = 0; i < additionalAttributes.Length; i += 2)
            {
                if (i + 1 < additionalAttributes.Length)
                {
                    attributes[additionalAttributes[i]] = additionalAttributes[i + 1];
                }
            }
            return attributes;
        }

        // Validation method
        public bool Validate()
        {
            return Nodes.Count > 0 &&
                   Pipes.Count > 0 &&
                   (Tanks.Count > 0 || Reservoirs.Count > 0);
        }

        // Export method to show parsed data
        public void PrintSummary()
        {
            Console.WriteLine($"Nodes: {Nodes.Count}");
            Console.WriteLine($"Pipes: {Pipes.Count}");
            Console.WriteLine($"Pumps: {Pumps.Count}");
            Console.WriteLine($"Tanks: {Tanks.Count}");
            Console.WriteLine($"Reservoirs: {Reservoirs.Count}");
        }
    }

   
}