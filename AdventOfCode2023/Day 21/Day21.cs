using System.IO.MemoryMappedFiles;
using AdventOfCode2023.Day_02;
using Xunit.Abstractions;

namespace AdventOfCode2023.Day_21;

public class Day21(ITestOutputHelper testOutputHelper)
{
    private readonly string _demoInput = @"...........
.....###.#.
.###.##..#.
..#.#...#..
....#.#....
.##..S####.
.##..#...#.
.......##..
.##.#.####.
.##..##.##.
...........";


    [Fact]
    public async Task DemoInputTest()
    {
        var map = new Map(_demoInput.Split("\n"));
        var distanceCalculator= new DistanceCalculator();

        var positionsWithMaxDistance6 = distanceCalculator.GetMinDistancesToStart(map, 6);
        var reachablePositions6 = positionsWithMaxDistance6.Count(x => x.Value % 2 == 0);
        Assert.Equal(16, reachablePositions6);
    }

    [Fact]
    public async Task SilverTest()
    {
        var data = await File.ReadAllLinesAsync("Day 21/Day21.txt");
        var map = new Map(data);
        var distanceCalculator = new DistanceCalculator();

        var positionsWithMaxDistance64 = distanceCalculator.GetMinDistancesToStart(map, 64);
        var reachablePositions64 = positionsWithMaxDistance64.Count(x => x.Value % 2 == 0);
        testOutputHelper.WriteLine(reachablePositions64.ToString());
    }

    [Theory]
    [InlineData(6, 16)]
    [InlineData(10, 50)]
    [InlineData(50, 1594)]
    [InlineData(100, 6536)]
    [InlineData(500, 167004)]
    [InlineData(1000, 668697)]
    [InlineData(5000, 16733044)]
    public async Task DemoInputTestPart2(int maxSteps, int expectedPlots)
    {
        var data = _demoInput.Split("\n");
        var map = new Map(data);

        var distanceCalculator = new DistanceCalculator();

        var positionsWithMaxDistance = distanceCalculator.GetMinDistancesToStart(map, maxSteps).Count(x => (x.Value % 2) == (maxSteps % 2));
        Assert.Equal(expectedPlots, positionsWithMaxDistance);
    }

    [Fact]
    public async Task ExperimentPart2()
    {
        var data = await File.ReadAllLinesAsync("Day 21/Day21.txt");
        var map = new Map(data);

        var distanceCalculator = new DistanceCalculator();

        for (int i = 0; i < 13; i++)
        {
            var maxSteps = 131 * i + 65;
            testOutputHelper.WriteLine($"{maxSteps}: {distanceCalculator.GetMinDistancesToStart(map, maxSteps).Count(x => (x.Value % 2) == (maxSteps % 2))}");
        }
    }
}

internal class DistanceCalculator
{
    public Dictionary<Position, int> GetMinDistancesToStart(Map map, int maxDistanceToStart)
    {
        var startposition = map.StartPosition;

        Dictionary<Position, int> minDistances = new Dictionary<Position, int> {{startposition, 0}};
        List<HashSet<Position>> tilesToExpand = [[startposition]];

        for (int distance = 1; distance <= maxDistanceToStart; distance++)
        {
            var previousItems = tilesToExpand[distance - 1];
            if (previousItems.Count == 0)
            {
                break;
            }

            var newItems = new HashSet<Position>();
            tilesToExpand.Add(newItems);
            foreach (var previousItem in previousItems)
            {
                var top = previousItem.Top;
                var bottom = previousItem.Bottom;
                var left = previousItem.Left;
                var right = previousItem.Right;

                foreach (var pos in new [] {top, bottom, left, right})
                {
                    var normalizedPosition = map.Normalize(pos);
                    if (minDistances.ContainsKey(pos) || !map.CanEnter(normalizedPosition))
                    {
                        continue;
                    }
                   
                    minDistances.Add(pos, distance);
                    newItems.Add(pos);
                }
            }
        }

        return minDistances;
    }
}

internal record MinDistanceToStart
{
    public required Position TilePosition { get; init; }
    public required int Distance { get; init; }
}

internal class Map
{
    private Tile[,] Tiles;

    public Map(string[] data)
    {
        Width= data[0].Trim().Length;
        Height = data.Length;

        Tiles = new Tile[Width, Height];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Tiles[x, y] = new Tile
                {
                    Position = new(x, y),
                    Type = data[y][x] switch
                    {
                        '.' => Type.Plot,
                        '#' => Type.Rock,
                        'S' => Type.Starting,
                        _ => throw new ArgumentOutOfRangeException()
                    }
                };

                if (data[y][x] == 'S')
                {
                    StartPosition = new(x, y);
                }
            }
        }
    }

    public Position StartPosition { get; set; }
    public int Height { get; }
    public int Width { get;  }

    public bool CanEnter(Position normalizedPosition)
    {
        return Tiles[normalizedPosition.X, normalizedPosition.Y].Type != Type.Rock;
    }

    public Position Normalize(Position position)
    {
        return new Position(((position.X % Width) + Width) % Width, ((position.Y % Height) + Height) % Height);
    }
}

internal record Tile
{
    public required Position Position { get; init; }
    public required Type Type { get; init; }

}

internal record Position(int X, int Y)
{
    public Position Top => this with { Y = Y - 1 };
    public Position Bottom => this with { Y = Y + 1 };
    public Position Left => this with { X = X - 1 };
    public Position Right => this with { X = X + 1 };

    public Position AtMap(int mapIndexX, int mapIndexY, Map map)
    {
        return new Position(X + mapIndexX * map.Width, Y + mapIndexY * map.Height);
    }

    public override string ToString()
    {
        return $"{X}_{Y}";
    }
};

internal enum Type
{
    Starting,
    Plot,
    Rock
}