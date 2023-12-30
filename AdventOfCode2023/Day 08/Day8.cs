using System.Collections.ObjectModel;
using Xunit.Abstractions;

namespace AdventOfCode2023.Day_08;

public class Day8(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void DemoInputPart1()
    {
        var data = @"RL

AAA = (BBB, CCC)
BBB = (DDD, EEE)
CCC = (ZZZ, GGG)
DDD = (DDD, DDD)
EEE = (EEE, EEE)
GGG = (GGG, GGG)
ZZZ = (ZZZ, ZZZ)".Split("\n");

        var result = Day8SilverCalc(data);

        Assert.Equal(2, result);
    }

    [Fact]
    public void DemoInputPart1Example2()
    {
        var data = @"LLR

AAA = (BBB, BBB)
BBB = (AAA, ZZZ)
ZZZ = (ZZZ, ZZZ)".Split("\n");

        var result = Day8SilverCalc(data);

        Assert.Equal(6, result);
    }

    [Fact]
    public void DemoInputPart2()
    {
        var data = @"LR

11A = (11B, XXX)
11B = (XXX, 11Z)
11Z = (11B, XXX)
22A = (22B, XXX)
22B = (22C, 22C)
22C = (22Z, 22Z)
22Z = (22B, 22B)
XXX = (XXX, XXX)".Split("\n");

        var result = Day8GoldCalc(data);

        Assert.Equal(6, result);
    }


    [Fact]
    public async Task Day8Silver()
    {
        var data = await File.ReadAllLinesAsync("Day 08/Day8.txt");
        var result = Day8SilverCalc(data);
        testOutputHelper.WriteLine(result.ToString());
    }
    
    
    [Fact]
    public async Task Day8Gold()
    {
        var data = await File.ReadAllLinesAsync("Day 08/Day8.txt");
        var result = Day8GoldCalc(data);
        testOutputHelper.WriteLine(result.ToString());
    }


    private decimal Day8SilverCalc(string[] data)
    {
        var network = new Network(data);

        var startNode = network.Nodes["AAA"];

        var executor = new Executor(network);

        var context = new ExecutionContext
        {
            InstructionPointer = new InstructionPointer(),
            Node = startNode,
            OverallSteps = 0
        };

        executor.Execute(context, node => node.Label == "ZZZ");

        return context.OverallSteps;
    }

    private decimal Day8GoldCalc(string[] data)
    {
        var network = new Network(data);

        var startNodes = network.Nodes.Values.Where(x => x.Label.EndsWith('A')).ToList();

        var contexts = startNodes.Select(x => new ExecutionContext
        {
            Node = x,
            InstructionPointer = new InstructionPointer(),
            OverallSteps = 0
        }).ToList();

        Func<Node, bool> endsWithZ = node => node.Label.EndsWith('Z');

        var executor = new Executor(network);
        foreach (var executionContext in contexts) executor.Execute(executionContext, endsWithZ);

        while (true)
        {
            var minSteps = contexts.MinBy(x => x.OverallSteps)!;
            if (contexts.TrueForAll(x => x.OverallSteps == minSteps.OverallSteps))
            {
                return minSteps.OverallSteps;
            };

            executor.Execute(minSteps, endsWithZ);
            //var stats = executor.Execute(new InstructionPointer(), currentState, node => node.Label == "ZZZ");

            //return stats.NumInstructionRequired;
        }

        // KGV equals: 14616363770447

        //[Fact]
        //public async Task Day7Gold()
        //{
        //    var data = await File.ReadAllLinesAsync("Day 07/Day7.txt");
        //    var result = Day7GoldCalc(data);
        //    testOutputHelper.WriteLine(result.ToString());
        //}

        //private static decimal Day8SilverCalc(string[] data)
        //{
        //    var hands = data.Select(x =>
        //    {
        //        var split = x.Split(' ');
        //        var cards = split[0];
        //        var bid = decimal.Parse(split[1]);
        //        return new Hand(cards, bid, false);
        //    }).ToList();

        //    hands.Sort();

        //    return hands.Select((h, i) => h.Bid * (i + 1)).Sum();
        //}

        //private static decimal Day7GoldCalc(string[] data)
        //{
        //    var hands = data.Select(x =>
        //    {
        //        var split = x.Split(' ');
        //        var cards = split[0];
        //        var bid = decimal.Parse(split[1]);
        //        return new Hand(cards, bid, true);
        //    }).ToList();

        //    hands.Sort();

        //    return hands.Select((h, i) => h.Bid * (i + 1)).Sum();
        //}
    }
}

