using System.Collections;
using System.Collections.Generic;

namespace AK {

	public static class ValidityChecker
	{
		private static Dictionary<char,string> acceptedPreceders = new Dictionary<char, string>()
		{
			{'-',"*()/^0123456789abcdefghijklmnopqrstuvxyzABCDEFGHIJKLMNOPQRSTUVXYZ_" },
			{'+',")0123456789abcdefghijklmnopqrstuvxyzABCDEFGHIJKLMNOPQRSTUVXYZ_" },
			{'/',")0123456789abcdefghijklmnopqrstuvxyzABCDEFGHIJKLMNOPQRSTUVXYZ_" },
			{'^',")0123456789abcdefghijklmnopqrstuvxyzABCDEFGHIJKLMNOPQRSTUVXYZ_" },
			{'*',")0123456789abcdefghijklmnopqrstuvxyzABCDEFGHIJKLMNOPQRSTUVXYZ_" },
			{')',")0123456789abcdefghijklmnopqrstuvxyzABCDEFGHIJKLMNOPQRSTUVXYZ_" },
			{'(',"*-+^/(0123456789abcdefghijklmnopqrstuvxyzABCDEFGHIJKLMNOPQRSTUVXYZ_" }
		};

		private static bool CanPrecede(char l, char r)
		{
			string accepted;
			if (acceptedPreceders.TryGetValue(r,out accepted)) {
				foreach (var c in accepted)
				{
					if (c==l)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static void ThrowSyntaxErrorAt(string expression, int index) {
			int i = index;
			int l = expression.Length;
			int from = System.Math.Max(0,i-3);
			int to = System.Math.Min(l,i+4);
			int len = to-from;
			string str = "Syntax error: ";
			if (from>0) {
				str+="...";
			}
			str += expression.Substring(from,len);
			if (to<l) {
				str+="...";
			}
			throw new ESSyntaxErrorException(str);
		}

		private static bool CanBeBeginOfRValue(char c) {
			if (c >= '0' && c <= '9')
				return true;
			if (c == '(')
				return true;
			if (c == '-')
				return true;
			if (c == '+')
				return true;
			if (c >= 'a' && c <='z')
				return true;
			if (c >= 'A' && c <='Z')
				return true;
			if (c >= '_')
				return true;
			return false;
		}

		public static void CheckValidity(string expression)
		{
			int parenthesisDepth = 0;
			int l = expression.Length;
			for (int i=0;i<l;i++)
			{
				var x = expression[i];
				switch (x) {
					case '(':
						parenthesisDepth++;
						if (i>0 && !CanPrecede(expression[i-1],expression[i])) {
							ThrowSyntaxErrorAt(expression,i);
						}
						break;
					case ')':
						if (parenthesisDepth == 0) {
							throw new ESSyntaxErrorException("Parenthesis mismatch.");
						}
						if (i>0 && !CanPrecede(expression[i-1],expression[i])) {
							ThrowSyntaxErrorAt(expression,i);
						}
						parenthesisDepth--;
						break;
					case '/':
					case '*':
					case '+':
					case '^':
					case '-':
						if (i==l-1)
							ThrowSyntaxErrorAt(expression,i);
						if (i==0 && !(x=='-' || x=='+') )
							ThrowSyntaxErrorAt(expression,i);
						if (!CanBeBeginOfRValue(expression[i+1])) {
							ThrowSyntaxErrorAt(expression,i);
						}
						if (i>0 && !CanPrecede(expression[i-1],expression[i])) {
							ThrowSyntaxErrorAt(expression,i);
						}
						if ( (x == '+' || x=='-') && i < l-2) {
							if ( (expression[i+2]=='+' || expression[i+2]=='-') && (expression[i+1]=='+' || expression[i+1]=='-') ) {
								ThrowSyntaxErrorAt(expression,i);
							}
						}
						break;
					case ',':
						if (i==l-1)
							ThrowSyntaxErrorAt(expression,i);
						if (!CanBeBeginOfRValue(expression[i+1])) {
							ThrowSyntaxErrorAt(expression,i);
						}
						break;
					case '.':
						if (i==l-1)
							ThrowSyntaxErrorAt(expression,i);
						if (! (expression[i+1] >= '0' && expression[i+1] <= '9') )
							ThrowSyntaxErrorAt(expression,i);
						break;
					default:
						if (x >= '0' && x<= '9')
							break;
						if (x >= 'a' && x<= 'z')
							break;
						if (x >= 'A' && x<= 'Z')
							break;
						if (x == '_')
							break;
						throw new ESInvalidCharacterException(expression.Substring(i,1));
				}
			}
			if (parenthesisDepth > 0) {
				throw new ESSyntaxErrorException("Parenthesis mismatch.");
			}
		}

	}
}
