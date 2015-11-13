using System.Collections;
using System.Collections.Generic;

namespace AK
{
	public class ESSyntaxErrorException : System.Exception
	{
		public ESSyntaxErrorException(string msg) : base(msg) {}
	}

	public class ESInvalidCharacterException : System.Exception
	{
		public ESInvalidCharacterException(string msg) : base(msg) {}
	}

	public class ESInvalidFunctionNameException : System.Exception
	{
		public ESInvalidFunctionNameException(string msg) : base(msg) {}
	}

	public class ESInvalidParametersException : System.Exception
	{
		public ESInvalidParametersException(string msg) : base(msg) {}
	}

	public class ESUnknownExpressionException : System.Exception
	{
		public ESUnknownExpressionException(string msg) : base(msg) {}
	}
}