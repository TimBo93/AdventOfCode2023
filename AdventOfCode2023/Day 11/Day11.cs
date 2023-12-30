using Xunit.Abstractions;

namespace AdventOfCode2023.Day_11;

public class Day11(ITestOutputHelper testOutputHelper)
{
    private readonly string _sampleInput = @"...#......
.......#..
#.........
..........
......#...
.#........
.........#
..........
.......#..
#...#.....";

    [Fact]
    public void DemoInputPart1Example1()
    {
        Assert.Equal(374, Day11SilverCalc(_sampleInput.Split("\n")));
    }

    [Fact]
    public void DemoInputPart2()
    {
        Assert.Equal(374, Day11GoldCalc(_sampleInput.Split("\n"), 2));
        Assert.Equal(1030, Day11GoldCalc(_sampleInput.Split("\n"), 10));
        Assert.Equal(8410, Day11GoldCalc(_sampleInput.Split("\n"), 100));
    }


    [Fact]
    public async Task Day11Silver()
    {
        var data = await File.ReadAllLinesAsync("Day 11/Day11.txt");
        testOutputHelper.WriteLine(Day11SilverCalc(data).ToString());
    }


    [Fact]
    public async Task Day11Gold()
    {
        var data = await File.ReadAllLinesAsync("Day 11/Day11.txt");
        testOutputHelper.WriteLine(Day11GoldCalc(data, 1_000_000).ToString());
    }

    private decimal Day11SilverCalc(string[] data)
    {
        var map = new Map(data);
        var galaxies = map.GetAllGalaxies(2);

        decimal sum = 0;
        for (var i = 0; i < galaxies.Count - 1; i++)
        for (var ii = i; ii < galaxies.Count; ii++)
            sum += galaxies[i].GetDistanceTo(galaxies[ii]);
        return sum;
    }

    private decimal Day11GoldCalc(string[] data, int distanceToApply)
    {
        var map = new Map(data);
        var galaxies = map.GetAllGalaxies(distanceToApply);

        decimal sum = 0;
        for (var i = 0; i < galaxies.Count - 1; i++)
        for (var ii = i; ii < galaxies.Count; ii++)
            sum += galaxies[i].GetDistanceTo(galaxies[ii]);
        return sum;
    }
}

internal class Map
{
    private readonly IReadOnlyList<int> _columnsWithoutGalaxy;
    private readonly string[] _data;
    private readonly int _height;
    private readonly IReadOnlyList<int> _rowsWithoutGalaxy;

    private readonly int _width;

    public Map(string[] data)
    {
        _data = data;
        _width = data[0].Trim().Length;
        _height = data.Length;

        _columnsWithoutGalaxy = GetAllColumnsWithoutGalaxies().ToList();
        _rowsWithoutGalaxy = GetAllRowsWithoutGalaxies().ToList();
    }

    private IEnumerable<int> GetAllColumnsWithoutGalaxies()
    {
        for (var column = 0; column < _width; column++)
            if (IsColumnEmpty(column))
                yield return column;
    }

    private IEnumerable<int> GetAllRowsWithoutGalaxies()
    {
        for (var row = 0; row < _height; row++)
            if (IsRowEmpty(row))
                yield return row;
    }

    public IReadOnlyList<Galaxy> GetAllGalaxies(int distanceToApply)
    {
        var result = new List<Galaxy>();

        for (var row = 0; row < _height; row++)
        for (var column = 0; column < _width; column++)
            if (!IsEmpty(column, row))
                result.Add(new Galaxy(column + _columnsWithoutGalaxy.Count(x => x < column) * (distanceToApply - 1),
                    row + _rowsWithoutGalaxy.Count(x => x < row) * (distanceToApply - 1)));


        return result;
    }

    private bool IsColumnEmpty(int column)
    {
        for (var row = 0; row < _height; row++)
            if (!IsEmpty(column, row))
                return false;
        return true;
    }

    private bool IsRowEmpty(int row)
    {
        for (var column = 0; column < _width; column++)
            if (!IsEmpty(column, row))
                return false;
        return true;
    }

    private bool IsEmpty(int x, int y)
    {
        return _data[y][x] == '.';
    }
}

internal class Galaxy(int x, int y)
{
    public int X { get; } = x;
    public int Y { get; } = y;

    public int GetDistanceTo(Galaxy galaxy)
    {
        return Math.Abs(x - galaxy.X) + Math.Abs(y - galaxy.Y);
    }
}