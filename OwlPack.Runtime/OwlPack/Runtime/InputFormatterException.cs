using System;

namespace OwlPack.Runtime;

public class InputFormatterException : Exception
{
	public InputFormatterException(string message)
		: base(message)
	{
	}
}
