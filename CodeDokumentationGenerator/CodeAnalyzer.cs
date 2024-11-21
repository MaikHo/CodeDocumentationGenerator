using System.Text.RegularExpressions;

namespace CodeDokumentationGenerator
{

    public class CodeAnalyzer
    {
        public void AnalyzeFile(string filePath, string outputFile)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Die angegebene Datei existiert nicht.");
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            string content = File.ReadAllText(filePath);

            // Klassen finden
            var classMatches = Regex.Matches(content, @"class\s+(\w+(\<.*\>)?)");
            foreach (Match match in classMatches)
            {
                string className = match.Groups[1].Value;
                Console.WriteLine($"Gefundene Klasse: {className}");
                GenerateClassDocumentation(className, lines, outputFile);
            }
        }

        private void GenerateClassDocumentation(string className, string[] lines, string outputFile)
        {
            List<string> methods = new();
            List<string> properties = new();
            List<string> fields = new();
            List<string> constructors = new();
            List<string> events = new();
            Dictionary<int, string> comments = new();

            // Kommentare sammeln
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("//") || line.StartsWith("///") || line.StartsWith("/*"))
                {
                    comments[i] = line;
                }
            }

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                // Generische Konstruktoren erkennen
                if (Regex.IsMatch(trimmedLine, $@"{className}\s*\<.*\>?\s*\("))
                {
                    constructors.Add(ExtractCommentForLine(comments, lines, trimmedLine) + $"`{trimmedLine}`");
                }

                // Generische Methoden erkennen
                else if (Regex.IsMatch(trimmedLine, @"(public|protected|private|internal)\s+.*\<.*\>\s*\(.*\)\s*(\{|\;)", RegexOptions.IgnoreCase))
                {
                    methods.Add(ExtractCommentForLine(comments, lines, trimmedLine) + $"`{trimmedLine}`");
                }

                // Generische Properties erkennen
                else if (Regex.IsMatch(trimmedLine, @"(public|protected|private|internal)\s+.*\<.*\>\s+\w+\s+\{.*\}", RegexOptions.IgnoreCase))
                {
                    properties.Add(ExtractCommentForLine(comments, lines, trimmedLine) + $"`{trimmedLine}`");
                }

                // Generische Variablen erkennen
                else if (Regex.IsMatch(trimmedLine, @"(public|protected|private|internal)\s+(readonly|static|const)?\s*\w+\<.*\>\s+\w+\s*;", RegexOptions.IgnoreCase))
                {
                    fields.Add(ExtractCommentForLine(comments, lines, trimmedLine) + $"`{trimmedLine}`");
                }

                // Event erkennen
                else if (Regex.IsMatch(trimmedLine, @"event\s+\w+\<.*\>\s+\w+", RegexOptions.IgnoreCase))
                {
                    events.Add(ExtractCommentForLine(comments, lines, trimmedLine) + $"`{trimmedLine}`");
                }
            }

            // Markdown generieren
            using StreamWriter writer = new(outputFile, append: true);
            writer.WriteLine($"# Klasse: {className}");
            writer.WriteLine();

            WriteSection(writer, "Konstruktoren", constructors);
            WriteSection(writer, "Methoden", methods);
            WriteSection(writer, "Properties", properties);
            WriteSection(writer, "Variablen", fields);
            WriteSection(writer, "Events", events);

            writer.WriteLine();
        }

        private void WriteSection(StreamWriter writer, string title, List<string> items)
        {
            writer.WriteLine($"## {title}:");
            if (items.Any())
            {
                foreach (var item in items)
                    writer.WriteLine($"- {item}");
            }
            else
            {
                writer.WriteLine($"Keine {title.ToLower()} gefunden.");
            }
        }

        private string ExtractCommentForLine(Dictionary<int, string> comments, string[] lines, string lineContent)
        {
            int index = Array.FindIndex(lines, line => line.Contains(lineContent));
            if (index < 0) return string.Empty;

            // Suche vorhergehende Kommentare
            List<string> associatedComments = new();
            while (comments.ContainsKey(index - 1))
            {
                associatedComments.Insert(0, comments[index - 1].Trim());
                index--;
            }

            if (associatedComments.Any())
            {
                return $"**Kommentar:** {string.Join(" ", associatedComments)}\n";
            }

            return string.Empty;
        }
    }    
}