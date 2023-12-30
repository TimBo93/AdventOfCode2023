using System.Diagnostics;
using System.Text;
using Xunit.Abstractions;

namespace AdventOfCode2023.Day_14;

public class Day14(ITestOutputHelper testOutputHelper)
{
    private readonly string demoString = @"O....#....
O.OO#....#
.....##...
OO.#O....O
.O.....O#.
O.#..O.#.#
..O..#O..O
.......O..
#....###..
#OO..#....";


    [Fact]
    public async Task DemoInputTest()
    {
        var data = demoString.Split("\n");

        var result = Day14SilverCalc(data);
        Assert.Equal(136, result);
    }

    [Fact]
    public async Task Day14SilverTest()
    {
        var data = await File.ReadAllLinesAsync("Day 14/Day14.txt");
        testOutputHelper.WriteLine(Day14SilverCalc(data).ToString());
    }

    [Fact]
    public async Task DemoInputGoldTest()
    {
        var data = demoString.Split("\n");

        Assert.Equal(64, CalculateBillionsOfRepeats(data));
    }

    [Fact]
    public async Task Day14GoldTest()
    {
        var data = await File.ReadAllLinesAsync("Day 14/Day14.txt");
        testOutputHelper.WriteLine(CalculateBillionsOfRepeats(data).ToString());
    }

    private int CalculateBillionsOfRepeats(string[] data)
    {
        var map = new Map(data);

        var step = 0;
        var cache = new Dictionary<string, int>();

        while (true)
        {
            var serializeMap = map.SerializeMap();

            if (cache.ContainsKey(serializeMap))
            {
                var stepsForPatternStart = new decimal(cache[serializeMap]);
                var stepsForPatternEnd = new decimal(step);

                var target = new decimal(1_000_000_000);

                var repeatsToGo = (target - stepsForPatternStart) % (stepsForPatternEnd - stepsForPatternStart);

                for (var i = 0; i < repeatsToGo; i++) CalculateCycle(map);

                return map.Items.Where(x => x.ItemType == ItemType.Rounded).Sum(x => x.PositionY);
            }

            cache.Add(serializeMap, step);

            CalculateCycle(map);
            step++;
        }
    }

    private void CalculateCycle(Map map)
    {
        MoveUp(map);
        MoveLeft(map);
        MoveDown(map);
        MoveRight(map);
    }

    //foreach (var s in map.RenderMap())
    //{
    //    System.Diagnostics.Debug.WriteLine(s);
    //}
    private int Day14SilverCalc(string[] data)
    {
        var map = new Map(data);
        MoveUp(map);
        return map.Items.Where(x => x.ItemType == ItemType.Rounded).Sum(x => x.PositionY);
    }

    private void MoveUp(Map map)
    {
        for (var column = 0; column < map.Width; column++)
        {
            var columnItems = map.Items.Where(x => x.PositionX == column).OrderByDescending(x => x.PositionY);
            var target = map.Height;
            foreach (var columnItem in columnItems)
            {
                if (columnItem.ItemType == ItemType.Rounded)
                    if (columnItem.PositionY <= target)
                    {
                        columnItem.PositionY = target;
                        target -= 1;
                    }

                if (columnItem.ItemType == ItemType.Qube) target = columnItem.PositionY - 1;
            }
        }
    }

    private void MoveDown(Map map)
    {
        for (var column = 0; column < map.Width; column++)
        {
            var columnItems = map.Items.Where(x => x.PositionX == column).OrderBy(x => x.PositionY);
            var target = 1;
            foreach (var columnItem in columnItems)
            {
                if (columnItem.ItemType == ItemType.Rounded)
                    if (columnItem.PositionY >= target)
                    {
                        columnItem.PositionY = target;
                        target += 1;
                    }

                if (columnItem.ItemType == ItemType.Qube) target = columnItem.PositionY + 1;
            }
        }
    }

    private void MoveRight(Map map)
    {
        for (var row = 1; row <= map.Height; row++)
        {
            var columnItems = map.Items.Where(x => x.PositionY == row).OrderByDescending(x => x.PositionX);
            var target = map.Width - 1;
            foreach (var columnItem in columnItems)
            {
                if (columnItem.ItemType == ItemType.Rounded)
                    if (columnItem.PositionX <= target)
                    {
                        columnItem.PositionX = target;
                        target -= 1;
                    }

                if (columnItem.ItemType == ItemType.Qube) target = columnItem.PositionX - 1;
            }
        }
    }

    private void MoveLeft(Map map)
    {
        for (var row = 1; row <= map.Height; row++)
        {
            var columnItems = map.Items.Where(x => x.PositionY == row).OrderBy(x => x.PositionX);
            var target = 0;
            foreach (var columnItem in columnItems)
            {
                if (columnItem.ItemType == ItemType.Rounded)
                    if (columnItem.PositionX >= target)
                    {
                        columnItem.PositionX = target;
                        target += 1;
                    }

                if (columnItem.ItemType == ItemType.Qube) target = columnItem.PositionX + 1;
            }
        }
    }
}

internal class Map
{
    public Map(string[] data)
    {
        Width = data[0].Trim().Length;
        Height = data.Length;

        var items = new List<Item>();
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            if (data[y][x] == 'O') items.Add(new Item(ItemType.Rounded, x, Height - y));

            if (data[y][x] == '#') items.Add(new Item(ItemType.Qube, x, Height - y));
        }

        Items = items;
    }

    public int Height { get; set; }

    public int Width { get; set; }

    public IEnumerable<Item> Items { get; }

    public string[] RenderMap()
    {
        var result = new List<string>();

        for (var i = 0; i < Height; i++)
        {
            var builder = new StringBuilder("".PadLeft(Width, '.'));

            foreach (var item in Items.Where(x => x.PositionY == Height - i))
                switch (item.ItemType)
                {
                    case ItemType.Rounded:
                        builder[item.PositionX] = 'O';
                        break;
                    case ItemType.Qube:
                        builder[item.PositionX] = '#';
                        break;
                }

            result.Add(builder.ToString());
        }

        return result.ToArray();
    }

    public string SerializeMap()
    {
        return string.Join('\n', RenderMap());
    }

    public void PrintMap()
    {
        Debug.WriteLine("-------------------------");
        foreach (var s in RenderMap()) Debug.WriteLine(s);
    }
}

internal class Item
{
    public Item(ItemType itemType, int positionX, int positionY)
    {
        ItemType = itemType;
        PositionX = positionX;
        PositionY = positionY;
    }

    public int PositionX { get; set; }
    public int PositionY { get; set; }

    public ItemType ItemType { get; }
}

internal enum ItemType
{
    Rounded,
    Qube
}