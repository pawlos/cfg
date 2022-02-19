using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace cfg.Model.Helpers;

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
