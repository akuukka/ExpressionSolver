using System.Collections;
using System.Collections.Generic;

namespace AK
{
	public static class ExpressionSolverTests
	{
		public static void AssertSameValue(double f1, double f2)
		{
			var diff = System.Math.Abs(f1-f2);
			if (diff>0.0000001f)
			{
				throw new System.Exception("ExpressionSolverTest failed");
			}
		}

		public static void Run()
		{
			TestGlobalConstants();
			TestExpLocalConstants();
			TestUndefinedVariablePolicies();
			TestSum();
			TestFuncs();
		}

		public static void TestFuncs()
		{
			ExpressionSolver solver = new ExpressionSolver();
			solver.SetGlobalVariable("zero",0);
			var exp1 = solver.SymbolicateExpression("sin(pi/2)-cos(zero)");
			AssertSameValue(exp1.Evaluate(),0);
			var exp2 = solver.SymbolicateExpression("2*e^zero - exp(zero)");
			AssertSameValue(exp2.Evaluate(),1);
			var exp3 = solver.SymbolicateExpression("log(e^6)");
			AssertSameValue(exp3.Evaluate(),6);
			var exp4 = solver.SymbolicateExpression("sqrt(2)-2^0.5");
			AssertSameValue(exp4.Evaluate(),0);
			var exp5 = solver.SymbolicateExpression("exp(log(6))");
			AssertSameValue(exp5.Evaluate(),6);
		}

		public static void TestSum()
		{
			const int N = 10000;
			ExpressionSolver solver = new ExpressionSolver();
			var exp = solver.SymbolicateExpression("1/2^i","i");
			double sum = 0;
			for (int i=0;i<N;i++)
			{
				exp.SetVariable("i",i);
				sum += exp.Evaluate();
			}
			AssertSameValue(sum,2);
			sum = 0;
			var variable = exp.GetVariable("i");
			for (int i=0;i<N;i++)
			{
				variable.value = i;
				sum += exp.Evaluate();
			}
			AssertSameValue(sum,2);
		}

		public static void TestUndefinedVariablePolicies()
		{
			ExpressionSolver solver = new ExpressionSolver();
			try
			{
				solver.SymbolicateExpression("test");
				throw new System.Exception("ExpressionSolverTest failed");
			}
			catch (AK.ESUnknownExpressionException)
			{
				// Expected behaviour
			}

			solver.undefinedVariablePolicy = ExpressionSolver.UndefinedVariablePolicy.DefineGlobalVariable;
			var exp2 = solver.SymbolicateExpression("test2");
			AssertSameValue(solver.GetGlobalVariable("test2").value,0);
			AssertSameValue(exp2.Evaluate(),0);

			solver.undefinedVariablePolicy = ExpressionSolver.UndefinedVariablePolicy.DefineExpressionLocalVariable;
			var exp3 = solver.SymbolicateExpression("sin(test3)");
			var test3 = exp3.GetVariable("test3");
			AssertSameValue(test3.value,0);
			test3.value = System.Math.PI/2;
			AssertSameValue(exp3.Evaluate(),1);

		}

		public static void TestGlobalConstants()
		{
			ExpressionSolver solver = new ExpressionSolver();
			solver.SetGlobalVariable("test",1);
			var exp1 = solver.SymbolicateExpression("test+1");
			AssertSameValue(2.0,exp1.Evaluate());
			solver.SetGlobalVariable("test",2);
			var exp2 = solver.SymbolicateExpression("test+1");
			AssertSameValue(3.0,exp2.Evaluate());
			AssertSameValue(exp1.Evaluate(),exp2.Evaluate());
		}

		public static void TestExpLocalConstants()
		{
			ExpressionSolver solver = new ExpressionSolver();
			var exp1 = solver.SymbolicateExpression("test+1",new string[]{"test"});
			exp1.SetVariable("test",1.0);
			var exp2 = solver.SymbolicateExpression("test+1","test"); // If you define only one variable, you don't need to use the string[] format
			exp2.SetVariable("test",2.0);
			solver.SetGlobalVariable("test",1000); // If there is name clash with a exp-local variable, we prefer the exp-local variable.
			AssertSameValue(exp1.Evaluate(),2);
			AssertSameValue(exp2.Evaluate(),3);
			var exp3 = solver.SymbolicateExpression("test^2");
			AssertSameValue(exp3.Evaluate(),1000*1000);
		}

	}

}