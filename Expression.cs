using System.Collections;
using System.Collections.Generic;

namespace AK
{
	public class Expression
	{
		public Symbol root;
		public Dictionary<string, Variable> constants = new Dictionary<string, Variable>();

		public Variable SetVariable(string name, double value)
		{
			Variable v;
			if (constants.TryGetValue(name,out v))
			{
				v.value = value;
				return v;
			}
			v = new Variable(name,value);
			constants.Add(name,v);
			return v;
		}

		public Variable GetConstant(string name)
		{
			return constants[name];
		}

		public override string ToString()
		{
			if (root.type == SymbolType.SubExpression)
			{
				var s = root.ToString();
				return s.Substring(1,s.Length-2);
			}
			else
			{
				return root.ToString();
			}
		}

		public double Evaluate()
		{
			return ExpressionSolver.GetSymbolValue(root);
		}
	}
}