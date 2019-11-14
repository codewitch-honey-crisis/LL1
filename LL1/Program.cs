using System;
using System.Collections.Generic;
using System.Text;

namespace LL1
{
	partial class Program
	{
		// shared state for demo
		// (don't use statics like this
		// in production code)
		static Cfg _cfg;
		static FA _lexer;

		static void Main(string[] args)
		{
			// see the individial Program.cs files under each lesson folder
			_RunLesson1();
			_RunLesson2();
			_RunLesson3();

		}
	}
}
