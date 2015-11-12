using System.Collections;
using System.Collections.Generic;

namespace AK
{
	public class SymbolList
	{
		public List<Symbol> symbols = new List<Symbol>();
		
		public override string ToString ()
		{
			var l = this;
			string r = "";
			for (int i=0;i<l.symbols.Count;i++) {
				var s = l.getSymbol(i);
				switch (s.type) {
				case SymbolType.FuncSin:
					r+=("sin(");
					r+=(l.getSymbol(i+1)).ToString();
					r+=(")");
					i++;
					break;
				case SymbolType.FuncCos:
					r+=("cos(");
					r+=(l.getSymbol(i+1));
					r+=(")");
					i++;
					break;
				case SymbolType.FuncAbs:
					r+=("abs(");
					r+=(l.getSymbol(i+1));
					r+=(")");
					i++;
					break;
				case SymbolType.FuncPow:
					r+=(l.getSymbol(i+1));
					r+=("^");
					r+=(l.getSymbol(i+2));
					i+=2;
					break;
				case SymbolType.FuncCustom:
				{
					/*var customFunc = m_env.getCustomFunctions().at(s.m_customFuncNameHash);
					printf("%s(",customFunc.getName().c_str());
					printSymbol(l.getSymbol(i+1));
					if (customFunc.getParameterCount()>1) {
						for (int j=1;j<customFunc.getParameterCount();j++) {
							printf(",");
							printSymbol(l.getSymbol(i+1+j));
						}
					}
					printf(")");
					i+= customFunc.getParameterCount();*/
					r+="customfunc";
				}
					break;
				case SymbolType.Value:
				case SymbolType.SubExpression:
					r+=(s);
					if (i<l.symbols.Count-1) {
						if (l.getSymbol(i+1).IsValueType()) {
							r+=("*");
						}
					}
					break;
				default:
					r+=(s);
					break;
				}
			}
			return r;
		}
		
		public int Length
		{
			get { return symbols.Count; }
		}
		
		public void SetSymbolAtIndex(int index, Symbol s)
		{
			symbols[index] = s;
		}
		
		public Symbol last 
		{
			get { return symbols[symbols.Count-1]; }
		}
		
		public Symbol first {
			get { return symbols[0]; }
		}
		
		public int Append(Symbol s) {
			symbols.Add(s);
			return 1;
		}
		
		public void InsertBefore(int index, Symbol s) {
			symbols.Insert(index,s);
		}
		
		public Symbol getSymbol(int index) {
			return symbols[index];
		}
		
	}
}
