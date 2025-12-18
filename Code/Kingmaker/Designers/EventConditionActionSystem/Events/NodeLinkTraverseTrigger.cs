using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

public class NodeLinkTraverseTrigger : MonoBehaviour
{
	[Serializable]
	public class ContextParameterPair
	{
		public ContextPropertyName Parameter;

		public int Value;
	}

	[SerializeField]
	private BlueprintAbilityReference m_CastAbilityOnTraverse;

	[SerializeReference]
	[CanBeNull]
	public PositionEvaluator CastPosition;

	[SerializeField]
	private bool CastOnPoint;

	[SerializeReference]
	[CanBeNull]
	[ShowIf("CastOnPoint")]
	public PositionEvaluator TargetPoint;

	[SerializeField]
	private ContextParameterPair[] Parameters;

	private IWarhammerNodeLink m_NodeLink;

	public BlueprintAbility CastAbilityOnTraverse => m_CastAbilityOnTraverse;

	private void Awake()
	{
		m_NodeLink = GetComponent<IWarhammerNodeLink>();
	}

	private void OnEnable()
	{
		if (m_NodeLink != null)
		{
			m_NodeLink.OnTransitionCompleted += RunActionsOnTraverse;
		}
	}

	private void OnDisable()
	{
		if (m_NodeLink != null)
		{
			m_NodeLink.OnTransitionCompleted -= RunActionsOnTraverse;
		}
	}

	private void RunActionsOnTraverse(ILinkTraversalProvider traverser)
	{
		SimpleCaster free = SimpleCaster.GetFree();
		if (free != null && CastPosition != null)
		{
			free.Position = CastPosition.GetValue();
		}
		TargetWrapper abilityTarget = (CastOnPoint ? ((TargetWrapper)(TargetPoint?.GetValue())) : ((TargetWrapper)traverser.Traverser));
		RulePerformAbility obj = new RulePerformAbility(new AbilityData(CastAbilityOnTraverse, free), abilityTarget)
		{
			IgnoreCooldown = true,
			ForceFreeAction = true
		};
		SetupContextParameters(obj, Parameters);
		Rulebook.Trigger(obj);
		obj.Context.HitPolicy = AttackHitPolicyType.AutoHit;
		obj.Context.RewindActionIndex();
	}

	private static void SetupContextParameters(RulePerformAbility rule, IEnumerable<ContextParameterPair> parameters)
	{
		if (parameters == null || rule == null)
		{
			return;
		}
		foreach (ContextParameterPair parameter in parameters)
		{
			rule.Context[parameter.Parameter] = parameter.Value;
		}
	}
}
