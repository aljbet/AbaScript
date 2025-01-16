using AbaScript.Models;

namespace AbaScript.Helpers;

public class VariableFinder
{
    public IHaveName? FindByShortName(string shortName, IList<IHaveName> variables)
    {
        var matches = variables.Where(v => v.GetName().StartsWith(shortName)).ToList();
        return matches.Count == 1 ? matches.First() : null;
    }
}