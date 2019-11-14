using System;
using System.Collections.Generic;
using System.Text;

namespace LL1
{
	/// <summary>
	/// Represents a single token returned from the <see cref="Tokenizer"/>'s <see cref="TokenEnumerator"/>
	/// A token contains the symbol, the value, and the location information for each lexeme returned from a lexer/tokenizer
	/// </summary>
	struct Token
	{
		public string Symbol { get; internal set; }
		public int Line { get; internal set; }
		public int Column { get; internal set; }
		public long Position { get; internal set; }
		public int Length { get; internal set; }
		public string Value { get; internal set; }
	}
}
