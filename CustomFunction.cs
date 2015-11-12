namespace AK
{
	
	public class CustomFunction
	{
		public string name;
		public System.Func<double[],double> func;
		public int paramCount;
		
		public CustomFunction(string name, int paramCount, System.Func<double[],double> func)
		{
			this.func = func;
			this.paramCount = paramCount;
			this.name = name;
		}
	}
	
}