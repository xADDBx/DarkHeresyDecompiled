using System;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[ComponentName("Root/DamageSkillCheckRoot")]
[TypeId("95b599bbf82e498b9c31dbdd4293b338")]
public class DamageSkillCheckRoot : BlueprintScriptableObject
{
	public SkillCheckRoot.CRAndDamagePair[] DamageCRPair = new SkillCheckRoot.CRAndDamagePair[0];
}
