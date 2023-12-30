using System.Numerics;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace AdventOfCode2023.Day_19;

public class Day19(ITestOutputHelper testOutputHelper)
{
    private readonly string _demoInput = @"px{a<2006:qkq,m>2090:A,rfg}
pv{a>1716:R,A}
lnx{m>1548:A,A}
rfg{s<537:gd,x>2440:R,A}
qs{s>3448:A,lnx}
qkq{x<1416:A,crn}
crn{x>2662:A,R}
in{s<1351:px,qqz}
qqz{s>2770:qs,m<1801:hdj,R}
gd{a>3333:R,R}
hdj{m>838:A,pv}

{x=787,m=2655,a=1222,s=2876}
{x=1679,m=44,a=2067,s=496}
{x=2036,m=264,a=79,s=2244}
{x=2461,m=1339,a=466,s=291}
{x=2127,m=1623,a=2188,s=1013}";


    [Fact]
    public async Task DemoInputTest()
    {
        Assert.Equal(19114, new Parser(_demoInput.Split("\n")).Calculate());
    }

    [Fact]
    public async Task Day19SilverTest()
    {
        var data = await File.ReadAllLinesAsync("Day 19/Day19.txt");
        testOutputHelper.WriteLine(new Parser(data).Calculate().ToString());
    }

    [Fact]
    public async Task DemoInputTestPart2()
    {
        Assert.Equal(BigInteger.Parse("167409079868000"), new Parser(_demoInput.Split("\n")).ResolveAllPossibleSolutions());
    }

    [Fact]
    public async Task Day19GoldTest()
    {
        var data = await File.ReadAllLinesAsync("Day 19/Day19.txt");
        testOutputHelper.WriteLine(new Parser(data).ResolveAllPossibleSolutions().ToString());
    }
}

internal class Parser
{
    public Parser(string[] data)
    {
        var parseMode = "workflow";

        var workflows = new List<Workflow>();
        var parts = new List<Part>();

        foreach (var line in data.Select(x => x.Trim()))
        {
            if(string.IsNullOrEmpty(line))
            {
                parseMode = "parts";
                continue;
            }

            if (parseMode == "workflow")
            {
                workflows.Add(new Workflow(line));
            }

            if (parseMode == "parts")
            {
                parts.Add(new Part(line));
            }
        }

        Workflows = workflows.ToDictionary(x => x.Name);
        Parts = parts;
    }

    public Dictionary<string, Workflow> Workflows { get; }
    public IReadOnlyList<Part> Parts { get; }

    public int Calculate()
    {
        return Parts.Where(x => Applies(x)).Select(x => x.Variables.Values.Sum()).Sum();
    }

    private bool Applies(Part part)
    {
        var workflow = Workflows["in"];

        while (true)
        {
            var resolved = workflow.Resolve(part);

            if (resolved == "A")
            {
                return true;
            }

            if (resolved == "R")
            {
                return false;
            }

            workflow = Workflows[resolved];
        }
    }

    public BigInteger ResolveAllPossibleSolutions()
    {
        var xmasSpan = new XMasSpan();
        var startWorkflow = Workflows["in"];

        return GetAllPossibleCombinations(xmasSpan, startWorkflow);
    }

    private BigInteger GetAllPossibleCombinations(XMasSpan xmasSpan, Workflow workflow)
    {
        var currentSpan = xmasSpan;

        BigInteger sum = BigInteger.Zero;
        foreach (var workflowExpression in workflow.Expressions.ToList()[..^1])
        {
            var nextSpan = currentSpan.TryApplyExpression(workflowExpression);
            if (nextSpan != null)
            {
                if (workflowExpression.TargetWorkflow == "A")
                {
                    sum += nextSpan.GetAllCombinations();
                } else if (workflowExpression.TargetWorkflow == "R")
                {
                    // do nothing
                }
                else
                {
                    sum += GetAllPossibleCombinations(nextSpan, Workflows[workflowExpression.TargetWorkflow]);
                }
            }

            currentSpan = currentSpan.TryApplyExpression(workflowExpression.ReverseExpression());
            if (currentSpan == null)
            {
                return sum;
            }
        }

        // Fallback Expression
        var fallbackExpression = workflow.Expressions.Last();
        if (fallbackExpression.TargetWorkflow == "A")
        {
            sum += currentSpan.GetAllCombinations();
        }
        else if(fallbackExpression.TargetWorkflow == "R")
        {
            // do nothing
        }
        else
        {
            sum += GetAllPossibleCombinations(currentSpan, Workflows[fallbackExpression.TargetWorkflow]);
        }

        return sum;
    }
}

internal class Workflow
{
    public string Name { get; }
    public IReadOnlyList<IWorkflowExpression> Expressions { get; }

    public Workflow(string line)
    {
        var split = line.Split("{");
        Name = split[0];

        var expressionStrings = split[1].Replace("}", "").Split(",");
        var workflowExpressions = new List<IWorkflowExpression>();
        foreach (var expression in expressionStrings[..^1])
        {
            workflowExpressions.Add(new WorkflowExpression(expression));
        }

        workflowExpressions.Add(new FallbackExpression(expressionStrings[^1]));
        Expressions = workflowExpressions;
    }

