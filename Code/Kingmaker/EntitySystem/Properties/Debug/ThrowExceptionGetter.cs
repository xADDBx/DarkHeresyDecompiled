using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Debug;

[Serializable]
[TypeId("22dbdf57708a4afd8603a76c37654f3b")]
public class ThrowExceptionGetter : PropertyGetter
{
	public string Message = "test";

	public override bool IsBool => false;

	protected override int GetBaseValueInternal()
	{
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Throw exception: " + Message;
	}
}
