namespace RSCG_ExportDiagram;
public class ExportPublicClass
{
    public string Name { get; set; } = "";
    public MethodPublic[] PublicMethods { get; set; } = [];

    public long LinesOfCode { get; set; } = 0;

}
public class MethodPublic
{
    public string MethodName { get; set; } = "";
    public long LinesOfCode { get; set; } = 0;
}
