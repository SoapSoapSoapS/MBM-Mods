using System.Collections.Generic;

namespace PunnettRebalance.NameGeneration.Models;

public class NameList : List<string>, INameGenerator
{
    public string GetName()
    {
        var r = Generator.Random.Next(Count);
        return this[r];
    }
}