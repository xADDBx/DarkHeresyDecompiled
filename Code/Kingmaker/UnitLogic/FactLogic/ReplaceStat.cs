using System;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[Obsolete]
[TypeId("5892c65f961d47f7b67f04073017fc22")]
public abstract class ReplaceStat : MechanicEntityFactComponentDelegate
{
	protected enum Attributes
	{
		Unknown,
		WarhammerBallisticSkill,
		WarhammerWeaponSkill,
		WarhammerStrength,
		WarhammerToughness,
		WarhammerAgility,
		WarhammerIntelligence,
		WarhammerPerception,
		WarhammerWillpower,
		WarhammerFellowship,
		WarhammerMedicae
	}

	[SerializeField]
	protected StatType m_OriginalStat;

	[SerializeField]
	protected Attributes m_NewAttribute;

	[SerializeField]
	protected bool m_OnlyIfHigher;

	[SerializeField]
	[ShowIf("m_OnlyIfHigher")]
	protected Attributes m_PreviousAttributeToCompare;

	protected StatType PreviousAttributeToCompare => GetStat(m_PreviousAttributeToCompare);

	public StatType OriginalStat => m_OriginalStat;

	protected static StatType GetStat(Attributes attribute)
	{
		return attribute switch
		{
			Attributes.WarhammerBallisticSkill => StatType.BallisticSkill, 
			Attributes.WarhammerWeaponSkill => StatType.WeaponSkill, 
			Attributes.WarhammerStrength => StatType.Strength, 
			Attributes.WarhammerToughness => StatType.Toughness, 
			Attributes.WarhammerAgility => StatType.Agility, 
			Attributes.WarhammerIntelligence => StatType.Intelligence, 
			Attributes.WarhammerPerception => StatType.Perception, 
			Attributes.WarhammerWillpower => StatType.Willpower, 
			Attributes.WarhammerFellowship => StatType.Fellowship, 
			Attributes.WarhammerMedicae => StatType.SkillMedicae, 
			Attributes.Unknown => StatType.Unknown, 
			_ => throw new ArgumentOutOfRangeException("attribute", attribute, null), 
		};
	}
}
