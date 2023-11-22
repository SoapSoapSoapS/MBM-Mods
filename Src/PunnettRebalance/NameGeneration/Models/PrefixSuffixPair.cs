using System.Collections.Generic;
using Newtonsoft.Json;

namespace PunnettRebalance.NameGeneration.Models;

public class PrefixSuffixPair(List<string> prefixes, List<string> suffixes) : INameGenerator
{
    [JsonProperty("prefix")]
    public List<string> Prefixes { get; } = prefixes;

    [JsonProperty("suffix")]
    public List<string> Suffixes { get; } = suffixes;

    public string GetName()
    {
        var r = Generator.Random.Next(Prefixes.Count);
        var prefix = Prefixes[r];

        r = Generator.Random.Next(Suffixes.Count);
        var suffix = Suffixes[r];

        return prefix + suffix;
    }
}
