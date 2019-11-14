using System;
using System.Collections.Generic;
using System.Text;

namespace LL1
{
	partial class Program
	{
		static void _RunLesson1()
		{
			// Create a new CFG with the following rules:

			// E -> T E'
			// E'-> + T E'
			// E'->
			// T -> F T'
			// T'-> * F T'
			// T'->
			// F -> (E)
			// F -> int

			_cfg = new Cfg();
			_cfg.Rules.Add(new CfgRule("E", "T", "E'"));
			_cfg.Rules.Add(new CfgRule("E'", "+", "T", "E'"));
			_cfg.Rules.Add(new CfgRule("E'"));
			_cfg.Rules.Add(new CfgRule("T", "F", "T'"));
			_cfg.Rules.Add(new CfgRule("T'", "*", "F", "T'"));
			_cfg.Rules.Add(new CfgRule("T'"));
			_cfg.Rules.Add(new CfgRule("F", "(", "E", ")"));
			_cfg.Rules.Add(new CfgRule("F", "int"));
			Console.WriteLine("Lesson 1 - CFG Grammar");
			Console.WriteLine(_cfg);
			Console.WriteLine();
		}
	}
}
