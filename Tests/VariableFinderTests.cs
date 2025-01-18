using AbaScript.Helpers;
using AbaScript.Models;

namespace Tests;

public class VariableFinderTests
{
    private VariableFinder _variableFinder;
    private List<IHaveName> _variables;

    [SetUp]
    public void Setup()
    {
        _variableFinder = new VariableFinder();
        _variables = new List<IHaveName>
        {
            new Variable("abas"),
            new Variable("baba"),
            new Variable("alpha"),
            new Variable("beta")
        };
    }

    [Test]
    public void FindByShortName_ShouldReturnVariable_WhenUniqueMatchExists()
    {
        var result = _variableFinder.FindByShortName("ab", _variables);
        Assert.IsNotNull(result);
        Assert.That(result.GetName(), Is.EqualTo("abas"));
    }

    [Test]
    public void FindByShortName_ShouldReturnNull_WhenNoMatchExists()
    {
        var result = _variableFinder.FindByShortName("z", _variables);
        Assert.IsNull(result);
    }

    [Test]
    public void FindByShortName_ShouldReturnNull_WhenMultipleMatchesExist()
    {
        var result = _variableFinder.FindByShortName("b", _variables);
        Assert.IsNull(result);
    }
}