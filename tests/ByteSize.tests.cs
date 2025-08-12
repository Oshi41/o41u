using System.Linq;
using lib.Model;

namespace tests;

public class ByteSizeTests
{
    [Test]
    public void Constants_HaveCorrectValues()
    {
        Assert.That(ByteSizes.FromB(1).TotalBytes, Is.EqualTo(1));
        Assert.That(ByteSizes.FromKb(1).TotalBytes, Is.EqualTo(1024));
        Assert.That(ByteSizes.FromMb(1).TotalBytes, Is.EqualTo(1024 * 1024));
        Assert.That(ByteSizes.FromGb(1).TotalBytes, Is.EqualTo(1024.0 * 1024 * 1024));
    }

    [Test]
    public void Addition_ReturnsCorrectResult()
    {
        var a = ByteSizes.FromB(100);
        var b = ByteSizes.FromKb(1);
        var result = a + b;

        Assert.That(result.TotalBytes, Is.EqualTo(100 + 1024));
    }

    [Test]
    public void Subtraction_ReturnsCorrectResult()
    {
        var a = ByteSizes.FromKb(2);
        var b = ByteSizes.FromB(100);
        var result = a - b;

        Assert.That(result.TotalBytes, Is.EqualTo(2 * 1024 - 100));
    }

    [Test]
    public void Multiplication_ReturnsCorrectResult()
    {
        var a = ByteSizes.FromKb(2);
        var result = a * 3;

        Assert.That(result.TotalBytes, Is.EqualTo(2 * 3 * 1024));
    }

    [Test]
    public void Comparison_WorksCorrectly()
    {
        var smaller = ByteSizes.FromKb(1);
        var larger = ByteSizes.FromMb(1);

        Assert.That(smaller < larger, Is.True);
        Assert.That(smaller <= larger, Is.True);
        Assert.That(larger > smaller, Is.True);
        Assert.That(larger >= smaller, Is.True);
        Assert.That(smaller == smaller, Is.True);
        Assert.That(smaller != larger, Is.True);
    }

    [Test]
    public void Total_ReturnsCorrectAmounts()
    {
        // 1.5MB
        var size = ByteSizes.FromMb(1.5);

        Assert.That(size.TotalBytes, Is.EqualTo(1.5 * 1024 * 1024));
        Assert.That(size.TotalKb, Is.EqualTo(1.5 * 1024));
        Assert.That(size.TotalMb, Is.EqualTo(1));
        Assert.That(size.TotalGb, Is.EqualTo(0)); // Less than 1GB
    }

    [Test]
    public void Factory_Methods_CreateCorrectSizes()
    {
        Assert.That(ByteSizes.FromB(100).TotalBytes, Is.EqualTo(100));
        Assert.That(ByteSizes.FromKb(2).TotalBytes, Is.EqualTo(2 * 1024));
        Assert.That(ByteSizes.FromMb(3).TotalBytes, Is.EqualTo(3 * 1024 * 1024));
        Assert.That(ByteSizes.FromGb(4).TotalBytes, Is.EqualTo(4.0 * 1024 * 1024 * 1024));
    }

    [Test]
    public void ToString_FormatsCorrectly()
    {
        var size = ByteSizes.FromMb(1) + ByteSizes.FromKb(200);
        var str = size.ToString();

        Assert.That(str, Contains.Substring("1 MB"));
        Assert.That(str, Contains.Substring("200 KB"));
    }
}