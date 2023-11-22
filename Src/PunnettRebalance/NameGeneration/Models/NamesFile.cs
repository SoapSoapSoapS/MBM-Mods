using Newtonsoft.Json;

namespace PunnettRebalance.NameGeneration.Models;

public class NamesFile<T,U>(T names, U surnames)
{
    [JsonProperty("names")]
    public T Names { get; } = names;

    [JsonProperty("surnames")]
    public U Surnames { get; } = surnames;
}