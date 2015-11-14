namespace AK
{
	
	public class CustomFunction
	{
		public string name;
		public System.Func<double[],double> funcmd;
		public System.Func<double,double> func1d;
		public int paramCount;
		public bool isRandom;
		
		public CustomFunction(string name, int paramCount, System.Func<double[],double> func, bool isRandom)
		{
			this.funcmd = func;
			this.isRandom = isRandom;
			this.paramCount = paramCount;
			this.name = name;
		}

		public CustomFunction(string name, System.Func<double,double> func, bool isRandom)
		{
			this.func1d = func;
			this.isRandom = isRandom;
			this.paramCount = 1;
			this.name = name;
		}

		public double Invoke(double[] p)
		{
			return funcmd(p);
		}

		public double Invoke(double x)
		{
			return func1d != null ? func1d(x) : funcmd(new double[]{x});
		}

	}
	
}