using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cfg.Formatters;

internal class dotFormatter : IFormatter
{
    public dotFormatter(Action<string> printer)
    {
        Printer = printer;
    }

    public void PrintHeader()
    {
        Printer("digraph CFG {");
        Printer("node [shape=box]");
    }

    public void PrintFooter()
    {
        Printer("}");
    }

    public void PrintBB(Instruction leader, IEnumerable<Instruction> opCodes)
    {
        Printer($@"IL_{leader.Offset:X4} [label=""{opCodes.Select(x => x.ToString().Replace(@"""", @"\""")).Aggregate((x, y) => x + "\\l" + y)}""]");
    }

    public void PrintEdge(Instruction leader, Instruction nextLeader)
    {
        var headPort = "";
        Printer($"IL_{leader.Offset:X4} -> IL_{nextLeader.Offset:X4}{headPort}");
    }

    private Action<string> Printer { get; }
}

