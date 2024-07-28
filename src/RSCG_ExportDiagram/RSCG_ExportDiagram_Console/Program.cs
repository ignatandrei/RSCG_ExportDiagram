using RSCG_ExportDiagram_Interfaces;
using RSCG_ExportDiagram_Objects;
public partial class Program
{
    static void Main()
    {
        IPerson p = new Person();
        p.FirstName = "";
        p.FirstName = "Andrei";
        p.LastName = "Ignat";

        Console.WriteLine($"Hello, {p.FullName()}");
    }
}

