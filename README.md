# ExpressionSolver
A C# mathematical expression solver with Unity3D compatibility

Usage:

The basic use cases:

AK.ExpressionSolver solver = new AK.ExpressionSolver();
var exp = solver.SymbolicateExpression("1+1");
var result = exp.Evaluate(); // Returns 2.0
solver.EvaluateExpression("1+1"); // Also returns 2.0


Expression is symbolicated only once: subsequent calls to Evaluate are fast.

There are two kinds of variables: global and expression-local.

Global variables must be defined before symbolication and they are shared between all expressions created
using the same ExpressionSolver object. 

AK.ExpressionSolver solver = new AK.ExpressionSolver();
solver.SetVariable("myvariable",50);
var exp = solver.SymbolicateExpression("myvariable-50");
var result = exp.Evaluate(); // Returns 0.0

If you need thread safety, use expression-local variables:

AK.ExpressionSolver solver = new AK.ExpressionSolver();
var exp1 = solver.SymbolicateExpression("x^2",new string[]{"x"}");
var exp2 = solver.SymbolicateExpression("x^3",new string[]{"x"}");

// Thread 1:
var var1 = exp1.SetVariable("x",0.0)
for (int i=0;i<=10000;i++)
{
	var1.value = i/10000;
	double val = exp1.Evaluate();
}

// Thread 2:
var var2 = exp2.SetVariable("x",0.0)
for (int i=0;i<=1000;i++)
{
	var2.value = i/10000;
	double val = exp2.Evaluate();
}

Custom functions are also supported:

solver.AddCustomFunction("Rnd",2, delegate(double[] p) {
	return UnityEngine.Random.Range(p[0],p[1]);
});

solver.EvaluateExpression("Rnd(0,1)"); // Returns something from [0,1]







ExpressionSolver is licensed under the MIT license.




Copyright (c) 2015 Antti Kuukka



Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:



The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.



THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.