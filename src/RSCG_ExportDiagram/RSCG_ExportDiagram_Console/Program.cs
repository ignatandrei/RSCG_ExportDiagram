using RSCG_ExportDiagram_Interfaces;
using RSCG_ExportDiagram_Objects;
public partial class Program
{
    static void Main()
    {
        var s =int.Parse("0");
        var s2 = int.Parse("0");

        File.WriteAllText("asd", "asd");
        IPerson p = new Person();
        p.FirstName = "";
        p.FirstName = "Andrei";
        p.LastName = "Ignat";

        Console.WriteLine($"Hello, {p.FullName()}");
        List<IPerson> list = new();
        list.Add(p);
        p = list.First();

    }
}

