using UnityEngine;
using System.Collections;

namespace AK
{
	
	public enum SymbolType
	{
		Empty,
		Value,
		OperatorAdd,
		OperatorMultiply,
		OperatorDivide,
		SubExpression,
		FuncPow,
		FuncSin,
		FuncCos,
		FuncAbs,
		FuncCustom,
	};
	
	public class Symbol
	{
		public SymbolType type;
		public double _value;
		public Variable m_ptrToConstValue;
		public SymbolList subExpression;
		public int m_customFuncNameHash;
		
		public bool IsImmutableConstant()
		{
			if (type == SymbolType.Value)
			{
				if (m_ptrToConstValue != null)
					return false;
				return true;
			}
			else if (type == SymbolType.SubExpression)
			{
				return IsSymbolListImmutableConstant(subExpression);
			}
			else
			{
				return false;
			}
		}

		public bool IsMonome() {
			var s = this;
			if (s.type==SymbolType.Value)
			{
				return true;
			}
			else if (s.type==SymbolType.SubExpression) 
			{
				var syms = s.subExpression;
				for (int i=0;i<syms.Length;i++)
				{
					var r = syms.getSymbol(i);
					if (r.type!=SymbolType.Value && r.type!=SymbolType.SubExpression)
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		public void Simplify() {
			// ((x)) ==> (x)
			if (type == SymbolType.SubExpression) {
				if (subExpression.Length == 1 && subExpression.first.type == SymbolType.SubExpression) {
					// Get pointer to sub-subexpression
					SymbolList subSubExpression = subExpression.symbols[0].subExpression;
					subExpression = subSubExpression;
				}
				else if (subExpression.Length == 1 && subExpression.first.type == SymbolType.Value) {
					// We have single real number surrounded by parenthesis, it can become a real number
					CopyValuesFrom(subExpression.first);
				}
			}
		}
		
		private bool IsSymbolListImmutableConstant(SymbolList l)
		{
			for (int k = 0; k < l.Length; k++) {
				var s = l.getSymbol(k);
				if (s.type == SymbolType.Value) {
					if (s.m_ptrToConstValue != null) {
						return false;
					}
				}
				else if (l.getSymbol(k).type == SymbolType.SubExpression) {
					if (!IsSymbolListImmutableConstant(s.subExpression)) {
						return false;
					}
				}
			}
			return true;
		}
		
		public void CopyValuesFrom(Symbol o)
		{
			type = o.type;
			_value = o._value;
			m_ptrToConstValue = o.m_ptrToConstValue;
			subExpression = o.subExpression;
			m_customFuncNameHash = o.m_customFuncNameHash;
		}
		
		public double value 
		{
			get
			{
				return m_ptrToConstValue == null ? _value : m_ptrToConstValue.value;
			}
		}
		
		public bool IsValueType() {
			return type == SymbolType.Value || type == SymbolType.SubExpression;
		}
		
		public Symbol()
		{
			type = SymbolType.Empty;
			m_ptrToConstValue = null;
		}
		
		public Symbol(SymbolType type, double va)
		{
			this.type = type;
			_value = va;
			m_ptrToConstValue = null;
		}
		
		public Symbol(SymbolType type) {
			this.type = type;
		}
		
		public Symbol(double value) {
			type = SymbolType.Value;
			_value = value;
			m_ptrToConstValue = null;
		}
		
		public Symbol(Variable ptrToConstValue) {
			type = SymbolType.Value;
			m_ptrToConstValue = ptrToConstValue;
		}
		
		static Symbol createCustomFunctionSymbol(string funcName) {
			Symbol s = new Symbol(SymbolType.FuncCustom);
			s.m_customFuncNameHash = funcName.GetHashCode();
			return s;
		}
		
		public override string ToString()
		{
			switch (type) {
			case SymbolType.Value:
				if (m_ptrToConstValue != null)
				{
					return m_ptrToConstValue.name;
				}
				return _value.ToString();
			case SymbolType.OperatorAdd:
				return "+";
			case SymbolType.OperatorMultiply:
				return "*";
			case SymbolType.OperatorDivide:
				return "/";
			case SymbolType.FuncSin:
				return "sin";
			case SymbolType.FuncCos:
				return "cos";
			case SymbolType.FuncPow:
				return "pow";
			case SymbolType.FuncAbs:
				return "abs";
			case SymbolType.SubExpression:
				return "("+subExpression.ToString()+")";
			case SymbolType.Empty:
				return "(null)";
			case SymbolType.FuncCustom:
				return "customfunc";
			}
			return "";
		}
	}

}
