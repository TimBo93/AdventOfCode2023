using Xunit.Abstractions;

namespace AdventOfCode2023.Day_09;

public class Day9(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void DemoInputPart1Example1()
    {
        Assert.Equal(18, Extrapolate("0   3   6   9  12  15"));
    }

    [Fact]
    public void DemoInputPart1Example2()
    {
        Assert.Equal(28, Extrapolate("1   3   6  10  15  21"));
    }

    [Fact]
    public void DemoInputPart1Example3()
    {
        Assert.Equal(68, Extrapolate("10  13  16  21  30  45"));
    }


    [Fact]
    public void DemoInputPart2Example1()
    {
        Assert.Equal(5, ExtrapolateBackwards("10  13  16  21  30  45"));
    }


    private int Extrapolate(string s)
    {
        var series = s.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
        return ExtrapolateNextItem(series);
    }

    private int ExtrapolateNextItem(List<int> series)
    {
        if (series.TrueForAll(x => x == 0)) return 0;

        var reducedSeries = ReduceSeries(series).ToList();
        var nextItem = ExtrapolateNextItem(reducedSeries);
        return series.Last() + nextItem;
    }

    private int ExtrapolateBackwards(string s)
    {
        var series = s.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
        return ExtrapolatePrevItem(series);
    }

    private int ExtrapolatePrevItem(List<int> series)
    {
        if (series.TrueForAll(x => x == 0)) return 0;

        var reducedSeries = ReduceSeries(series).ToList();
        var prevItem = ExtrapolatePrevItem(reducedSeries);
        return series.First() - prevItem;
    }

    private IEnumerable<int> ReduceSeries(List<int> series)
    {
        for (var i = 0; i < series.Count - 1; i++) yield return series[i + 1] - series[i];
    }

    [Fact]
    public async Task Day9Silver()
    {
        var data = await File.ReadAllLinesAsync("Day 09/Day9.txt");
        var result = data.Sum(Extrapolate);
        testOutputHelper.WriteLine(result.ToString());
    }

    [Fact]
    public async Task Day9Gold()
    {
        var data = await File.ReadAllLinesAsync("Day 09/Day9.txt");
        var result = data.Sum(ExtrapolateBackwards);
        testOutputHelper.WriteLine(result.ToString());
    }

}