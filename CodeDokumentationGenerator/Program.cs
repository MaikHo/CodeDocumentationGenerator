namespace CodeDokumentationGenerator
{

    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Geben Sie den Pfad zur .cs-Datei ein:");
            string filePath = Console.ReadLine();

            Console.WriteLine("Geben Sie den Namen der Ausgabedatei (.md) ein:");
            string outputFile = Console.ReadLine();

            if (string.IsNullOrEmpty(outputFile))
            {
                Console.WriteLine("Ungültiger Ausgabedateiname.");
                return;
            }

            CodeAnalyzer analyzer = new();
            analyzer.AnalyzeFile(filePath, outputFile);

            Console.WriteLine($"Dokumentation generiert: {outputFile}");
        }
    }
}