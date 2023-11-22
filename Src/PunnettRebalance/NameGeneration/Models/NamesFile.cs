using System.IO;
using System.Xml.Serialization;

namespace PunnettRebalance.NameGeneration.Models;

[XmlRoot(ElementName = "root")]
public class NamesFile<T, U>
    where T : INameGenerator
    where U : INameGenerator
{
    [XmlElement(ElementName = "names")]
    public T? Names { get; set; }

    [XmlElement(ElementName = "surnames")]
    public U? Surnames { get; set; }

    public static NamesFile<T, U> ParseXml(string file)
    {
        var xmls = new XmlSerializer(typeof(NamesFile<T, U>));

        using TextReader reader = new StringReader(file);

        return (NamesFile<T, U>) xmls.Deserialize(reader);
    }

    public string? GetFullName(string? surname = null)
    {
        if (Names == null)
            return null;

        if (surname != null && surname != string.Empty)
        {
            var beginLastName = surname.LastIndexOf(' ') + 1;

            if (beginLastName < surname.Length)
            {
                surname = surname.Substring(beginLastName);
            }
        }

        if (surname == null || surname == string.Empty)
        {
            surname = Surnames?.GetName();
        }

        if(surname == null)
            return Names.GetName();

        return Names.GetName() + " " + surname;
    }
}
