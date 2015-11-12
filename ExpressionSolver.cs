using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AK
{

	public static class SolverTools
	{
		public static int countParameters(string formula,int begin,int end) {
			int depth = 0;
			int r = 1;
			for (int i=begin;i<end;i++) {
				if (formula[i] == '(') {
					depth++;
				}
				else if (formula[i] == ')') {
					depth--;
				}
				else if (formula[i] == ',' && depth == 1) {
					r++;
				}
			}
			return r;
		}
		
		public static int parseUntilEndOfExponent(string formula, int begin, int end) {
			int currentDepth = 0;
			for (int i=begin;i<end;i++) {
				if (formula[i] == '(') {
					currentDepth++;
				}
				else if (formula[i] == ')') {
					currentDepth--;
					if (currentDepth == -1)
						return i;
				}
				else if (currentDepth==0) {
					if (i>begin && formula[i]=='-')
						return i;
					else if (i>begin && formula[i]=='+')
						return i;
				}
			}
			return end;
		}
	}

	public class ExpressionSolver
	{
		private static Dictionary<string,double> immutableGlobalConstants = new Dictionary<string, double>()
		{
			{"e",System.Math.E},
			{"pi",System.Math.PI}
		};

		private Dictionary<string,Variable> globalConstants = new Dictionary<string, Variable>();

		public Variable SetGlobalVariable(string name, double value)
		{
			if (!globalConstants.ContainsKey(name))
			{
				Variable v = new Variable(name,value);
				globalConstants.Add (name,v);
				return v;
			}
			globalConstants[name].value = value;
			return globalConstants[name];
		}

		public Expression SymbolicateExpression(string formula, string[] localVariables = null) {
			Expression newExpression = new Expression();
			if (localVariables != null)
			{
				foreach (var localVariableName in localVariables)
				{
					newExpression.SetVariable(localVariableName.Trim(),0.0);
				}
			}
			Symbol s = Symbolicate(formula, 0, formula.Length,newExpression);
			newExpression.root = s;
			return newExpression;
		}

		static double ParseSymbols(SymbolList syms, int threadId = 0) {
			
			bool transformNextValue = false;
			double sum = 0;
			double curTerm = 0;
			
			SymbolType prevOper = SymbolType.OperatorAdd;
			
			int len = syms.symbols.Count;
			var symbolList = syms.symbols;
			
			for (int i=0;i<len;i++) {
				var s = symbolList[i];
				
				switch (s.type) {
				case SymbolType.Value:
				case SymbolType.SubExpression: {
					double value = GetSymbolValue(s);
					if (transformNextValue) {
						var funcSymbol = symbolList[i-1];
						switch (funcSymbol.type) {
						case SymbolType.FuncSin:
							value = System.Math.Sin(value);
							break;
						case SymbolType.FuncAbs:
							value = System.Math.Abs(value);
							break;
						case SymbolType.FuncCos:
							value = System.Math.Cos(value);
							break;
						case SymbolType.FuncPow:
						{
							var rhs = GetSymbolValue(symbolList[i+1]);
							value = System.Math.Pow(value,rhs);
							i++;
						}
							break;
						case SymbolType.FuncCustom:
							{
								/*var customFunc = m_env.getCustomFunctions().at(funcSymbol.m_customFuncNameHash);
								if (customFunc.getParameterCount()==1) {
									value = customFunc.getValue(value);
								}
								else if (customFunc.getParameterCount()==2) {
									var param2 = getSymbolValue(symbolList[i+1],threadId);
									value = customFunc.getValue(value,param2);
									i++;
									break;
								}*/
								throw new System.NotImplementedException();
							}
							break;
						default:
							break;
						}
						transformNextValue = false;
					}
					
					switch (prevOper) {
					case SymbolType.OperatorMultiply:
						curTerm *= value;
						break;
					case SymbolType.OperatorDivide:
						curTerm /= value;
						break;
					case SymbolType.OperatorAdd:
						sum += curTerm;
						curTerm = value;
						break;
					default:
						throw new System.Exception("Unable to parse symbols.");
					}
					prevOper = SymbolType.OperatorMultiply;
					break;
				}
				case SymbolType.OperatorDivide:
					
				case SymbolType.OperatorAdd:
					prevOper = s.type;
					break;
				case SymbolType.FuncCos:
				case SymbolType.FuncSin:
				case SymbolType.FuncAbs:
				case SymbolType.FuncPow:
				case SymbolType.FuncCustom:
					transformNextValue = true;
					break;
				default:
					throw new System.Exception("Unable to parse symbols.");
				}
			}
			// Remember to add the final term to sum
			return sum + curTerm;
		}

		public static double GetSymbolValue(Symbol s) 
		{
			if (s.type == SymbolType.Value)
			{
				return s.value;
			}
			var syms = s.subExpression;
			double value = ParseSymbols(syms,0);
			return value;
		}

		Symbol parseBuiltInFunction(string formula,int begin,int end) {
			int nameLength = end-begin;
			// Check for built-in functions
			var functionName = formula.Substring(begin,nameLength);
			if (functionName[0] == 's' && nameLength == 3 && functionName[1] == 'i' && functionName[2] == 'n') {
				return new Symbol(SymbolType.FuncSin);
			}
			else if (functionName[0] == 'c' && nameLength == 3 	&& functionName[1] == 'o' && functionName[2] == 's') {
				return new Symbol(SymbolType.FuncCos);
			}
			else if (functionName[0] == 'a' && nameLength == 3 	&& functionName[1] == 'b' && functionName[2] == 's') {
				return new Symbol(SymbolType.FuncAbs);
			}
			
			return new Symbol(SymbolType.Empty);
		}

		Symbol SymbolicateValue(string formula, int begin, int end, Expression exp) {

			
			if (formula[begin] == '+')
				begin++;
			
			// Check if the value contains power operator. If yes, return the return symbol is symbollist of type pow(left,right) where left and right are symbols we get by recursively calling this
			// function for the parts of formula left and rigth from the pow operaotr
			// But take parenthesis into account so that power operators inside function arguments do not cause trouble
			int depth=0;
			for (int k = begin; k < end; k++) {
				if (formula[k]=='(')
					depth++;
				else if (formula[k]==')')
					depth--;
				else if (depth == 0 && formula[k] == '^') {
					// Check for small integer powers: they will be done using multiplication instead!
					
					Symbol lhs = Symbolicate(formula,begin,k,exp);
					Symbol rhs = Symbolicate(formula,k+1,end,exp);
					var newSubExpression = new SymbolList();
					if (end-k-1 == 1 && lhs.type == SymbolType.Value && formula.Substring(k+1,end-k-1)=="2") {
						// Second power found
						newSubExpression.Append(lhs);
						newSubExpression.Append(lhs);
					}
					else if (end-k-1 == 1 && lhs.type == SymbolType.Value && formula.Substring(k+1,end-k-1)=="3") {
						// Second power found
						newSubExpression.Append(lhs);
						newSubExpression.Append(lhs);
						newSubExpression.Append(lhs);
					}
					else {
						newSubExpression.Append(new Symbol(SymbolType.FuncPow));
						newSubExpression.Append(lhs);
						newSubExpression.Append(rhs);
					}
					Symbol newSymbol = new Symbol(SymbolType.SubExpression);
					newSymbol.subExpression = newSubExpression;
					return newSymbol;
				}
			}
			
			if (formula[begin] == '(' && formula[end - 1] == ')') {
				var s = Symbolicate(formula, begin + 1, end - 1,exp);
				s.Simplify();
				return s;
			}
			

			double valueAsRealNumber;
			if (double.TryParse(formula.Substring(begin,end-begin),out valueAsRealNumber)) {
				return new Symbol(valueAsRealNumber);
			}
			
			// Check if the value is transformed by a function
			if (formula[end-1]==')') {
				int i = begin;
				while (i < end-1) {
					if (formula[i]=='(') {
						break;
					}
					i++;
				}
				Symbol s = parseBuiltInFunction(formula,begin,i);
				switch (s.type) {
				case SymbolType.FuncCos:
				case SymbolType.FuncAbs:
				case SymbolType.FuncSin: {
					Symbol argument = Symbolicate(formula,i+1,end-1,exp);
					var newSubExpression = new SymbolList();
					newSubExpression.Append(new Symbol(s.type));
					newSubExpression.Append(argument);
					Symbol newSymbol = new Symbol(SymbolType.SubExpression);
					newSymbol.subExpression = newSubExpression;
					return newSymbol;
				}
				default:
				{
					// Get hash value for the value name
					throw new System.NotImplementedException();
					/*
					int hash = stringHash(formula,begin,i);
					auto globalFuncs = m_env.getCustomFunctions();
					auto it = globalFuncs.find(hash);
					if (it != globalFuncs.end()) {
						var func = it->second;
						int requiredParameterCount = func.getParameterCount();
						int foundParameterCount = countParameters(formula,i,end);
						if (requiredParameterCount == foundParameterCount) {
							if (requiredParameterCount == 1) {
								Symbol argument = symbolicate(formula,i+1,end-1,parentExpression);
								SymbolList<T>* newSubExpression = parentExpression.initSymbolList();
								newSubExpression.Append(Symbol.createCustomFunctionSymbol(func.getName()));
								newSubExpression.Append(argument);
								Symbol newSymbol = new Symbol(SymbolType.SubExpression);
								newSymbol.m_ptrToSubExpression = newSubExpression;
								return newSymbol;
							}
							else {
								std::vector<std::pair<int32,int32> > parameters = parseParameters(formula,i,end);
								assert(parameters.size() == (size_t)requiredParameterCount);
								
								SymbolList<T>* newSubExpression = parentExpression.initSymbolList();
								newSubExpression->append(Symbol::createCustomFunctionSymbol(func.getName()));
								for (int32 i=0;i<requiredParameterCount;i++) {
									Symbol p = symbolicate(formula,parameters[i].first,parameters[i].second,parentExpression);
									newSubExpression->append(p);
								}
								
								Symbol newSymbol(SymbolType::SubExpression);
								newSymbol.m_ptrToSubExpression = newSubExpression;
								return newSymbol;
								
							}
						}
						else {
							std::string errorMessage = "Function " + func.getName() + " requires " + std::to_string(requiredParameterCount) + std::string(" parameters, ") + std::to_string(foundParameterCount) + std::string(" found.");
							throw FunctionParameterException(errorMessage);
						}
					}
					*/
				}
					throw new System.Exception("Unknown expression: " + formula.Substring(begin,i-begin));
					break;
				}
			}
			
			// Get hash value for the value name
			var valueName = formula.Substring(begin,end-begin);//formula,begin,end

			// Then immutable globals
			if (immutableGlobalConstants.ContainsKey(valueName)) {
				return new Symbol(immutableGlobalConstants[valueName]);
			}

			// Then non immutable globals
			if (globalConstants.ContainsKey(valueName)) {
				return new Symbol(globalConstants[valueName]);
			}

			// Then a local constant specific to our expression
			if (exp.constants.ContainsKey(valueName))
			{
				return new Symbol(exp.constants[valueName]);
			}

			throw new System.Exception("Unknown expression: " + valueName);
		}

		Symbol SymbolicateMonome(string formula, int begin, int end, Expression exp)
		{
			Symbol s = new Symbol(SymbolType.SubExpression);
			var symbols = new SymbolList();
			int sign = 0;
			int i = begin - 1;
			int currentTermBegin = begin;
			int numValues = 0;
			int currentDepth = 0;
			double constMultiplier = 1.0;
			bool divideNext = false;
			bool constMultiplierUsed = false;
			for (;;) {
				i++;
				if (i == end || (currentDepth == 0 && i > begin && (formula[i] == '*' || formula[i] == '/'))) {
					numValues++;
					
					// Unless we are dealing with a monome, symbolicate the term
					Symbol newSymbol = SymbolicateValue(formula, formula[currentTermBegin] == '-' ? currentTermBegin + 1 : currentTermBegin, i,exp);
					// Check if we can simplify the generated symbol
					if (newSymbol.IsImmutableConstant()) {
						// Constants are multiplied/divided together
						if (divideNext)
							constMultiplier /= GetSymbolValue(newSymbol);
						else
							constMultiplier *= GetSymbolValue(newSymbol);
						constMultiplierUsed = true;
					}
					else {
						if (divideNext)
							symbols.Append(new Symbol(SymbolType.OperatorDivide));
						newSymbol.Simplify();
						symbols.Append(newSymbol);
					}

					if (i == end) {
						break;
					}
					divideNext = formula[i] == '/';
					currentTermBegin = i + 1;
				}
				else if (formula[i] == '(') {
					currentDepth++;
				}
				else if (formula[i] == ')') {
					currentDepth--;
				}
				else if (formula[i] == '-' && currentDepth == 0 && !(i>begin && formula[i-1] == '^') ) {
					sign++;
				}
			}
			
			// If the generated monome has negative number of minus signs, then we append *-1 to end of the list, or if the preceding symbol is constant real number that is part of a monome, we multiply it.
			if (sign % 2 == 1) {
				constMultiplier =-constMultiplier;
			}
			if (constMultiplierUsed || sign % 2 == 1) {
				// Add the const multiplier to the expression
				if (symbols.Length > 0 && symbols.last.IsMonome() && symbols.last.type == SymbolType.SubExpression )
				{
					// Add inside the last subexpression
					SymbolList leftSideExpression = symbols.last.subExpression;
					if (leftSideExpression.last.type==SymbolType.Value && leftSideExpression.last.IsImmutableConstant()) {
						leftSideExpression.SetSymbolAtIndex(leftSideExpression.Length-1,new Symbol(leftSideExpression.last.value*constMultiplier));
					}
					else {
						leftSideExpression.Append(new Symbol(constMultiplier));
					}
				}
				else if (symbols.Length>0 && symbols.first.type==SymbolType.OperatorDivide) {
					// Put to the begin of the expression we are building
					symbols.symbols.Insert(0,new Symbol(constMultiplier));
				}
				else {
					// Put to the end of the expression we are building
					symbols.Append(new Symbol(constMultiplier));
				}
			}

			// Check if the final monome is just a real number, in which case we don't have to return a subexpression type
			if (symbols.Length == 1 && symbols.first.IsImmutableConstant())
			{
				return new Symbol(symbols.getSymbol(0).value);
			}
			
			s.subExpression = symbols;
			
			s.Simplify();
			return s;
		}

		Symbol Symbolicate(string formula, int begin, int end, Expression exp) 
		{
			var symbols = new SymbolList();

			int i = begin - 1;
			int currentTermBegin = formula[begin] == '+' ? begin + 1 : begin;
			int currentDepth = 0;
			
			for (;;) {
				i++;
				if (i == end || (currentDepth == 0 && i > begin && (formula[i - 1] != '*' && formula[i - 1] != '/') && (formula[i] == '+' || formula[i] == '-'))) {
					// Unless we are dealing with a monome, symbolicate the term
					try {
						symbols.Append(SymbolicateMonome(formula, currentTermBegin, i,exp));
					}
					catch (System.Exception ex) 
					{
						throw ex;
					}
					
					if (i == end) {
						break;
					}
					else {
						// The sign of the term is included in the next monome only if its minus
						currentTermBegin = (formula[i] == '-') ? i : i + 1;
						symbols.Append(new Symbol(SymbolType.OperatorAdd));
					}
				}
				else if (formula[i] == '(') {
					currentDepth++;
				}
				else if (formula[i] == ')') {
					currentDepth--;
				}
				else if (formula[i] == '^') {
					i = SolverTools.parseUntilEndOfExponent(formula,i+1,end) - 1;
				}
			}
			
			// If at this point we only have one real number left, just return it as a simple value.
			if (symbols.Length == 1 && symbols.getSymbol(0).type == SymbolType.Value) {
				Symbol s = symbols.getSymbol(0);
				return s;
			}
			
			// We don't have that single expression, but:
			// Now that we are here, we have symbol list which consists of only addition operators and value types. This is a great place to sum constant values together!
			double constantSum = 0.0f;
			bool addedConstants = false;

			for (int j = 0; j < symbols.Length; j++) {
				Symbol s = symbols.getSymbol(j);
				if (s.IsImmutableConstant()) {
					constantSum += s.value;
					addedConstants = true;
					if (j == symbols.Length - 1) {
						// Destroy preceding +
						symbols.symbols.RemoveAt (j);
						break;
					}
					symbols.symbols.RemoveAt(j);
					symbols.symbols.RemoveAt(j);
					j--;
				}
				else {
					// Skip the following + symbol
					j++;
				}
			}
			if (addedConstants) {
				if (symbols.Length > 0 && symbols.getSymbol(symbols.Length - 1).IsValueType()) {
					symbols.Append(new Symbol(SymbolType.OperatorAdd));
				}
				symbols.Append(new Symbol(constantSum));
			}
			
			// Finally, if the symbolicated sum is just a single real number, even varying, return just a simple symbol
			if (symbols.Length == 1 && symbols.getSymbol(0).type == SymbolType.Value) {
				Symbol s = symbols.getSymbol(0);
				return s;
			}
			
			// Optimization: get rid of unnecessary jumps to subexpressions
			for (int j=0;j<symbols.Length;j++) {
				var s = symbols.getSymbol(j);
				if (s.type==SymbolType.SubExpression) {
					var subExpression = s.subExpression;
					int subExpressionLength = subExpression.Length;
					for (int k=0;k<subExpressionLength;k++) {
						symbols.InsertBefore(j+k,subExpression.getSymbol(k));
					}
					j += subExpressionLength;
					// The symbols of the subexpression were copied into parent symbollist
					// Therefore we can destroy the subexpression symbollist, but only after setting it to an empty list
					//symbols.symbols.(symbols.symbols.begin()+i);
					symbols.symbols.RemoveAt (j);
					j--;
				}
			}
			
			// We have turned the formula into a subexpression symbol
			Symbol returnSymbol = new Symbol(SymbolType.SubExpression);
			returnSymbol.subExpression = symbols;
			returnSymbol.Simplify();
			return returnSymbol;
		}



	}

}