using System;
using System.Collections.Generic;
using System.Text;

namespace LL1
{
	partial class Program
	{
		static void _RunLesson2()
		{
			// our regular expression engine does not have its own parser
			// therefore we must create the expressions manually by using
			// the appropriate construction methods.

			// create a new lexer with the following five expressions:
			// four self titled literals +, *, (, and )
			// one regex [0-9]+ as "int"

			// note that the symbols we use here match the terminals used in our 
			// CFG grammar from lesson 1. This is important.

			_lexer = new FA();
			_lexer.EpsilonTransitions.Add(FA.Literal("+", "+"));
			_lexer.EpsilonTransitions.Add(FA.Literal("*", "*"));
			_lexer.EpsilonTransitions.Add(FA.Literal("(", "("));
			_lexer.EpsilonTransitions.Add(FA.Literal(")", ")"));
			_lexer.EpsilonTransitions.Add(FA.Repeat(FA.Set("0123456789"), "int"));
			Console.WriteLine("Lesson 2 - FA Lexer");
			// there's no easy way to show the contents of this machine so we'll just show the total states
			Console.WriteLine("NFA machine containes {0} total states", _lexer.FillClosure().Count);
			Console.WriteLine();
		}
	}
}
