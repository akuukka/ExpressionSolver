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
		
		public double Evaluate()
		{
			return ExpressionSolver.GetSymbolValue(root);
		}
	}
}