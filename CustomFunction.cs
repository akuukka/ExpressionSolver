namespace AK
{
	
	public class CustomFunction
	{
		public string name;
		public System.Func<double[],double> func;
		public int paramCount;
		public bool isRandom;
		
		public CustomFunction(string name, int paramCount, System.Func<double[],double> func, bool isRandom)
		{
			this.func = func;
			this.isRandom = isRandom;
			this.paramCount = paramCount;
			this.name = name;
		}
	}
	
}