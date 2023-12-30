using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Xunit.Abstractions;

namespace AdventOfCode2023.Day_13;


public class Day12(ITestOutputHelper testOutputHelper)
{
    private readonly string demoString = @"#.##..##.
..#.##.#.
##......#
##......#
..#.##.#.
..##..##.
#.#.##.#.

#...##..#
#....#..#
..##..###
#####.##.
#####.##.
..##..###
#....#..#";


    [Fact]
    public async Task DemoInputTest()
    {
        var data = this.demoString.Split("\n");
        var maps = MapSetParser(data);

        Assert.Equal(5, maps[0].GetVerticalMirror());
        Assert.Null(maps[0].GetHoricontalMirror());

        Assert.Null(maps[1].GetVerticalMirror());
        Assert.Equal(4, maps[1].GetHoricontalMirror());

        var result = Day13SilverCalc(data);
        Assert.Equal(405, result);
    }

    


    [Fact]
    public async Task Day13Silver()
    {
        var data = await File.ReadAllLinesAsync("Day 13/Day13.txt");
        testOutputHelper.WriteLine(Day13SilverCalc(data).ToString());
    }


    [Fact]
    public async Task Day13GoldTest()
    {
        var data = this.demoString.Split("\n");
        var maps = MapSetParser(data);

        Assert.Null(maps[0].GetVerticalMirrorWithMutations());
        Assert.Equal(3, maps[0].GetHoricontalMirrorWithMutations());

        Assert.Null(maps[1].GetVerticalMirrorWithMutations());
        Assert.Equal(1, maps[1].GetHoricontalMirrorWithMutations());

        var result = Day13GoldCalc(data);
        Assert.Equal(400, result);
    }

    [Fact]
    public async Task Day13Gold()
    {
        var data = await File.ReadAllLinesAsync("Day 13/Day13.txt");
        testOutputHelper.WriteLine(Day13GoldCalc(data).ToString());
    }

    private int Day13SilverCalc(string[] data)
    {
        var maps = MapSetParser(data);

        for (int i = 0; i < maps.Count; i++)
        {
            System.Diagnostics.Debug.WriteLine($"= = = = = = = = = = = = = ");
            System.Diagnostics.Debug.WriteLine($"Calculating {i}");
            var horicontalMirror = maps[i].GetHoricontalMirror();
            System.Diagnostics.Debug.WriteLine($"Horicontal {horicontalMirror}");
            var verticalMirror = maps[i].GetVerticalMirror();
            System.Diagnostics.Debug.WriteLine($"Vertical {verticalMirror}");
            Assert.True(horicontalMirror != null || verticalMirror != null);
        }

        var horicontalSum = maps.Select(x => x.GetHoricontalMirror()).Where(x => x.HasValue).Sum(x => x.Value!);
        var verticalSum = maps.Select(x => x.GetVerticalMirror()).Where(x => x.HasValue).Sum(x => x.Value!);

        return horicontalSum * 100 + verticalSum;
    }



    private int Day13GoldCalc(string[] data)
    {
        var maps = MapSetParser(data);

        for (int i = 0; i < maps.Count; i++)
        {
            System.Diagnostics.Debug.WriteLine($"= = = = = = = = = = = = = ");
            System.Diagnostics.Debug.WriteLine($"Calculating {i}");
            var horicontalMirror = maps[i].GetHoricontalMirrorWithMutations();
            System.Diagnostics.Debug.WriteLine($"Horicontal {horicontalMirror}");
            var verticalMirror = maps[i].GetVerticalMirrorWithMutations();
            System.Diagnostics.Debug.WriteLine($"Vertical {verticalMirror}");

            if (!(horicontalMirror != null || verticalMirror != null))
            {
                var x = 123;
            }
            Assert.True(horicontalMirror != null || verticalMirror != null);
        }

        var horicontalSum = maps.Select(x => x.GetHoricontalMirrorWithMutations()).Where(x => x.HasValue).Sum(x => x.Value!);
        var verticalSum = maps.Select(x => x.GetVerticalMirrorWithMutations()).Where(x => x.HasValue).Sum(x => x.Value!);

        return horicontalSum * 100 + verticalSum;
    }

