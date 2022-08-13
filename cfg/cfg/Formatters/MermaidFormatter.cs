using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cfg.Formatters
{
    internal class MermaidFormatter : IFormatter
    {
        private readonly Action<string> printer;
        private readonly Dictionary<Instruction, IEnumerable<Instruction>> instructions;
        public MermaidFormatter(Action<string> printer)
        {
            this.printer = printer ?? throw new ArgumentNullException(nameof(printer));
            instructions = new Dictionary<Instruction, IEnumerable<Instruction>>();
        }
        public void PrintBB(Instruction leader, IEnumerable<Instruction> opCodes)
        {
            instructions.Add(leader, opCodes);
        }

        public void PrintEdge(Instruction leader, Instruction nextLeader)
        {
            var opCodes = instructions[leader];
            var opcodesStrings = opCodes.Select(x => x.ToString().Replace(@"""", @"\""")).Aggregate((x, y) => x + "<br>" + y);         
            printer(@$"IL_{leader.Offset:X4}(""{opcodesStrings}"") --> IL_{nextLeader.Offset:X4}");
        }

        public void PrintFooter()
        {
            //empty
        }

        public void PrintHeader()
        {
            printer("graph TD;");
        }
    }
}
