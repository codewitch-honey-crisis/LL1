using System;
using System.Collections.Generic;
using System.Text;

namespace LL1
{
	/// <summary>
	/// An enumeration indicating the node types of the parser
	/// </summary>
	public enum ParserNodeType
	{
		/// <summary>
		/// Indicates the initial state.
		/// </summary>
		Initial = 0,
		/// <summary>
		/// Parser is on a non-terminal
		/// </summary>
		NonTerminal = 1,
		/// <summary>
		/// Parser is ending a non-terminal node
		/// </summary>
		EndNonTerminal = 2,
		/// <summary>
		/// Parser is on a terminal node
		/// </summary>
		Terminal = 3,
		/// <summary>
		/// Parser is on an error node
		/// </summary>
		Error = 4,
		/// <summary>
		/// The parser is at the end of the document
		/// </summary>
		EndDocument = 5
	}
	/// <summary>
	/// An LL(1) parser implemented as a pull-style parser.
	/// </summary>
	/// <remarks>This interface is similar in use to <see cref="System.Xml.XmlReader"/></remarks>
	class Parser
	{
		string _startSymbol;
		IDictionary<string, IDictionary<string, CfgRule>> _parseTable;
		Tokenizer _tokenizer;
		IEnumerator<Token> _tokEnum;
		Token _errorToken;
		Stack<string> _stack;
		/// <summary>
		/// Indicates the <see cref="ParserNodeType"/> at the current position.
		/// </summary>
		public ParserNodeType NodeType {
			get {
				if (null != _errorToken.Symbol)
					return ParserNodeType.Error;
				if(_stack.Count>0)
				{
					var s = _stack.Peek();
					if (s.StartsWith("#END "))
						return ParserNodeType.EndNonTerminal;
					if (s == _tokEnum.Current.Symbol)
						return ParserNodeType.Terminal;
					return ParserNodeType.NonTerminal;
				}
				try
				{
					if("#EOS"==_tokEnum.Current.Symbol)
						return ParserNodeType.EndDocument;
				}
				catch { }
				return ParserNodeType.Initial;
			}
		}
		/// <summary>
		/// Indicates the current symbol
		/// </summary>
		public string Symbol {
			get {
				if (null != _errorToken.Symbol)
					return _errorToken.Symbol;
				if (_stack.Count > 0)
				{
					var s = _stack.Peek();
					if (s.StartsWith("#END "))
						return s.Substring(5);
					return s;
				}
				return null;
			}
		}
		/// <summary>
		/// Indicates the current line
		/// </summary>
		public int Line => (null==_errorToken.Symbol)?_tokEnum.Current.Line:_errorToken.Line;
		/// <summary>
		/// Indicates the current column
		/// </summary>
		public int Column => (null == _errorToken.Symbol) ? _tokEnum.Current.Column:_errorToken.Column;
		/// <summary>
		/// Indicates the current position
		/// </summary>
		public long Position => (null == _errorToken.Symbol) ? _tokEnum.Current.Position:_errorToken.Position;
		/// <summary>
		/// Indicates the current value
		/// </summary>
		public string Value {
			get {
				switch (NodeType)
				{
					case ParserNodeType.Error:
						return _errorToken.Value;
					case ParserNodeType.Terminal:
						return _tokEnum.Current.Value;
				}
				return null;
			}
		}
		/// <summary>
		/// Constructs a new instance of the parser
		/// </summary>
		/// <param name="parseTable">The parse table to use</param>
		/// <param name="tokenizer">The tokenizer to use </param>
		/// <param name="startSymbol">The start symbol</param>
		public Parser(IDictionary<string, IDictionary<string, CfgRule>> parseTable,
			Tokenizer tokenizer,
			string startSymbol
			)
		{
			_parseTable = parseTable;
			_tokenizer = tokenizer;
			_tokEnum = tokenizer.GetEnumerator();
			_startSymbol = startSymbol;
			_stack = new Stack<string>();
			_errorToken.Symbol = null;
		}
		/// <summary>
		/// Reads and parses the next node from the document
		/// </summary>
		/// <returns>True if there is more to read, otherwise false.</returns>
		public bool Read()
		{
			var n = NodeType;
			if (ParserNodeType.Error == n && "#EOS" == _tokEnum.Current.Symbol)
			{
				_errorToken.Symbol = null;
				_stack.Clear();
				return true;
			}
			if (ParserNodeType.Initial == n)
			{
				_stack.Push(_startSymbol);
				_tokEnum.MoveNext();
				return true;
			}
			_errorToken.Symbol = null; // clear the error status
			if(0<_stack.Count)
			{
				var sid = _stack.Peek(); 
				if(sid.StartsWith("#END "))
				{
					_stack.Pop();
					return true;
				}
				if(sid==_tokEnum.Current.Symbol) // terminal
				{
					// lex the next token
					_tokEnum.MoveNext();

					_stack.Pop();
					return true;
				}
				// non-terminal
				IDictionary<string, CfgRule> d;
				if(_parseTable.TryGetValue(sid, out d))
				{
					CfgRule rule;
					if(d.TryGetValue(_tokEnum.Current.Symbol, out rule))
					{
						_stack.Pop();

						// store the end non-terminal marker for later
						_stack.Push(string.Concat("#END ", sid));

						// push the rule's derivation onto the stack in reverse order
						var ic = rule.Right.Count;
						for (var i = ic - 1; 0 <= i;--i)
						{
							sid = rule.Right[i];
							_stack.Push(sid);
						}
						return true;
					}
					_Panic();
					return true;
				}
				_Panic();
				return true;
			}
			// last symbol must be the end of the input stream or there's a problem
			if ("#EOS" != _tokEnum.Current.Symbol)
			{
				_Panic();
				return true;
			}
			return false;
		}
		/// <summary>
		/// Parses the from the current position into a parse tree. This will read an entire sub-tree.
		/// </summary>
		/// <param name="trimEmpties">Remove non-terminal nodes that have no terminals</param>
		/// <returns>A <see cref="ParseNode"/> representing the parse tree. The reader is advanced.</returns>
		public virtual ParseNode ParseSubtree(bool trimEmpties = false)
		{
			if (!Read())
				return null;
			var nn = NodeType;
			if (ParserNodeType.EndNonTerminal==nn)
				return null;

			var result = new ParseNode();
			
			if (ParserNodeType.NonTerminal == nn)
			{
				result.Symbol = Symbol;
				while (true)
				{
					var k = ParseSubtree(trimEmpties);
					if (null != k)
					{
						if (!trimEmpties || ((null != k.Value) || 0 < k.Children.Count))
							result.Children.Add(k);
					}
					else
						break;
				}
				
				return result;
			}
			else if (ParserNodeType.Terminal == nn)
			{
				result.SetLocationInfo(Line, Column, Position);
				result.Symbol = Symbol;
				result.Value = Value;
				return result;
			}
			else if (ParserNodeType.Error == nn)
			{
				System.Diagnostics.Debug.WriteLine("Error");
				result.SetLocationInfo(Line, Column, Position);
				result.Symbol = Symbol;
				result.Value = Value;
				return result;
			}
			return null;
		}
		/// <summary>
		/// Does panic-mode error recovery
		/// </summary>
		void _Panic()
		{
			// turn off error reporting if we're already at the end.
			if ("#EOS" == _tokEnum.Current.Symbol)
			{
				_errorToken.Symbol = null;
				return;
			}
			// fill the error token
			_errorToken.Symbol = "#ERROR"; // turn on error reporting
			_errorToken.Value = "";
			_errorToken.Column = _tokEnum.Current.Column;
			_errorToken.Line = _tokEnum.Current.Line;
			_errorToken.Position= _tokEnum.Current.Position;
			string s;
			IDictionary<string, CfgRule> d;
			if (_parseTable.TryGetValue(_stack.Peek(), out d))
			{
				_errorToken.Value += _tokEnum.Current.Value;
				while (!d.Keys.Contains(s = _tokEnum.Current.Symbol) 
					&& s != "#EOS" && _tokEnum.MoveNext())
				{
					if(!d.Keys.Contains(_tokEnum.Current.Symbol))
						_errorToken.Value += _tokEnum.Current.Value;
				}
			}
			else
			{
				do
				{
					s = _tokEnum.Current.Symbol;
					_errorToken.Value += _tokEnum.Current.Value;
					if (!_tokEnum.MoveNext())
						break;

				} while ("#EOS" != s && !_stack.Contains(s));

			}
			while (_stack.Contains((s = _tokEnum.Current.Symbol)) && _stack.Peek() != s)
				_stack.Pop();
		}
	}
}
