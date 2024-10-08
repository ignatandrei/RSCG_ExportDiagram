﻿using RSCG_ExportDiagram_Interfaces;

namespace RSCG_ExportDiagram_Objects;

public class Person : IPerson, IDebug
{
    public Person()
    {
        FirstName = "";
        LastName = "";
    }    
    private string Test()
    {
        return "";
    }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    int IPerson.Age { get ; set ; }

    public string FullName() => $"{FirstName} {LastName}";
    public void WriteToFile(string nameFile)
    {
        //this is a comment
        File.WriteAllText(nameFile, FullName());
    }
    public void DebugData()
    {
        var x = $"Debugging {FullName}";
        var MyAge=(this as IPerson).Age;
        x+= MyAge;
        //Console.WriteLine($"Debugging {FullName}");
    }
}
