using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints;

[Serializable]
[TypeId("50180856b8ea4cf6965e53bb91472358")]
public class BlueprintBroken : BlueprintScriptableObject
{
	[NonSerialized]
	public Exception Exception;
}
