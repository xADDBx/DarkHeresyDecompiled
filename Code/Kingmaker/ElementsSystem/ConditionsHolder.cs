using JetBrains.Annotations;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.ElementsSystem;

[TypeId("b9ea3359b1204b798a61750d6cb4e723")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class ConditionsHolder : ElementsScriptableObject
{
	[NotNull]
	public ConditionsChecker Conditions = new ConditionsChecker();

	public bool Check()
	{
		return Conditions.Check();
	}
}
