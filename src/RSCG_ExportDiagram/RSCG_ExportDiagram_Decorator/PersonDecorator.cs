using RSCG_ExportDiagram_Interfaces;
using RSCG_ExportDiagram_Objects;
using System;

namespace RSCG_ExportDiagram_Decorator;

public class PersonDecorator :IPerson
{
    private IPerson _person;
    public PersonDecorator(IPerson person)
    {
        _person = person;
    }
    public string FirstName { get => _person.FirstName; set => _person.FirstName = value; }
    public string LastName { get => _person.LastName; set => _person.LastName = value; }
    public string FullName() => _person.FullName();
    public int Age { get => _person.Age; set => _person.Age = value; }

    public void DebugData(string nameFile)
    {
        var p=new Person();
        p.WriteToFile(nameFile);

    }
    public void AnotherDebug(string nameFile)
    {
        var s = _person.Age;

    }
}



