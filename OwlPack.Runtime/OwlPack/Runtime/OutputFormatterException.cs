using System;

namespace OwlPack.Runtime;

public class OutputFormatterException : Exception
{
	public OutputFormatterException(string message)
		: base(message)
	{
	}
}
