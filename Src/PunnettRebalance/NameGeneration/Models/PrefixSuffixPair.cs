using System.Collections.Generic;
using System.Xml.Serialization;

namespace PunnettRebalance.NameGeneration.Models;

public class PrefixSuffixPair : INameGenerator
{
    [XmlElement(ElementName="prefix")] 
    public List<string>? Prefixes { get; set; }

    [XmlElement(ElementName="suffix")]
    public List<string>? Suffixes { get; set; }

    public string? GetName()
    {
        if(Prefixes == null || Suffixes == null)
            return null;

        var r = Generator.Random.Next(Prefixes.Count);
        var prefix = Prefixes[r];

        r = Generator.Random.Next(Suffixes.Count);
        var suffix = Suffixes[r];

        return prefix + suffix;
    }
}
