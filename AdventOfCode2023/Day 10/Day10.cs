using Xunit.Abstractions;

namespace AdventOfCode2023.Day_10;

public class Day10(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void DemoInputPart1Example1()
    {
        Assert.Equal(8, LongestDistanceOnPath(@"..F7.
.FJ|.
SJ.L7
|F--J
LJ...".Split("\n")));
    }

    [Fact]
    public void DemoInputPart1Example2()
    {
        Assert.Equal(4, LongestDistanceOnPath(@".....
.S-7.
.|.|.
.L-J.
.....".Split("\n")));
    }


    [Fact]
    public async Task Day10Silver()
    {
        var data = await File.ReadAllLinesAsync("Day 10/Day10.txt");
        testOutputHelper.WriteLine(LongestDistanceOnPath(data).ToString());
    }

    private int LongestDistanceOnPath(string[] data)
    {
        var map = new Map(data);
        var mapIterator = new MapIterator(map);
        return (int)Math.Ceiling((mapIterator.FindPath() + 1) / 2.0d);
    }
}

internal class Position
{
    public required int X { get; init; }
    public required int Y { get; init; }
}

internal class MapIterator
{
    private readonly Map _map;

    public MapIterator(Map map)
    {
        _map = map;
    }

    public int FindPath()
    {
        var distances = DistancesForStartTile().ToList();
        return distances.Max();
    }

    private IEnumerable<int> DistancesForStartTile()
    {
        var startPosition = _map.StartTile.Position;

        var top = (new Position { X = startPosition.X, Y = startPosition.Y - 1 }, WalkingDirection.North);
        var bottom = (new Position { X = startPosition.X, Y = startPosition.Y + 1 }, WalkingDirection.South);
        var left = (new Position { X = startPosition.X - 1, Y = startPosition.Y }, WalkingDirection.West);
        var right = (new Position { X = startPosition.X + 1, Y = startPosition.Y }, WalkingDirection.East);

        foreach (var startConfig in new[] { top, bottom, left, right })
        {
            var val = CalculateForStartConfig(startConfig);
            if (val != null)
            {
                yield return val.Value;
            }
        }
    }

    private int? CalculateForStartConfig((Position, WalkingDirection) startConfig)
    {
        Tile? currentTile = _map.GetTileForPosition(startConfig.Item1);
        if (currentTile == null) return null;

        if (currentTile.GetNextDirectionForWalkingDirection(startConfig.Item2) == null) return null;
        var steps = 0;
        WalkingDirection currentDirection = startConfig.Item2;
        do
        {
            steps++;
            currentDirection = (WalkingDirection)currentTile.GetNextDirectionForWalkingDirection(currentDirection)!;
            var newPosition = GetNextPosition(currentTile.Position, currentDirection);
            currentTile = _map.GetTileForPosition(newPosition);
            if (currentTile == null) return null;
        } while (currentTile != _map.StartTile);

        return steps;

    }

    private Position GetNextPosition(Position currentPosition, WalkingDirection outputDirection)
    {
        return outputDirection switch
        {
            WalkingDirection.North => new Position { X = currentPosition.X, Y = currentPosition.Y - 1 },
            WalkingDirection.South => new Position { X = currentPosition.X, Y = currentPosition.Y + 1 },
            WalkingDirection.West => new Position { X = currentPosition.X - 1, Y = currentPosition.Y },
            WalkingDirection.East => new Position { X = currentPosition.X + 1, Y = currentPosition.Y },
            _ => throw new ArgumentOutOfRangeException(nameof(outputDirection), outputDirection, null)
        };
    }
}

internal class Map
{
    private readonly Tile[,] _tiles;

    public Map(string[] data)
    {
        var width = data[0].Length - 1;
        var height = data.Length;

        _tiles = new Tile[width, height];
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var tile = new Tile(data[y][x], new Position {X = x, Y = y});
            _tiles[x, y] = tile;
            if (tile.IsStartPosition) StartTile = tile;
        }
    }

    public Tile StartTile { get; }

    public Tile? GetTileForPosition(Position position)
    {
        try
        {
            return _tiles[position.X, position.Y];
        }
        catch
        {
            return null;
        }
    }
}

internal class Tile
{
    public Position Position { get; }

    public Tile(char c, Position position)
    {
        this.Position = position;
        InOutDirections = c switch
        {
            '|' => new List<InOutDirection> { InOutDirection.North, InOutDirection.South },
            '-' => new List<InOutDirection> { InOutDirection.East, InOutDirection.West },
            'L' => new List<InOutDirection> { InOutDirection.North, InOutDirection.East },
            'J' => new List<InOutDirection> { InOutDirection.North, InOutDirection.West },
            '7' => new List<InOutDirection> { InOutDirection.South, InOutDirection.West },
            'F' => new List<InOutDirection> { InOutDirection.South, InOutDirection.East },
            '.' => new List<InOutDirection>(),
            'S' => new List<InOutDirection>(),
            _ => throw new NotSupportedException("not supported")
        };

        if (c == 'S') IsStartPosition = true;
    }
//| is a vertical pipe connecting north and south.
//- is a horizontal pipe connecting east and west.
//L is a 90-degree bend connecting north and east.
//J is a 90-degree bend connecting north and west.
//7 is a 90-degree bend connecting south and west.
//F is a 90-degree bend connecting south and east.
//. is ground; there is no pipe in this tile.
//S is the starting Position of the animal; there is a pipe on this tile, but your sketch doesn't show what shape the pipe has.

    public bool IsStartPosition { get; }

    public List<InOutDirection> InOutDirections { get; }

    public WalkingDirection? GetNextDirectionForWalkingDirection(WalkingDirection walkingDirection)
    {
        var inputDirection = FromWalkingDirection(walkingDirection);

        try
        {
            var newDirection = InOutDirections.First(x => x != inputDirection);
            return FromInoutDirection(newDirection);
        }
        catch (Exception)
        {
            return null;
        }
    }


    public InOutDirection FromWalkingDirection(WalkingDirection walkingDirection)
    {
        return walkingDirection switch
        {
            WalkingDirection.North => InOutDirection.South,
            WalkingDirection.East => InOutDirection.West,
            WalkingDirection.South => InOutDirection.North,
            WalkingDirection.West => InOutDirection.East,
            _ => throw new ArgumentOutOfRangeException(nameof(walkingDirection), walkingDirection, null)
        };
    }

    public WalkingDirection FromInoutDirection(InOutDirection inOutDirection)
    {
        return inOutDirection switch
        {
            InOutDirection.North => WalkingDirection.North,
            InOutDirection.East => WalkingDirection.East,
            InOutDirection.South => WalkingDirection.South,
            InOutDirection.West => WalkingDirection.West,
            _ => throw new ArgumentOutOfRangeException(nameof(inOutDirection), inOutDirection, null)
        };
    }
}

internal enum WalkingDirection
{
    North,
    West,
    South,
    East
}

internal enum InOutDirection
{
    North,
    West,
    South,
    East
}