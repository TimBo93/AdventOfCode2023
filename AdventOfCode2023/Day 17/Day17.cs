using System.Reflection.Metadata;
using Xunit.Abstractions;

namespace AdventOfCode2023.Day_17;

public class Day17(ITestOutputHelper testOutputHelper)
{
    private readonly string _demoInput = @"2413432311323
3215453535623
3255245654254
3446585845452
4546657867536
1438598798454
4457876987766
3637877979653
4654967986887
4564679986453
1224686865563
2546548887735
4322674655533";

    private readonly string _demoInput2 = @"111111111111
999999999991
999999999991
999999999991
999999999991";

    [Fact]
    public async Task DemoInputTest()
    {
        Assert.Equal(102, new HeatPathCalculator().CalculateLessHeatLost(Map.From(_demoInput.Split("\n")), true));
    }

    [Fact]
    public async Task DemoInputTestPart2Test1()
    {
        Assert.Equal(94, new HeatPathCalculator().CalculateLessHeatLost(Map.From(_demoInput.Split("\n")), false));
    }

    [Fact]
    public async Task DemoInputTestPart2Test2()
    {
        Assert.Equal(71, new HeatPathCalculator().CalculateLessHeatLost(Map.From(_demoInput2.Split("\n")), false));
    }

    [Fact]
    public async Task Day17SilverTest()
    {
        var data = await File.ReadAllLinesAsync("Day 17/Day17.txt");
        testOutputHelper.WriteLine(new HeatPathCalculator().CalculateLessHeatLost(Map.From(data), true).ToString());
    }

    [Fact]
    public async Task Day17GoldTest()
    {
        var data = await File.ReadAllLinesAsync("Day 17/Day17.txt");
        testOutputHelper.WriteLine(new HeatPathCalculator().CalculateLessHeatLost(Map.From(data), false).ToString());
    }
}

internal class HeatPathCalculator
{
    
    public int CalculateLessHeatLost(Map map, bool part1)
    {
        var nodes = new Node[map.Width, map.Height];
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                nodes[x, y] = new Node();
            }
        }

        SortedList<SortedListItemHandle, SortedListItemHandle> sortedList = new();
        var itemHandleRight = new SortedListItemHandle(Path.Init(map, Direction.Right));
        var itemHandleBottom = new SortedListItemHandle(Path.Init(map, Direction.Bottom));
        sortedList.Add(itemHandleRight, itemHandleRight);
        sortedList.Add(itemHandleBottom, itemHandleBottom);


        while (true)
        {
            var bestPath = sortedList.First().Key;
            sortedList.RemoveAt(0);

            var path = bestPath.RelatedPath;
            if (path.IsValidTargetPosition(part1))
            {
                return path.OverallCosts;
            }

            if (!nodes[path.Position.X, path.Position.Y]
                    .SetMinCostCalculatedIfNewBestScore(path.LastDirection, path.Repeats, path.OverallCosts))
            {
                continue;
            }

            List<Path> allNeighborPaths;
            if (part1)
            {
                allNeighborPaths = path.GetAllNeighborPathsPart1().ToList();
            }
            else
            {
                allNeighborPaths = path.GetAllNeighborPathsPart2().ToList();
            }

            foreach (var neighbor in allNeighborPaths)
            {
                var newHandle = new SortedListItemHandle(neighbor);
                sortedList.Add(newHandle, newHandle);
            }
        }
    }
}

internal class Map
{
    private readonly int[,] _costMap;
    public int Height { get; }
    public int Width { get; }

    private Map(string[] map)
    {
        Width = map[0].Trim().Length;
        Height = map.Length;

        _costMap = new int[Width, Height];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            _costMap[x, y] = int.Parse(map[y][x].ToString());
    }

    public static Map From(string[] map)
    {
        return new Map(map);
    }

    public bool IsPositionLegal(Position nextPosition)
    {
        return nextPosition is { X: >= 0, Y: >= 0 } && nextPosition.X < Width && nextPosition.Y < Height;
    }

    public int GetCosts(Position nextPosition)
    {
        return _costMap[nextPosition.X, nextPosition.Y];
    }

    public bool IsTargetPosition(Position pathPosition)
    {
        return pathPosition.X == Width - 1 && pathPosition.Y == Height - 1;
    }
}

