namespace RSCG_ExportDiagram;
public class ExportPublicClass
{
    public string Name { get; set; } = "";
    public MethodPublic[] PublicMethods { get; set; } = [];

}
public class MethodPublic
{
    public string MethodName { get; set; } = "";
}
