using Xunit.Abstractions;

namespace AdventOfCode2023.Day_03;

public class Day3(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void DemoInputPart1()
    {
        var data = @"467..114..
...*......
..35..633.
......#...
617*......
.....+.58.
..592.....
......755.
...$.*....
.664.598..".Split("\n").Select(x => x.Trim()).ToArray();

        var result = Day3SilverCalc(data);
        Assert.Equal(4361, result);
    }

    [Fact]
    public void DemoInputPart2()
    {
        var data = @"467..114..
...*......
..35..633.
......#...
617*......
.....+.58.
..592.....
......755.
...$.*....
.664.598..".Split("\n").Select(x => x.Trim()).ToArray();

        var result = Day3GoldCalc(data);
        Assert.Equal(467835, result);
    }

    [Fact]
    public async Task Day3Silver()
    {
        var data = await File.ReadAllLinesAsync("Day 03/Day3.txt");
        var result = Day3SilverCalc(data);
        testOutputHelper.WriteLine(result.ToString());
    }

    [Fact]
    public async Task Day3Gold()
    {
        var data = await File.ReadAllLinesAsync("Day 03/Day3.txt");
        var result = Day3GoldCalc(data);
        testOutputHelper.WriteLine(result.ToString());
    }

    private decimal Day3SilverCalc(string[] data)
    {
        var map = new EngineSchematic(data);

        return map._numbers.Where(x => map._symbols.Any(y => x.IsAdjacentTo(y.Coordinate))).Sum(x => x.Value);
    }

    private decimal Day3GoldCalc(string[] data)
    {
        var map = new EngineSchematic(data);
        return map._symbols.Where(x => x.IsStar()).Select(x =>
            {
                return new
                {
                    symbol = x,
                    neighbouhrs = map._numbers.Where(y => y.IsAdjacentTo(x.Coordinate))
                };
            }).Where(x => x.neighbouhrs.Count() == 2)
            .Select(x => x.neighbouhrs.Aggregate(1, (i, number) => i * number.Value))
            .Sum();
    }
}

internal class Coordinate(int x, int y)
{
    public int X { get; } = x;
    public int Y { get; } = y;
}

internal class EngineSchematic
{
    private readonly string[] _data;
    private readonly int _height;
    public readonly IReadOnlyList<EngineNumber> _numbers;
    public readonly IReadOnlyList<Symbol> _symbols;
    private readonly int _width;

    public EngineSchematic(string[] data)
    {
        _data = data;
        _width = data[0].Length;
        _height = data.Length;

        _numbers = FindAllEngineNumbers().ToList();
        _symbols = FindAllSymbols().ToList();
    }

    private IEnumerable<Symbol> FindAllSymbols()
    {
        for (var y = 0; y < _height; y++)
        for (var x = 0; x < _width; x++)
        {
            var coordinate = new Coordinate(x, y);
            var tile = GetTileForCoordinate(coordinate);
            if (tile.IsSymbol()) yield return new Symbol(coordinate, tile.TileChar);
        }
    }

    private IEnumerable<EngineNumber> FindAllEngineNumbers()
    {
        string? currentNumberString = null;
        Coordinate? startCoordinate = null;


        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                var coordinate = new Coordinate(x, y);
                var tile = GetTileForCoordinate(coordinate);

                if (tile.IsPartOfNumber())
                {
                    if (currentNumberString == null)
                    {
                        startCoordinate = coordinate;
                        currentNumberString = $"{GetCharForCoordinate(coordinate)}";
                        continue;
                    }

                    currentNumberString += GetCharForCoordinate(coordinate);
                    continue;
                }

                if (currentNumberString != null)
                {
                    yield return new EngineNumber(startCoordinate!, new Coordinate(coordinate.X - 1, coordinate.Y),
                        int.Parse(currentNumberString));
                    currentNumberString = null;
                    startCoordinate = null;
                }
            }

            if (currentNumberString != null)
            {
                yield return new EngineNumber(startCoordinate!, new Coordinate(_width - 1, y),
                    int.Parse(currentNumberString));
                currentNumberString = null;
                startCoordinate = null;
            }
        }
    }

    private Tile GetTileForCoordinate(Coordinate coordinate)
    {
        return new Tile(GetCharForCoordinate(coordinate));
    }

    private char GetCharForCoordinate(Coordinate coordinate)
    {
        try
        {
            return _data[coordinate.Y][coordinate.X];
        }
        catch
        {
            return '\0';
        }
    }
}

internal class Tile(char tileChar)
{
    public char TileChar { get; } = tileChar;

    public bool IsPartOfNumber()
    {
        return char.IsNumber(tileChar);
    }

    public bool IsEmpty()
    {
        return tileChar == '.';
    }

    public bool IsSymbol()
    {
        return !IsPartOfNumber() && !IsEmpty();
    }
}

internal class EngineNumber(Coordinate from, Coordinate to, int value)
{
    public Coordinate From { get; } = from;
    public Coordinate To { get; } = to;
    public int Value { get; } = value;

    public bool IsAdjacentTo(Coordinate coordinate)
    {
        return From.X - 1 <= coordinate.X && coordinate.X <= To.X + 1 && Math.Abs(From.Y - coordinate.Y) <= 1;
    }
}

internal class Symbol(Coordinate coordinate, char character)
{
    public Coordinate Coordinate { get; } = coordinate;

    public bool IsStar()
    {
        return character == '*';
    }
}