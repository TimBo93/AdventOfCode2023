using Xunit.Abstractions;

namespace AdventOfCode2023.Day_05;

public class Day4(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void DemoInputPart1()
    {
        var data = @"seeds: 79 14 55 13

seed-to-soil map:
50 98 2
52 50 48

soil-to-fertilizer map:
0 15 37
37 52 2
39 0 15

fertilizer-to-water map:
49 53 8
0 11 42
42 0 7
57 7 4

water-to-light map:
88 18 7
18 25 70

light-to-temperature map:
45 77 23
81 45 19
68 64 13

temperature-to-humidity map:
0 69 1
1 0 69

humidity-to-location map:
60 56 37
56 93 4".Split("\n");

        var result = Day5SilverCalc(data);


        Assert.Equal(35, result);
    }

    [Fact]
    public async Task Day5Silver()
    {
        var data = await File.ReadAllLinesAsync("Day 05/Day5.txt");
        var result = Day5SilverCalc(data);
        testOutputHelper.WriteLine(result.ToString());
    }

    private decimal Day5SilverCalc(string[] data)
    {
        var maps = Maps.FromData(data);
        return maps.Seeds.Select(maps.Project).Min();
    }
}

class Maps(IReadOnlyList<Map> maps, IReadOnlyList<decimal> seeds)
{
    public IReadOnlyList<decimal> Seeds { get; } = seeds;

    public static Maps FromData(string[] data)
    {
        var maps = new List<Map>();
        var seeds = new List<decimal>();

        var currentMapName = "";
        var currentRanges = new List<Range>();
        foreach (var line in data.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            if (line.StartsWith("seeds:"))
            {
                seeds = line.Split(":")[1].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse)
                    .ToList();
                continue;
            }

            if (line.Contains(":"))
            {
                if (currentMapName != "")
                {
                    maps.Add(new Map(currentMapName, currentRanges));
                }

                currentMapName = line.Split(" ")[0];
                currentRanges = new List<Range>();
                continue;
            }

            currentRanges.Add(Range.FromLine(line));
        }

        maps.Add(new Map(currentMapName, currentRanges));

        return new Maps(maps, seeds);
    }

    public decimal Project(decimal source)
    {
        return maps.Aggregate(source, (currentSource, map) => map.Project(currentSource));
    }
}

class Map(string name, IReadOnlyList<Range> projections)
{
    public decimal Project(decimal source)
    {
        var projection = projections.FirstOrDefault(x => x.IsInRange(source));
        return projection?.Project(source) ?? source;
    }
}

class Range(decimal destinationRangeStart, decimal sourceRangeStart, decimal rangeLength)
{
    public decimal Project(decimal source)
    {
        if (!IsInRange(source))
        {
            return source;
        }
        return source - sourceRangeStart + destinationRangeStart;
    }

    public bool IsInRange(decimal source)
    {
        return sourceRangeStart <= source && source < sourceRangeStart + rangeLength;
    }

    public static Range FromLine(string line)
    {
        var split = line.Split(" ");
        return new Range(decimal.Parse(split[0]), decimal.Parse(split[1]), decimal.Parse(split[2]));
    }
}