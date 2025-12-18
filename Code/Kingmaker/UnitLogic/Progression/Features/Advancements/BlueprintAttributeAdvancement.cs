using System;
using Kingmaker.EntitySystem.Stats.Base;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Features.Advancements;

[Serializable]
[TypeId("d6f5095e34f14990a98461b6cd77d321")]
public class BlueprintAttributeAdvancement : BlueprintStatAdvancement
{
	[SerializeField]
	private AttributeType Attribute;

	public override int ValuePerRank => 5;

	public override StatType Stat => Attribute.ToStatType();
}