    private IReadOnlyList<Map> MapSetParser(string[] data)
    {
        var resultList = new List<Map>();
        var currentData = new List<string>();

        foreach (var row in data.Select(x => x.Trim()))
        {
            if (String.IsNullOrWhiteSpace(row) && currentData.Count > 0)
            {
                resultList.Add(new Map(currentData.ToArray()));
                currentData.Clear();
                continue;
            }

            currentData.Add(row);
        }

        if (currentData.Count > 0)
        {
            resultList.Add(new Map(currentData.ToArray()));
        }

        return resultList;
    }
}


class Map{

    private readonly string[] _data;
    private readonly int _numRows;
    private readonly int _numColumns;

    public Map(string[] data)
    {
        _data = data;
        _numRows = _data.Length;
        _numColumns = _data[0].Trim().Length;
    }

    public IEnumerable<Map> GetAllMutationMaps()
    {
        for (int rowIndex = 0; rowIndex < _numRows; rowIndex++)
        {
            for (int columnIndex= 0; columnIndex < _numColumns; columnIndex++)
            {
                yield return CloneAndMutate(rowIndex, columnIndex);
            }
        }    
    }

    private Map CloneAndMutate(int rowIndex, int columnIndex)
    {
        var copiedMap = _data.ToArray();
        var stringToMutate = new StringBuilder(copiedMap[rowIndex]);
        stringToMutate[columnIndex] = stringToMutate[columnIndex] == '.' ? '#' : '.';
        copiedMap[rowIndex] = stringToMutate.ToString();
        return new Map(copiedMap);
    }

    public int? GetVerticalMirror(int? lineToSkip = null)
    {
        for (var columnIndex = 1; columnIndex < _numColumns; columnIndex++)
        {
            if (CheckEachRowForMirror(columnIndex))
            {
                if (lineToSkip.HasValue && lineToSkip == columnIndex)
                {
                    continue;
                }
                return columnIndex;
            }
        }
        return null;
    }

    private bool CheckEachRowForMirror(int columnIndex)
    {
        for (int row = 0; row < _numRows; row++)
        {
            var rowStr = GetRow(row);
            var strLeft = new string(rowStr.Substring(0, columnIndex).ToCharArray().Reverse().ToArray());
            var strRight = rowStr.Substring(columnIndex);
            var isMirror = CheckMirror(strLeft, strRight);
            if (!isMirror)
            {
                return false;
            }
        }
        return true;
    }

    public int? GetHoricontalMirror(int? lineToSkip = null)
    {
        for (var rowIndex = 1; rowIndex < _numRows; rowIndex++)
        {
            if (CheckEachColumnForMirror(rowIndex))
            {
                if (lineToSkip.HasValue && lineToSkip == rowIndex)
                {
                    continue;
                }
                return rowIndex;
            }
        }
        return null;
    }

    private bool CheckEachColumnForMirror(int rowIndex)
    {
        for (int columnIndex = 0; columnIndex < _numColumns; columnIndex++)
        {
            var columnStr = GetColumn(columnIndex);
            var strLeft = new string(columnStr.Substring(0, rowIndex).ToCharArray().Reverse().ToArray());
            var strRight = columnStr.Substring(rowIndex);
            var isMirror = CheckMirror(strLeft, strRight);
            if (!isMirror)
            {
                return false;
            }
        }
        return true;
    }

    private bool CheckMirror(string strLeft, string strRight)
    {
        if (strLeft.Length < strRight.Length)
        {
            return strRight.StartsWith(strLeft);
        } else if (strLeft.Length > strRight.Length)
        {
            return strLeft.StartsWith(strRight);

        }
        return strLeft == strRight;
    }

    public string GetRow(int row)
    {
        return _data[row];
    }

    public string GetColumn(int column)
    {
        var line = "";
        for (int i = 0; i < _numRows; i++)
        {
            line += _data[i][column];
        }
        return line;
    }

    public int? GetVerticalMirrorWithMutations()
    {
        var solutionWithoutMirror = this.GetVerticalMirror();
        //solutionWithoutMirror = null;

        foreach (var mutation in GetAllMutationMaps())
        {
            var mirror = mutation.GetVerticalMirror(solutionWithoutMirror);
            if (mirror.HasValue)
            {
                return mirror.Value;
            }
        }
        return null;
    }

    public int? GetHoricontalMirrorWithMutations()
    {
        var solutionWithoutMirror = this.GetHoricontalMirror();
        //solutionWithoutMirror = null;

        foreach (var mutation in GetAllMutationMaps())
        {
            var mirror = mutation.GetHoricontalMirror(solutionWithoutMirror);
            if (mirror.HasValue)
            {
                return mirror.Value;
            }
        }
        return null;
    }
}