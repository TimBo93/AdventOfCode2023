using System.Diagnostics;
using System.Numerics;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Xunit.Abstractions;

namespace AdventOfCode2023.Day_12;

public static class BigIntegerExtension
{
    public static BigInteger Sum(this IEnumerable<BigInteger> bigInteger)
    {
        BigInteger sum = 0;

        foreach (var integer in bigInteger)
        {
            sum += integer;
        }

        return sum;
    }
}

public class Day12(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData("???.### 1,1,3", 1)]
    [InlineData(".??..??...?##. 1,1,3", 4)]
    [InlineData("?#?#?#?#?#?#?#? 1,3,1,6", 1)]
    [InlineData("????.#...#... 4,1,1", 1)]
    [InlineData("????.######..#####. 1,6,5", 4)]
    [InlineData("?###???????? 3,2,1", 10)]
    public void DemoInputPart1Examples(string input, int expected)
    {
        Assert.Equal(expected, NumPossibleArrangements(input));
    }

    [Theory]
    [InlineData("#.#.### 1,1,3")]
    [InlineData(".#...#....###. 1,1,3")]
    [InlineData(".#.###.#.###### 1,3,1,6")]
    [InlineData("####.#...#... 4,1,1")]
    [InlineData("#....######..#####. 1,6,5")]
    [InlineData(".###.##....# 3,2,1")]
    public void ParsingTests(string input)
    {
        var split = input.Split(' ');
        var unknownRecordStr = split[0];
        var backupRecordStr = split[1];

        var expectedGroups = ParseBackupRecordStr(backupRecordStr);

        Assert.Equal(UnknownStrToBackupRecord(unknownRecordStr), expectedGroups);
    }


    [Fact]
    public async Task Day12Silver()
    {
        var data = await File.ReadAllLinesAsync("Day 12/Day12.txt");
        var sum = data.Select(NumPossibleArrangements).Sum();
        Assert.Equal(7344, sum);
        testOutputHelper.WriteLine(sum.ToString());
    }


    [Theory]
    [InlineData("???.### 1,1,3", 1)]
    [InlineData(".??..??...?##. 1,1,3", 16384)]
    [InlineData("?#?#?#?#?#?#?#? 1,3,1,6", 1)]
    [InlineData("????.#...#... 4,1,1", 16)]
    [InlineData("????.######..#####. 1,6,5", 2500)]
    //[InlineData("?###???????? 3,2,1", 506250)]
    public void Part2Tests(string input, int expected)
    {
        Assert.Equal(expected, NumPossibleArrangementsFactor5(input));
    }

    [Fact]
    public async Task Day12Gold()
    {
        //// too low: 712617224655
        var data = await File.ReadAllLinesAsync("Day 12/Day12.txt");
        var sum = data.Select(NumPossibleArrangementsFactor5).Sum();
        testOutputHelper.WriteLine(sum.ToString());
    }

    private Dictionary<string, BigInteger> SolutionCache = new();
    private IReadOnlyList<int> expectedGroups = new List<int>();

    private BigInteger NumPossibleArrangements(string input)
    {
        Debug.WriteLine($"=======");
        Debug.WriteLine($"Calculating: {input}");
        Debug.WriteLine($"=======");

        var split = input.Split(' ');
        var unknownRecordStr = split[0];
        var backupRecordStr = split[1];

        expectedGroups = ParseBackupRecordStr(backupRecordStr);
        SolutionCache = new();

        return DetermineAllPossibleCombinations(0, unknownRecordStr);
    }

    private BigInteger NumPossibleArrangementsFactor5(string input)
    {
        Debug.WriteLine($"=======");
        Debug.WriteLine($"Calculating: {input}");
        Debug.WriteLine($"=======");

        var split = input.Split(' ');
        var unknownRecordStr = split[0];
        var backupRecordStr = split[1];

        var unknownRecordStrBuilder = $"{unknownRecordStr}";
        var backupRecordStrBuilder = $"{backupRecordStr}";
        for (int i = 0; i < 4; i++)
        {
            unknownRecordStrBuilder = $"{unknownRecordStrBuilder}?{unknownRecordStr}";
            backupRecordStrBuilder = $"{backupRecordStrBuilder},{backupRecordStr}";
        }

        expectedGroups = ParseBackupRecordStr(backupRecordStrBuilder);
        SolutionCache = new();

        return DetermineAllPossibleCombinations(0, unknownRecordStrBuilder);
    }


    private IReadOnlyList<int> ParseBackupRecordStr(string backupRecordStr)
    {
        var split = backupRecordStr.Split(',');
        return split.Select(int.Parse).ToList();
    }

    private IReadOnlyList<int> UnknownStrToBackupRecord(string operationalString)
    {
        return operationalString.Split('.').Select(x => x.Length).Where(x => x > 0).ToList();
    }

    private BigInteger DetermineAllPossibleCombinations(int currentIndex, string currentValue)
    {
        var currentGroups = ExtractGroupToCheck(currentValue);
        if (HasContradiction(currentGroups, expectedGroups)) return 0;

        if (currentIndex == currentValue.Length)
        {
            // we are finish
            if (currentGroups.Count == expectedGroups.Count)
            {
                return 1;
            }
            return 0;
        }


        var currentSpring = currentValue[currentIndex];
        
        // here we have a new 'era' and try to reuse my solutions from previous runs
        if (currentSpring == '.')
        {
            var openItems = currentValue.Substring(currentIndex);
            var key = $"{openItems}_{currentGroups.Count}";
            if (SolutionCache.ContainsKey(key))
            {
                return SolutionCache[key];
            }

            var solution = DetermineAllPossibleCombinations(currentIndex + 1, currentValue);
            SolutionCache.Add(key, solution);
            return solution;
        }

        if (currentSpring == '#')
        {
            return DetermineAllPossibleCombinations(currentIndex + 1, currentValue);
        }

        Assert.Equal('?', currentSpring);

        // Case 1 -> it is damaged 
        var operationalString = new StringBuilder(currentValue);
        operationalString[currentIndex] = '#';
        var damangedCount =  DetermineAllPossibleCombinations(currentIndex + 1, operationalString.ToString());

        // Case 2 -> it is operational 
        var outOfOrderStr = new StringBuilder(currentValue);
        outOfOrderStr[currentIndex] = '.';
        // go not to next index and try to use '.' and cache instead
        var undamagedCount = DetermineAllPossibleCombinations(currentIndex, outOfOrderStr.ToString());

        return damangedCount + undamagedCount;
    }


    private IReadOnlyList<int> ExtractGroupToCheck(string operationalString)
    {
        var result = new List<int>();
        var groups = operationalString.Split('.').Where(x => x.Length > 0);

        foreach (var group in groups)
        {
            if (group.Contains('?')) return result;

            result.Add(group.Length);
        }

        return result;
    }

    private bool HasContradiction(IReadOnlyList<int> groupsToCheck, IReadOnlyList<int> expectedGroups)
    {
        return !expectedGroups.Take(groupsToCheck.Count).SequenceEqual(groupsToCheck);
    }
}