internal enum Instruction
{
    Left,
    Right
}

internal class Network
{
    public Network(string[] data)
    {
        Program = Program.FromLine(data[0], 0);
        Nodes = data.Skip(2).Select(Node.FromLine).AsEnumerable().ToDictionary(x => x.Label).AsReadOnly();
    }

    public Program Program { get; }
    public ReadOnlyDictionary<string, Node> Nodes { get; }
}

internal class ExecutionContext
{
    public required InstructionPointer InstructionPointer { get; set; }
    public required Node Node { get; set; }
    public required decimal OverallSteps { get; set; }
}

internal class Executor(Network network)
{
    private
        Dictionary<(int instructionPtr, Node node), (int targetInstructionPointer, Node targetNode, int requiredSteps)>
        _cache = new(); 

    public void Execute(ExecutionContext executionContext, Func<Node, bool> isTerminal)
    {
        if(_cache.TryGetValue((executionContext.InstructionPointer.Pointer, executionContext.Node), out var cached))
        {
            executionContext.OverallSteps += cached.requiredSteps;
            executionContext.Node = cached.targetNode;
            executionContext.InstructionPointer.Pointer = cached.targetInstructionPointer;
            return;
        }

        var startPointer = executionContext.InstructionPointer.Pointer;
        var startNode = executionContext.Node;
        var stepsTaken = 0;
        while (true)
        {
            stepsTaken++;
            executionContext.OverallSteps++;
            var nextInstruction = GetNextInstruction(executionContext.InstructionPointer);
            executionContext.Node = network.Nodes[executionContext.Node.Execute(nextInstruction)];
            if (isTerminal(executionContext.Node))
            {
                _cache.Add((startPointer, startNode), (executionContext.InstructionPointer.Pointer, executionContext.Node, stepsTaken));
                return;
            }
        }
    }


    private Instruction GetNextInstruction(InstructionPointer instructionPointer)
    {
        var currentInstruction = network.Program.Instructions[instructionPointer.Pointer];
        instructionPointer.Pointer = (instructionPointer.Pointer + 1) % network.Program.Instructions.Count;
        return currentInstruction;
    }
}

internal class InstructionPointer
{
    public int Pointer { get; set; }
}

internal class Program(IReadOnlyList<Instruction> instructions)
{
    public IReadOnlyList<Instruction> Instructions { get; } = instructions;

    public static Program FromLine(string line, int instructionPointer)
    {
        return new Program(line.Trim().Select(x => x switch
        {
            'R' => Instruction.Right,
            'L' => Instruction.Left,
            _ => throw new NotImplementedException()
        }).ToList());
    }
}

internal class Node(string label, string left, string right)
{
    public string Label { get; } = label;
    public string Left { get; } = left;
    public string Right { get; } = right;

    public string Execute(Instruction instruction)
    {
        return instruction switch
        {
            Instruction.Left => left,
            Instruction.Right => right,
            _ => throw new NotImplementedException()
        };
    }

    public static Node FromLine(string line)
    {
        // AAA = (BBB, CCC)
        var split = new string(line.Trim().Where(x => char.IsAsciiLetter(x) || char.IsNumber(x) || char.IsWhiteSpace(x)).ToArray())
            .Split(" ",
                StringSplitOptions.RemoveEmptyEntries);
        return new Node(split[0], split[1], split[2]);
    }
}