internal class Node
{
    private readonly Dictionary<(Direction, int), int> nodeState =new ();

    public bool SetMinCostCalculatedIfNewBestScore(Direction direction, int repeats, int costs)
    {
        if (nodeState.TryGetValue((direction, repeats), out var bestValue) && bestValue < costs)
        {
            return false;
        }

        nodeState[(direction, repeats)] = bestValue;
        return true;
    }
}

internal class SortedListItemHandle : IComparable<SortedListItemHandle>
{

    public static int UniqueId = 0;
    private readonly int id = UniqueId++;

    public SortedListItemHandle(Path relatedPath)
    {
        RelatedPath = relatedPath;
        OverallCosts = relatedPath.OverallCosts;
    }

    public int OverallCosts { get; }

    public Path RelatedPath { get; }

    public int CompareTo(SortedListItemHandle? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var overallCostsComparison = OverallCosts.CompareTo(other.OverallCosts);
        if (overallCostsComparison != 0) return overallCostsComparison;
        return id.CompareTo(other.id);
    }
}

internal class Path
{
    private readonly Map _map;
    public int OverallCosts { get; }
    public Direction LastDirection { get; }
    public int Repeats { get; }
    public Position Position { get; }

    private Path(int overallCosts, Direction lastDirection, int repeats, Position position, Map map)
    {
        _map = map;
        OverallCosts = overallCosts;
        LastDirection = lastDirection;
        Repeats = repeats;
        Position = position;
    }

    public static Path Init(Map map, Direction direction)
    {
        return new Path(0, direction, 0, new Position { X = 0, Y = 0}, map);
    }

    public IEnumerable<Path> GetAllNeighborPathsPart1()
    {
        var allDirections = new List<Direction> { Direction.Top, Direction.Right, Direction.Bottom, Direction.Left };
        allDirections.Remove(GetOppositeDirection(LastDirection));


        foreach (var direction in allDirections)
        {
            var nextPosition = Position.Move(direction);
            if (!_map.IsPositionLegal(nextPosition))
            {
                continue;
            }
            
            var nextRepeats = direction == LastDirection ? Repeats + 1 : 1;
            if(nextRepeats > 3) {continue;}

            var nextCost = OverallCosts + _map.GetCosts(nextPosition);

            yield return new Path(nextCost, direction, nextRepeats, nextPosition, _map);
        }
    }

    public IEnumerable<Path> GetAllNeighborPathsPart2()
    {
        var allAllowedDirections = new List<Direction> { Direction.Top, Direction.Right, Direction.Bottom, Direction.Left };
        allAllowedDirections.Remove(GetOppositeDirection(LastDirection));

        if (Repeats < 4)
        {
            allAllowedDirections = new List<Direction>() { LastDirection };
        }
        
        if (Repeats == 10)
        {
            allAllowedDirections.Remove(LastDirection);
        }

        foreach (var direction in allAllowedDirections)
        {
            var nextPosition = Position.Move(direction);
            if (!_map.IsPositionLegal(nextPosition))
            {
                continue;
            }

            var nextRepeats = direction == LastDirection ? Repeats + 1 : 1;
            var nextCost = OverallCosts + _map.GetCosts(nextPosition);
            yield return new Path(nextCost, direction, nextRepeats, nextPosition, _map);
        }
    }

    private Direction GetOppositeDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Top => Direction.Bottom,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.Bottom => Direction.Top,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
    }

    public bool IsValidTargetPosition(bool isPart1)
    {
        if (isPart1)
        {
            return _map.IsTargetPosition(Position);
        }

        return _map.IsTargetPosition(Position) && Repeats >= 4;
    }
}

internal class Position
{
    public required int X { get; init; }
    public required int Y { get; init; }

    public Position Move(Direction direction)
    {
        return direction switch
        {
            Direction.Top => new Position { X = X, Y = Y - 1 },
            Direction.Left => new Position { X = X - 1, Y = Y },
            Direction.Right => new Position { X = X + 1, Y = Y },
            Direction.Bottom => new Position { X = X, Y = Y + 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
    }
}

public enum Direction
{
    Top,
    Left,
    Right,
    Bottom,
}