    public string Resolve(Part part)
    {
        return Expressions.First(x => x.Matches(part)).TargetWorkflow;
    }
}

internal interface IWorkflowExpression
{
    public string VariableNameToCheck { get; }
    public int ValueToCompare { get; }
    public Comparison Comparison { get; }
    bool Matches(Part part);
    string TargetWorkflow { get; }
    IWorkflowExpression ReverseExpression();
}

internal class WorkflowExpression : IWorkflowExpression
{
    public int ValueToCompare { get; }
    public Comparison Comparison { get; }
    public string VariableNameToCheck { get; }
    public string TargetWorkflow { get; }

    private WorkflowExpression(Comparison comparison, string variableNameToCheck, int valueToCompare)
    {
        Comparison = comparison;
        VariableNameToCheck = variableNameToCheck;
        ValueToCompare = valueToCompare;
    }

    public WorkflowExpression(string expression)
    {
        var pattern = @"(.+)(>|<)(\d+):(.+)";
        var group = Regex.Match(expression, pattern).Groups;

        VariableNameToCheck = group[1].ToString();
        Comparison = group[2].ToString() switch
        {
            ">" => Comparison.Greater,
            "<" => Comparison.Lower,
            _ => throw new NotSupportedException("Operation not supported.")
        };
        ValueToCompare = int.Parse(group[3].ToString());
        TargetWorkflow = group[4].ToString();
    }

    public IWorkflowExpression ReverseExpression()
    {
        Comparison reverseComparison;
        int newValueToCompare;
        switch (Comparison)
        {
            case Comparison.Greater:
                reverseComparison = Comparison.Lower;
                newValueToCompare = ValueToCompare + 1;
                break;
            case Comparison.Lower:
                reverseComparison = Comparison.Greater;
                newValueToCompare = ValueToCompare - 1;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return new WorkflowExpression(reverseComparison, VariableNameToCheck, newValueToCompare);
    }

    public bool Matches(Part part)
    {
        var variableValue = part.Variables[VariableNameToCheck];

        return Comparison switch
        {
            Comparison.Greater => variableValue > ValueToCompare,
            Comparison.Lower => variableValue < ValueToCompare,
            _ => throw new NotSupportedException()
        };
    }
}

internal class FallbackExpression : IWorkflowExpression
{
    public FallbackExpression(string targetWorkflow)
    {
        TargetWorkflow = targetWorkflow;
    }

    public string VariableNameToCheck => throw new NotSupportedException();
    public int ValueToCompare => throw new NotSupportedException();
    public Comparison Comparison => throw new NotSupportedException();

    public bool Matches(Part part)
    {
        return true;
    }

    public string TargetWorkflow { get; }

    public IWorkflowExpression ReverseExpression()
    {
        throw new NotSupportedException();
    }
}

internal enum Comparison
{
    Greater, 
    Lower
}

internal class XMasSpan
{
    public Span XSpan { get;init; } = new Span("x");
    public Span MSpan { get;init; } = new Span("m");
    public Span ASpan { get;init; } = new Span("a");
    public Span SSpan { get; init; } = new Span("s");

    public XMasSpan? TryApplyExpression(IWorkflowExpression expression)
    {
        var clone = DeepCopy();
        if (expression.VariableNameToCheck switch
            {
                "x" => clone.XSpan.Apply(expression),
                "m" => clone.MSpan.Apply(expression),
                "a" => clone.ASpan.Apply(expression),
                "s" => clone.SSpan.Apply(expression),
                _ => throw new ArgumentException(nameof(expression))
            })
        {
            return clone;
        }
        return null;
    }

    private XMasSpan DeepCopy()
    {
        return new XMasSpan()
        {
            XSpan = XSpan.Clone(),
            MSpan = MSpan.Clone(),
            ASpan = ASpan.Clone(),
            SSpan = SSpan.Clone(),
        };
    }

    public BigInteger GetAllCombinations() =>
        new BigInteger(XSpan.AllCombinations) *
        new BigInteger(MSpan.AllCombinations) * 
        new BigInteger(ASpan.AllCombinations) * 
        new BigInteger(SSpan.AllCombinations);
}

internal class Span
{
    public Span(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public int MinValue { get; private set; } = 1;
    public int MaxValue { get; private set; } = 4000;

    public int AllCombinations => MaxValue - MinValue + 1;

    public bool Apply(IWorkflowExpression expression)
    {
        switch (expression.Comparison)
        {
            case Comparison.Greater:
                MinValue = Math.Max(MinValue, expression.ValueToCompare + 1);
                break;
            case Comparison.Lower:
                MaxValue = Math.Min(MaxValue, expression.ValueToCompare - 1);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return MinValue <= MaxValue;
    }

    public Span Clone()
    {
        return new Span(Name)
        {
            MinValue = MinValue,
            MaxValue = MaxValue,
        };
    }
}

internal class Part
{
    public IReadOnlyDictionary<string, int> Variables;

    public Part(string line)
    {
        Variables = line.Replace("{", "")
            .Replace("}", "")
            .Split(",")
            .Select(x => x.Split("="))
            .ToDictionary(x => x[0], x => int.Parse(x[1]));
    }
}