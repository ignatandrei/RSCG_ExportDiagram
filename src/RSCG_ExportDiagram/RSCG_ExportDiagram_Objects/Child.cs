
using RSCG_ExportDiagram_Interfaces;

namespace RSCG_ExportDiagram_Objects;
internal record Child : IPerson
{
    string IPerson.FirstName { get ; set ; }
    string IPerson.LastName { get ; set ; }
    int IPerson.Age { get  ; set ; }

    string IPerson.FullName()
    {
        return "Child" +(this as IPerson).FirstName;
    }
    string Test()
    {
        return "Child" + (this as IPerson).FirstName;
    }
}
