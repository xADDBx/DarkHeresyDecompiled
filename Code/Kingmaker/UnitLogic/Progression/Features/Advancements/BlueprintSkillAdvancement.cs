using System;
using Kingmaker.EntitySystem.Stats.Base;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Features.Advancements;

[Serializable]
[TypeId("bd939c4ccfa84c4188d8ea01b8bc2592")]
public class BlueprintSkillAdvancement : BlueprintStatAdvancement
{
	[SerializeField]
	private SkillType m_Skill;

	public override int ValuePerRank => 10;

	public override StatType Stat => m_Skill.ToStatType();
}
