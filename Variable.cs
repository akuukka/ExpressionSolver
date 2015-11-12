namespace AK
{

	public class Variable
	{
		public double value;
		public string name;
		
		public Variable(string name)
		{
			this.value=0;
			this.name = name;
		}
		
		public Variable(string name, double v)
		{
			this.value=v;
			this.name = name;
		}
	}

}