using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using AssemblyDefinition = Mono.Cecil.AssemblyDefinition;

namespace cfg
{
    class OpCodeEqualityComparer : IEqualityComparer<Instruction>
    {
        public bool Equals(Instruction x, Instruction y)
        {
            return x?.Offset == y?.Offset;
        }

        public int GetHashCode(Instruction obj)
        {
            return obj.GetHashCode();
        }
    }
    class OpCodeComparer : IComparer<Instruction>
    {
        public int Compare(Instruction x, Instruction y)
        {
            return x?.Offset - y?.Offset ?? 0;
        }
    }

    class Edge
    {
        public Instruction From;
        public Instruction To;
    }

    class Program
    {
        static void PrintBB(Instruction leader, IEnumerable<Instruction> opCodes) =>
            Console.WriteLine($@"IL_{leader.Offset:X4} [label=""{opCodes.Select(x => x.ToString()).Aggregate((x, y) => x + "\\l" + y)}""]");

        static void PrintEdge(Instruction leader, Instruction nextLeader)
        {
            var headPort = "";
            Console.WriteLine($"IL_{leader.Offset:X4} -> IL_{nextLeader.Offset:X4}{headPort}");
        }

        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("[?] Usage: cfg.exe MyAssembly.dll Namespace.Class.Method[:n]");
                return -1;
            }

            var location = args[1].Split(new[] {'.'});
            if (location.Length < 3)
            {
                Console.WriteLine("[-] Please provide full path to the method - Namespace.Class.Method[:n]");
                return -2;
            }

            var len = location.Length;

            var className = location[len-2];
            var methodName = location[len-1];
            int skip = 0;
            if (methodName.Contains(':'))
            {
                var noOfMethod = methodName.Split(':');
                methodName = noOfMethod[0];
                int.TryParse(noOfMethod[1], out skip);
            }
            var namespaceName = String.Join(".", location.TakeWhile(x => x != className));
            AssemblyDefinition definition = AssemblyDefinition.ReadAssembly(args[0]);
            var targetMethod = definition.MainModule.Types.First(x => x.Namespace == namespaceName && x.Name == className).Methods
                .Where(x => x.Name == methodName).Skip(skip).First();

            Console.WriteLine("digraph CFG {");
            Console.WriteLine("node [shape=box]");
            //first instruction is always a leader
            var leadersToVisit = new List<Instruction> { targetMethod.Body.Instructions.First() };
            var edges = new List<Edge>();
            foreach(var instruction in targetMethod.Body.Instructions.Skip(1))
            {
                switch (instruction.OpCode.FlowControl)
                {
                    case FlowControl.Cond_Branch:
                        var leaders = instruction.Operand is Instruction[] switchTargets
                            ? switchTargets
                            : new[] {instruction.Operand as Instruction};
                        leaders = leaders.Append(instruction.Next).ToArray();
                        leadersToVisit.AddRange(leaders);
                        edges.AddRange(leaders.Select(x => new Edge {From = instruction, To = x}));
                        break;
                    case FlowControl.Branch:
                        var branchTarget = instruction.Operand as Instruction;
                        leadersToVisit.Add(branchTarget);
                        edges.Add(new Edge {From = instruction, To = instruction.Operand as Instruction});

                        var previousInstruction = (instruction.Operand as Instruction).Previous;
                        edges.Add(new Edge { From = previousInstruction, To = branchTarget });
                        break;
                    case FlowControl.Return:
                        if (instruction.Next != null)
                        {
                            leadersToVisit.Add(instruction.Next);
                        }
                        break;
                }
            }

            var distinctLeaders = leadersToVisit.Distinct(new OpCodeEqualityComparer()).ToArray();
            Array.Sort(distinctLeaders, new OpCodeComparer());
            var reverseLeadersBBs = new Dictionary<Instruction[],Instruction>();
            for(var i = 0; i< distinctLeaders.Length-1;i++)
            {
                var leader = distinctLeaders[i];
                var nextLeader = distinctLeaders[i + 1];
                var bb = targetMethod.Body.Instructions
                    .SkipWhile(x => x.Offset < leader.Offset)
                    .TakeWhile(x => x.Offset < nextLeader.Offset)
                    .ToArray();
                reverseLeadersBBs.Add(bb, leader);
                PrintBB(leader, bb);
            }

            var lastLeader = distinctLeaders.Last();
            var lastLeaderBB = targetMethod.Body.Instructions.SkipWhile(x => x.Offset < lastLeader.Offset).ToArray();
            PrintBB(lastLeader, lastLeaderBB);
            reverseLeadersBBs.Add(lastLeaderBB, lastLeader);
            foreach (var edge in edges)
            {
                var leader = reverseLeadersBBs.First(x=>x.Key.Select(y=>y.Offset)
                                                            .Contains(edge.From.Offset)).Value;
                PrintEdge(leader, edge.To);
            }
            Console.WriteLine("}");
            return 0;
        }
    }
}
