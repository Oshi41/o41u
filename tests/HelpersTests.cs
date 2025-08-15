using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using lib.Extensions;
using lib.Helpers;

namespace tests;

public class HelpersTests
{
    [Test]
    public void CommonComparer_Compare_NullAndNonNull()
    {
        var cmp = Extensions.CommonComparer;
        Assert.That(cmp.Compare(null, 5), Is.EqualTo(1));
        Assert.That(cmp.Compare(5, null), Is.EqualTo(-1));
        Assert.That(cmp.Compare(5, 5), Is.EqualTo(0));
    }

    [Test]
    public void CommonComparer_Compare_Dictionaries()
    {
        var cmp = Extensions.CommonComparer;
        IDictionary x = new Dictionary<string, int> { {"a", 1}, {"b", 2} };
        IDictionary y = new Dictionary<string, int> { {"a", 1}, {"b", 2} };
        // For equal non-empty dictionaries current implementation returns -1 at the end
        Assert.That(cmp.Compare(x, y), Is.EqualTo(-1));

        IDictionary y2 = new Dictionary<string, int> { {"a", 1} };
        Assert.That(cmp.Compare(x, y2), Is.EqualTo(1)); // count diff => positive

        IDictionary y3 = new Dictionary<string, int> { {"a", 1}, {"c", 2} };
        Assert.That(cmp.Compare(x, y3), Is.Not.EqualTo(0)); // keys differ
    }

    [Test]
    public void CommonComparer_Compare_Dictionary_vs_IEnumerable()
    {
        var cmp = Extensions.CommonComparer;
        IDictionary x = new Dictionary<string, int> { {"a", 1}, {"b", 2} };
        IEnumerable y = new List<string> {"a", "b", "c"};
        // list has one element not in dict keys => Except count = 1
        Assert.That(cmp.Compare(x, y), Is.EqualTo(1));
    }

    [Test]
    public void CommonComparer_Compare_Enumerables()
    {
        var cmp = Extensions.CommonComparer;
        IEnumerable x = new List<int> {1, 2, 3};
        IEnumerable y = new List<int> {3, 2, 1};
        Assert.That(cmp.Compare(x, y), Is.EqualTo(0)); // same set

        IEnumerable y2 = new List<int> {1, 2};
        Assert.That(cmp.Compare(x, y2), Is.EqualTo(1)); // count diff (3-2)

        Assert.That(cmp.Compare(x, 123), Is.EqualTo(-1)); // enumerable vs non-enumerable
    }

    private class Foo {}

    [Test]
    public void CommonComparer_Compare_UnsupportedTypes_ReturnsNonZeroWhenDifferent()
    {
        var cmp = Extensions.CommonComparer;
        var a = new Foo();
        var b = new Foo();
        Assert.That(cmp.Compare(a, b), Is.Not.EqualTo(0));
    }

    [Test]
    public void CommonComparer_Contains_Dictionary_and_Subset()
    {
        var cmp = Extensions.CommonComparer;
        IDictionary x = new Dictionary<string, int> { {"a", 1}, {"b", 2}, {"c", 3} };
        IDictionary y = new Dictionary<string, int> { {"a", 1}, {"b", 2} };
        Assert.That(cmp.Contains(x, y), Is.True);

        IDictionary yWrong = new Dictionary<string, int> { {"a", 9} };
        Assert.That(cmp.Contains(x, yWrong), Is.False);

        IEnumerable keys = new List<string> {"a", "b"};
        Assert.That(cmp.Contains(x, keys), Is.True);
    }

    [Test]
    public void CommonComparer_Contains_Enumerables_And_Single()
    {
        var cmp = Extensions.CommonComparer;
        IEnumerable x = new List<int> {1, 2, 3};
        Assert.That(cmp.Contains(x, 2), Is.True);

        IEnumerable y = new List<int> {2, 3};
        Assert.That(cmp.Contains(x, y), Is.True);

        IEnumerable yBad = new List<int> {2, 4};
        Assert.That(cmp.Contains(x, yBad), Is.False);
    }

    [Test]
    public void CommonComparer_Contains_String()
    {
        var cmp = Extensions.CommonComparer;
        Assert.That(cmp.Contains("hello world", "ell"), Is.True);
        Assert.That(cmp.Contains("hello", null), Is.False);
    }

    [Test]
    public void CommonComparer_CastAdapter_Works()
    {
        var intComparer = Extensions.CommonComparer.Cast<int>();
        Assert.That(intComparer.Compare(10, 5), Is.GreaterThan(0));
        Assert.That(intComparer.Equals(7, 7), Is.True);
        Assert.That(intComparer.Contains(new[] {1,2,3}, 2), Is.True);
    }

    [Test]
    public void Guard_BasicChecks()
    {
        Assert.That(Guard.Null(null).Ok, Is.True);
        Assert.That(Guard.IsEmpty("").Ok, Is.True);
        Assert.That(Guard.IsNotEmpty("abc").Ok, Is.True);

        // Contains via Guard (string)
        Assert.That(Guard.Contains("hello", "ell").Ok, Is.True);

        // Same/Less/More using strings and numbers
        Assert.That(Guard.Same("a", "a").Ok, Is.True);
        Assert.That(Guard.Less(1, 2).Ok, Is.True);
        Assert.That(Guard.More(3, 2).Ok, Is.True);

        // Operators
        // Operators are only accessible via instances, but Guard has protected ctor.
        // We verify logical equivalence via static methods instead.
        Assert.That(Guard.Same("x", "x").Ok, Is.True);
        Assert.Throws<Exception>(() => Guard.Same("x", "y").CheckAndThrow());
        Assert.That(Guard.Less("a", "b").Ok, Is.True);
        Assert.That(Guard.More("b", "a").Ok, Is.True);
    }

    [Test]
    public void GuardResult_CheckAndThrow_And_Conversions()
    {
        GuardResult ok = true;
        Assert.That(ok.Ok, Is.True);
        Assert.That((bool)ok, Is.True);

        GuardResult error = "error";
        Assert.That(error.Ok, Is.False);
    }

    [Test]
    public void Directory_Ensure_CreatesOrReturns()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "o41u_tests", Guid.NewGuid().ToString("N"));
        var di = lib.Helpers.Directory.Ensure(tempRoot);
        Assert.That(di, Is.Not.Null);
        Assert.That(System.IO.Directory.Exists(tempRoot), Is.True);
    }

    [Test]
    public void File_Ensure_CreatesWithDefaultContent()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "o41u_tests", Guid.NewGuid().ToString("N"));
        var filePath = Path.Combine(tempDir, "test.txt");
        var fi = lib.Helpers.File.Ensure(filePath, "abc");
        Assert.That(fi, Is.Not.Null);
        Assert.That(System.IO.File.Exists(fi!.FullName), Is.True);
        Assert.That(System.IO.File.ReadAllText(fi!.FullName), Is.EqualTo("abc"));
    }

    private string InnerMethod() => CallerInfo.GetMemberName(1)!;
    private string OuterMethod()
    {
        return InnerMethod();
    }

    [Test]
    public void CallerInfo_GetMemberName_PreviousFrame()
    {
        var name = OuterMethod();
        Assert.That(name, Is.EqualTo(nameof(OuterMethod)));
    }
}
