using System.Text;
using Xunit.Abstractions;

namespace AdventOfCode2023.Day_15;

public class Day15(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task DemoInputTest()
    {
        Assert.Equal(0, "rn".GetAocHash());
        Assert.Equal(3, "pc".GetAocHash());
        Assert.Equal(30, "rn=1".GetAocHash());
        Assert.Equal(253, "cm-".GetAocHash());
        Assert.Equal(1320, "rn=1,cm-,qp=3,cm=2,qp-,pc=4,ot=9,ab=5,pc-,pc=6,ot=7".Split(',').Sum(x => x.GetAocHash()));
    }

    [Fact]
    public async Task Day15SilverTest()
    {
        var data = await File.ReadAllLinesAsync("Day 15/Day15.txt");
        testOutputHelper.WriteLine(data[0].Split(',').Sum(x => x.GetAocHash()).ToString());
    }

    [Fact]
    public async Task Day15GoldInputTest()
    {
        var hash = new HashMap();

        foreach (var s in "rn=1,cm-,qp=3,cm=2,qp-,pc=4,ot=9,ab=5,pc-,pc=6,ot=7".Split(',').Select(x => x.Trim()))
        {
            hash.ProcessItem(s);
        }

        Assert.Equal(145, hash.GetSumOfAll());
    }

    [Fact]
    public async Task Day15GoldTest()
    {
        var data = (await File.ReadAllLinesAsync("Day 15/Day15.txt")).First();

        var hash = new HashMap();

        foreach (var s in data.Split(',').Select(x => x.Trim()))
        {
            hash.ProcessItem(s);
        }


        testOutputHelper.WriteLine(hash.GetSumOfAll().ToString());
    }
}

public class HashMap
{
    public IReadOnlyList<LinkedList<string>> _boxes;

    public HashMap()
    {
        var boxes = new List<LinkedList<string>>(256);
        for (int i = 0; i < 256; i++)
        {
            boxes.Add(new LinkedList<string>());
        }
        _boxes = boxes;
    }

    public int GetSumOfAll()
    {
        var sum = 0;

        for (int box = 0; box < 256; box++)
        {
            var slot = 1;
            foreach (var s in _boxes[box])
            {
                var focalLength = GetFocalStrength(s);

                sum += (box + 1) * slot * focalLength;

                slot++;
            }
        }

        return sum;
    }

    public void ProcessItem(string item)
    {
        var label = GetLabel(item);
        var boxIndex = label.GetAocHash();
        var box = _boxes[boxIndex];

        if (item.Contains('='))
        {
            var existingEntry = box.Select(x => new
            {
                label = GetLabel(x),
                entry = x
            }).FirstOrDefault(x => x.label == label);

            if (existingEntry != null)
            {
                var itemNode = box.Find(existingEntry.entry);
                itemNode.ValueRef = item;
                return;
            }

            box.AddLast(item);
            return;
        }

        if (item.Contains('-'))
        {
            var existingEntry = box.Select(x => new
            {
                label = GetLabel(x),
                entry = x
            }).FirstOrDefault(x => x.label == label);
            if (existingEntry != null)
            {
                box.Remove(existingEntry.entry);
                return;
            }
            return;
        }

        throw new NotSupportedException(item);
    }

    private static string GetLabel(string item)
    {
        return new string(item.Where(char.IsAsciiLetter).ToArray());
    }

    private static int GetFocalStrength(string item)
    {
        return int.Parse(new string(item.Where(char.IsNumber).ToArray()));
    }
}

public static class StringExtension
{
    public static int GetAocHash(this string input)
    {
        var result = 0;
        foreach (var c in Encoding.ASCII.GetBytes(input)) result = (result + c) * 17 % 256;
        return result;
    }
}