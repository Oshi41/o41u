using IL_Weaver;
using lib.Extensions;
using lib.Helpers;
using Directory = lib.Helpers.Directory;
using Is = NUnit.Framework.Is;

namespace tests;

[TestFixture]
public class WeavingTests
{
    [Test]
    public void HasAnyDlls()
    {
        var dlls = Directory.Current.ListFiles("**/*.dll", "**/*.exe");
        Assert.That(dlls, Is.Not.Empty);
    }
    
    [Test]
    public void LoadAotManagedDll()
    {
        var dlls = Directory.Current.ListFiles("**/aot.dll");
        Assert.That(dlls, Is.Not.Empty);

        var weaver = new IlWeaver(dlls.First().FullName);
        Assert.That(weaver.AllTypes, Is.Not.Empty);
    }
}