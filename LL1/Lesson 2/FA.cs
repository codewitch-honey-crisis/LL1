using System;
using System.Collections.Generic;
using System.Text;

namespace LL1
{
	/// <summary>
	/// Represents a remedial bare bones "regular expression" engine.
	/// </summary>
	/// <remarks>There's just enough here to make it work, not to make it fast or fancy.</remarks>
	class FA : ICloneable
	{
		/// <summary>
		/// The symbol to return when this state accepts, or null if it does not accept.
		/// </summary>
		public string AcceptingSymbol { get; set; } = null;
		/// <summary>
		/// The input transitions
		/// </summary>
		public IDictionary<char, FA> Transitions { get; } = new Dictionary<char, FA>();
		/// <summary>
		/// The epsilon transitions (transitions on no input)
		/// </summary>
		public ICollection<FA> EpsilonTransitions { get; } = new List<FA>();
		
		/// <summary>
		/// Computes the set of all states reachable from this state, including itself. Puts the result in the <paramref name="result"/> field amd returns the same collection."/>
		/// </summary>
		/// <param name="result">The collection to fill, or null for one to be created</param>
		/// <returns>Either <paramref name="result"/> or a new collection filled with the result of the closure computation.</returns>
		public IList<FA> FillClosure(IList<FA> result = null)
		{
			if (null == result)
				result = new List<FA>();
			if (!result.Contains(this))
			{
				result.Add(this);
				foreach (var fa in Transitions.Values)
					fa.FillClosure(result);
				foreach (var fa in EpsilonTransitions)
					fa.FillClosure(result);
			}
			return result;
		}
		/// <summary>
		/// Computes the set of all states reachable from this state on no input, including itself. Puts the result in the <paramref name="result"/> field amd returns the same collection."/>
		/// </summary>
		/// <param name="result">The collection to fill, or null for one to be created</param>
		/// <returns>Either <paramref name="result"/> or a new collection filled with the result of the epsilon closure computation.</returns>
		public IList<FA> FillEpsilonClosure(IList<FA> result = null)
		{
			if (null == result)
				result = new List<FA>();
			if (!result.Contains(this))
			{
				result.Add(this);
				foreach (var fa in EpsilonTransitions)
					fa.FillEpsilonClosure(result);
			}
			return result;
		}
		/// <summary>
		/// Creates a clone of this FA state
		/// </summary>
		/// <returns>A new FA that is equal to this FA</returns>
		public FA Clone()
		{
			var closure = FillClosure();
			var nclosure = new FA[closure.Count];
			for(var i = 0;i<nclosure.Length;i++)
			{
				nclosure[i] = new FA();
				nclosure[i].AcceptingSymbol = closure[i].AcceptingSymbol;
			}
			for (var i = 0; i < nclosure.Length; i++)
			{
				var t = nclosure[i].Transitions;
				var e = nclosure[i].EpsilonTransitions;
				foreach (var trns in closure[i].Transitions)
				{
					var id = closure.IndexOf(trns.Value);
					t.Add(trns.Key, nclosure[id]);
				}
				foreach (var trns in closure[i].EpsilonTransitions)
				{
					var id = closure.IndexOf(trns);
					e.Add(nclosure[id]);
				}
			}
			return nclosure[0];
		}
		object ICloneable.Clone() => Clone();
		/// <summary>
		/// Creates an FA that matches a literal string
		/// </summary>
		/// <param name="string">The string to match</param>
		/// <param name="accept">The symbol to accept</param>
		/// <returns>A new FA machine that will match this literal</returns>
		public static FA Literal(IEnumerable<char> @string,string accept = "")
		{
			var result = new FA();
			var current = result;
			foreach(char ch in @string)
			{
				current.AcceptingSymbol = null;
				var fa = new FA();
				fa.AcceptingSymbol = accept;
				current.Transitions.Add(ch, fa);
				current = fa;
			}
			return result;
		}
		/// <summary>
		/// Creates an FA that will match any one of a set of a characters
		/// </summary>
		/// <param name="set">The set of characters that will be matched</param>
		/// <param name="accept">The symbol to accept</param>
		/// <returns>An FA that will match the specified set</returns>
		public static FA Set(IEnumerable<char> set, string accept = "")
		{
			var result = new FA();
			var final = new FA();
			final.AcceptingSymbol = accept;
			foreach (char ch in set)
				result.Transitions.Add(ch, final);
			return result;
		}
		/// <summary>
		/// Creates a new FA that is a concatenation of two other FA expressions
		/// </summary>
		/// <param name="exprs">The FAs to concatenate</param>
		/// <param name="accept">The symbol to accept</param>
		/// <returns>A new FA that is the concatenation of the specified FAs</returns>
		public static FA Concat(IEnumerable<FA> exprs, string accept = "")
		{
			FA left = null;
			var right = left;
			foreach(var val in exprs)
			{
				if (null == val) continue;
				var nval = val.Clone();
				if (null == left)
				{
					left = nval;
					continue;
				}
				else if (null == right)
					right = nval;
				else
					_Concat(right, nval);
				
				_Concat(left, right);
			}
			right.FirstAcceptingState.AcceptingSymbol = accept;
			return left;
		}
		static void _Concat(FA lhs,FA rhs)
		{
			var f = lhs.FirstAcceptingState;
			lhs.FirstAcceptingState.EpsilonTransitions.Add(rhs);
			f.AcceptingSymbol = null;
		}
		/// <summary>
		/// Creates a new FA that matche any one of the FA expressions passed
		/// </summary>
		/// <param name="exprs">The expressions to match</param>
		/// <param name="accept">The symbol to accept</param>
		/// <returns>A new FA that will match the union of the FA expressions passed</returns>
		public static FA Or(IEnumerable<FA> exprs,string accept = "")
		{
			var result = new FA();
			var final = new FA();
			final.AcceptingSymbol = accept;
			foreach(var fa in exprs)
			{
				fa.EpsilonTransitions.Add(fa);
				var nfa = fa.Clone();
				var nffa = fa.FirstAcceptingState;
				nfa.FirstAcceptingState.EpsilonTransitions.Add(final);
				nffa.AcceptingSymbol = null;
			}
			return result;
		}
		/// <summary>
		/// Creates a new FA that will match a repetition of one or more of the specified FA expression
		/// </summary>
		/// <param name="expr">The expression to repeat</param>
		/// <param name="accept">The symbol to accept</param>
		/// <returns>A new FA that matches the specified FA one or more times</returns>
		public static FA Repeat(FA expr, string accept = "")
		{
			var result = expr.Clone();
			result.FirstAcceptingState.EpsilonTransitions.Add(result);
			result.FirstAcceptingState.AcceptingSymbol = accept;
			return result;
		}
		/// <summary>
		/// Creates a new FA that matches the specified FA expression or empty
		/// </summary>
		/// <param name="expr">The expression to make optional</param>
		/// <param name="accept">The symbol to accept</param>
		/// <returns>A new FA that will match the specified expression or empty</returns>
		public static FA Optional(FA expr, string accept = "")
		{
			var result = expr.Clone();
			var f = result.FirstAcceptingState;
			f.AcceptingSymbol = accept;
			result.EpsilonTransitions.Add(f);
			return result;
		}
		/// <summary>
		/// Creates a new FA that will match a repetition of zero or more of the specified FA expressions
		/// </summary>
		/// <param name="expr">The expression to repeat</param>
		/// <param name="accept">The symbol to accept</param>
		/// <returns>A new FA that matches the specified FA zero or more times</returns>
		public static FA Kleene(FA expr,string accept = "")
		{
			return Optional(Repeat(expr),accept);
		}
		/// <summary>
		/// Returns the first state that accepts from a given FA, or null if none do.
		/// </summary>
		public FA FirstAcceptingState {
			get {
				foreach(var fa in FillClosure())
					if (null != fa.AcceptingSymbol)
						return fa;
				return null;
			}
		}
		/// <summary>
		/// Fills a collection with the result of moving each of the specified <paramref name="states"/> by the specified input.
		/// </summary>
		/// <param name="states">The states to examine</param>
		/// <param name="input">The input to use</param>
		/// <param name="result">The states that are now entered as a result of the move</param>
		/// <returns><paramref name="result"/> or a new collection if it wasn't specified.</returns>
		public static ICollection<FA> FillMove(IEnumerable<FA> states, char input,ICollection<FA> result = null) {
			if (null == result) result = new List<FA>();
			foreach(var fa in states)
			{
				// examine each of the states reachable from this state on no input
				foreach(var efa in fa.FillEpsilonClosure())
				{
					FA ofa;
					// see if this state has this input in its transitions
					if (efa.Transitions.TryGetValue(input, out ofa))
						if (!result.Contains(ofa)) // if it does, add it if it's not already there
							result.Add(ofa);
				}
			}
			return result;
		}
	}
}
