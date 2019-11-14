using System;
using System.Collections.Generic;
using System.Text;

namespace LL1
{
	partial class Program
	{
		static void _RunLesson3()
		{
			var text = "3+5*7";

			Console.WriteLine("Lesson 3 - Runtime Parser");
			Console.WriteLine();
			Console.WriteLine("Reading expression \"{0}\"", text);

			// create a parser using our parse table and lexer, and input text
			var parser = new Parser(
				_cfg.ToParseTable(),
				new Tokenizer(_lexer, text),
				"E");

			// read the nodes
			while (parser.Read())
			{
				if (ParserNodeType.NonTerminal == parser.NodeType && parser.Symbol == "")
					System.Diagnostics.Debugger.Break();
				Console.WriteLine("{0}\t{1}: {2}, Line {3}, Columm {4}", parser.NodeType, parser.Symbol, parser.Value, parser.Line, parser.Column);
			}
			Console.WriteLine();
			Console.WriteLine("Parse tree for \"{0}\"", text);
			// parse again
			parser = new Parser(
				_cfg.ToParseTable(),
				new Tokenizer(_lexer, text),
				_cfg.StartSymbol);
			// ... this time into a tree
			Console.WriteLine(parser.ParseSubtree());
		}
	}
}
