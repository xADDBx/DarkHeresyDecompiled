using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics;

public enum ContextValueType
{
	[FormerlySerializedAs("Simple")]
	Const,
	CasterProperty,
	TargetProperty,
	CasterCustomProperty,
	TargetCustomProperty,
	CasterNamedProperty,
	TargetNamedProperty,
	ContextProperty
}
