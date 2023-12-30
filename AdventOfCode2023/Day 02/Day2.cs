using Xunit.Abstractions;

namespace AdventOfCode2023.Day_02;

public class Day2(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task DemoInput1()
    {
        var data = @"Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green
Game 2: 1 blue, 2 green; 3 green, 4 blue, 1 red; 1 green, 1 blue
Game 3: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red
Game 4: 1 green, 3 red, 6 blue; 3 green, 6 red; 3 green, 15 blue, 14 red
Game 5: 6 red, 1 blue, 3 green; 2 blue, 1 red, 2 green".Split("\n");

        var ruleSet = new Subset("12 red, 13 green, 14 blue");

        Assert.Equal(8, data.Select(x => new Line(x)).Where(x => x.IsPossible(ruleSet)).Select(x => x.GameId).Sum());
    }

    [Fact]
    public async Task DemoInput2()
    {
        var data = @"Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green
Game 2: 1 blue, 2 green; 3 green, 4 blue, 1 red; 1 green, 1 blue
Game 3: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red
Game 4: 1 green, 3 red, 6 blue; 3 green, 6 red; 3 green, 15 blue, 14 red
Game 5: 6 red, 1 blue, 3 green; 2 blue, 1 red, 2 green".Split("\n");

        Assert.Equal(2286, data.Select(x => new Line(x)).Sum(x => x.MinPossible().Power()));
    }

    [Fact]
    public void ParseLineTest()
    {
        var line = new Line("Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green");

        Assert.Equal(1, line.GameId);
        Assert.Equal("blue", line.Subsets[0].CubeSets[0].Color);
        Assert.Equal(3, line.Subsets[0].CubeSets[0].Amount);

        Assert.Equal("green", line.Subsets.Last().CubeSets.Last().Color);
        Assert.Equal(2, line.Subsets.Last().CubeSets.Last().Amount);
    }

    [Fact]
    public async Task Day2Silver()
    {
        var data = await File.ReadAllLinesAsync("Day 02/Day2.txt");

        var ruleSet = new Subset("12 red, 13 green, 14 blue");

        var result = data.Select(x => new Line(x)).Where(x => x.IsPossible(ruleSet)).Select(x => x.GameId).Sum();

        testOutputHelper.WriteLine(result.ToString());
    }

    [Fact]
    public async Task Day2Gold()
    {
        var data = await File.ReadAllLinesAsync("Day 02/Day2.txt");
        testOutputHelper.WriteLine(data.Select(x => new Line(x)).Sum(x => x.MinPossible().Power()).ToString());
    }
}

internal class Line
{
    public Line(string line)
    {
        var payloadSplit = line.Split(":");
        GameId = int.Parse(payloadSplit[0].Split(" ")[1]);
        Subsets = payloadSplit[1].Split(";").Select(x => new Subset(x)).ToList();
    }

    public IReadOnlyList<Subset> Subsets { get; set; }

    public int GameId { get; }

    public bool IsPossible(Subset ruleSet)
    {
        return Subsets.All(x => x.IsPossible(ruleSet));
    }

    public Subset MinPossible()
    {
        return Subsets.Aggregate(new Subset("0 red, 0 green, 0 blue"), (a, b) => a.Merge(b));
    }
}

internal class Subset
{
    public Subset(IReadOnlyList<CubeSet> cubeSets)
    {
        CubeSets = cubeSets;
    }

    public Subset(string s)
    {
        CubeSets = s.Split(",").Select(x => new CubeSet(x)).ToList();
    }

    public IReadOnlyList<CubeSet> CubeSets { get; }

    public bool IsPossible(Subset ruleSet)
    {
        return CubeSets.All(x => x.IsPossible(ruleSet));
    }

    public Subset Merge(Subset minimumSubset)
    {
        return new Subset(CubeSets.Select(x => x.Merge(minimumSubset)).ToList());
    }

    public int Power()
    {
        return CubeSets.Select(x => x.Amount).Aggregate(1, (a, b) => a * b);
    }
}

internal class CubeSet
{
    public CubeSet(string color, int amount)
    {
        Color = color;
        Amount = amount;
    }

    public CubeSet(string s)
    {
        var split = s.Trim().Split(" ");
        Amount = int.Parse(split[0]);
        Color = split[1];
    }

    public string Color { get; }
    public int Amount { get; }

    public bool IsPossible(Subset ruleSet)
    {
        return ruleSet.CubeSets.FirstOrDefault(x => x.Color == Color)?.Amount >= Amount;
    }

    public CubeSet Merge(Subset minimumSubset)
    {
        return new CubeSet(Color,
            Math.Max(minimumSubset.CubeSets.FirstOrDefault(x => x.Color == Color)?.Amount ?? 0, Amount));
    }
}