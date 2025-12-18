using System;
using JetBrains.Annotations;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Blueprints;

[TypeId("f73264d322814b5f90d973478a501cff")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class RestrictionsHolder : BlueprintScriptableObject
{
	[Serializable]
	[OwlPackable(OwlPackableMode.NoGenerate)]
	public class Reference : BlueprintReference<RestrictionsHolder>
	{
	}

	[SerializeField]
	[NotNull]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	public bool IsPassed(PropertyContext context)
	{
		return m_Restrictions.IsPassed(context);
	}

	public bool IsPassed([NotNull] MechanicEntity currentEntity, MechanicsContext context = null, TargetWrapper currentTarget = null, RulebookEvent rule = null, AbilityData ability = null)
	{
		return m_Restrictions.IsPassed(context, currentEntity, currentTarget, rule, ability);
	}
}
