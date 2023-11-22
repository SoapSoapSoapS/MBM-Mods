using System;

namespace PunnettRebalance.NameGeneration.Models;

[Serializable]
public class NamesFile<T, U>(T names, U surnames)
    where T : INameGenerator
    where U : INameGenerator
{
    public T names { get; } = names;

    public U surnames { get; } = surnames;

    public string? GetFullName(string? surname = null)
    {
        if(surname != null && surname != string.Empty)
        {
            var beginLastName = surname.LastIndexOf(' ') + 1;

            if(beginLastName < surname.Length)
            {
                surname = surname.Substring(beginLastName);
            }

        }

        if(surname == null || surname == string.Empty)
        {
            surname = surnames.GetName();
        }

        return names.GetName() + " " + surname;
    }
}
