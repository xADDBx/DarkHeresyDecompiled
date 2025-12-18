using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Code.GameCore.ElementsSystem.Debug;

[Serializable]
[TypeId("702fcb2beb1849bcb8b878f2dd02e476")]
public class ThrowExceptionCondition : Condition
{
	public string Message = "test";

	protected override string GetConditionCaption()
	{
		return "Throw exception: " + Message;
	}

	protected override bool CheckCondition()
	{
		return true;
	}
}
