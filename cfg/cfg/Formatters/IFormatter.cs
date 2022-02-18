using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace cfg.Formatters;

internal interface IFormatter
{
    void PrintHeader();
    void PrintFooter();
    void PrintBB(Instruction leader, IEnumerable<Instruction> opCodes);
    void PrintEdge(Instruction leader, Instruction nextLeader);
}

