using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace cfg;

class OpCodeComparer : IComparer<Instruction>
{
    public int Compare(Instruction x, Instruction y)
    {
        return x?.Offset - y?.Offset ?? 0;
    }
}
