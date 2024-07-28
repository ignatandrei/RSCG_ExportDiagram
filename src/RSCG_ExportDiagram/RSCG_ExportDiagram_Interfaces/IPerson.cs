namespace RSCG_ExportDiagram_Interfaces;

public interface IPerson
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }

    public string FullName(); 
    public int Age { get; set; }
}
