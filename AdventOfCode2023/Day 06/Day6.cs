using Xunit.Abstractions;

namespace AdventOfCode2023.Day_06;

public class Day6(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void DemoInputPart1()
    {
        var data = @"Time:      7  15   30
Distance:  9  40  200".Split("\n");

        var result = Day6SilverCalc(data);

        Assert.Equal(288, result);
    }

    [Fact]
    public void DemoInputPart2()
    {
        var data = @"Time:      7  15   30
Distance:  9  40  200".Split("\n");

        var result = Day6GoldCalc(data);

        Assert.Equal(71503, result);
    }

    [Fact]
    public async Task Day6Silver()
    {
        var data = await File.ReadAllLinesAsync("Day 06/Day6.txt");
        var result = Day6SilverCalc(data);
        testOutputHelper.WriteLine(result.ToString());
    }

    [Fact]
    public async Task Day6Gold()
    {
        var data = await File.ReadAllLinesAsync("Day 06/Day6.txt");
        var result = Day6GoldCalc(data);
        testOutputHelper.WriteLine(result.ToString());
    }

    private static int Day6SilverCalc(string[] data)
    {
        var times = data[0].Split(" ", StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(int.Parse).ToList();
        var recordDistance = data[1].Split(" ", StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(int.Parse)
            .ToList();

        var result = times.Zip(recordDistance).Select(x => new Race(x.First, x.Second)).Select(x => x.NumWaysToWin())
            .Aggregate(1, (a, b) => a * b);
        return result;
    }

    private static int Day6GoldCalc(string[] data)
    {
        var times = data[0].Replace(" ", "").Split(":")[1].Trim();
        var recordDistance = data[1].Replace(" ", "").Split(":")[1].Trim();

        var result = new Race(decimal.Parse(times), decimal.Parse(recordDistance)).NumWaysToWin();
        return result;
    }
}

internal class Race(decimal raceTime, decimal recordDistance)
{
    public int NumWaysToWin()
    {
        var rt = (double)raceTime;
        var rd = (double)recordDistance;

        var minBurstTime = rt / 2.0d - Math.Sqrt(Math.Pow(-rt / 2.0d, 2) - rd);
        var minBurstTimeRounded = Math.Ceiling(minBurstTime);
        if (minBurstTime == minBurstTimeRounded) minBurstTimeRounded += 1;
        var minBurstTimeClamped = Math.Max(Math.Ceiling(minBurstTimeRounded), 0);


        var maxBurstTime = rt / 2.0d + Math.Sqrt(Math.Pow(-rt / 2.0d, 2) - rd);
        var maxBurstTimeRounded = Math.Floor(maxBurstTime);
        if (maxBurstTime == maxBurstTimeRounded) maxBurstTimeRounded -= 1;
        var maxBurstTimeClamped = Math.Min(Math.Floor(maxBurstTimeRounded), (double)raceTime);


        var numWaysToWin = maxBurstTimeClamped - minBurstTimeClamped + 1;
        return (int)numWaysToWin;
    }
}