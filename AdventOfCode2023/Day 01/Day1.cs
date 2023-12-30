using Xunit.Abstractions;

namespace AdventOfCode2023.Day_01;

public class Day1(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task DemoInput1()
    {
        var data = @"1abc2
pqr3stu8vwx
a1b2c3d4e5f
treb7uchet".Split("\n");

        Assert.Equal(142, Part1(data));
    }


    [Fact]
    public async Task DemoInput2()
    {
        var data = @"two1nine
eightwothree
abcone2threexyz
xtwone3four
4nineeightseven2
zoneight234
7pqrstsixteen".Split("\n");

        Assert.Equal(281, Part2(data));
    }


    [Fact]
    public async Task Day1Silver()
    {
        var lines = await File.ReadAllLinesAsync("Day 01/Day1.txt");

        var result = Part1(lines);

        testOutputHelper.WriteLine(result.ToString());
    }

    [Fact]
    public async Task Day1Gold()
    {
        var lines = await File.ReadAllLinesAsync("Day 01/Day1.txt");

        var result = Part2(lines);

        testOutputHelper.WriteLine(result.ToString());
    }

    private static int Part1(IEnumerable<string> lines)
    {
        return lines.Select(x =>
        {
            var firstNumber = int.Parse(x.First(char.IsNumber).ToString());
            var lastNumber = int.Parse(x.Last(char.IsNumber).ToString());
            return firstNumber * 10 + lastNumber;
        }).Sum();
    }

    private static int Part2(string[] lines)
    {
        return lines.Select(x =>
        {
            int? CheckForWording(string stringToCheck)
            {
                if (char.IsNumber(stringToCheck[0])) return int.Parse(stringToCheck[0].ToString());

                var numberSpelled = new[]
                {
                    "one", "two", "three", "four", "five", "six", "seven", "eight", "nine"
                };

                for (var i = 0; i < numberSpelled.Length; i++)
                    if (stringToCheck.StartsWith(numberSpelled[i]))
                        return i + 1;

                return null;
            }

            var firstNumber = x.Select((_, i) => CheckForWording(x[i..])).First(x => x != null).Value;
            var lastNumber = x.Select((_, i) => CheckForWording(x[i..])).Last(x => x != null).Value;
            return firstNumber * 10 + lastNumber;
        }).Sum();
    }
}

