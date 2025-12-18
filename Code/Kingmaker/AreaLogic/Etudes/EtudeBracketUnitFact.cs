using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("84536e9de79985246a8668a54daa52d5")]
public class EtudeBracketUnitFact : EtudeBracketTrigger, IAreaActivationHandler, ISubscriber, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>
{
	[SerializeField]
	[ValidateNotNull]
	private AbstractUnitEvaluator m_TargetUnit;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintMechanicEntityFact.Reference m_TargetFact;

	private BlueprintMechanicEntityFact TargetFact => m_TargetFact;

	[CanBeNull]
	private BaseUnitEntity EvaluateTarget()
	{
		if (!m_TargetUnit.TryGetValue(out var value))
		{
			return null;
		}
		return value as BaseUnitEntity;
	}

	private void TryAddFact()
	{
		BaseUnitEntity baseUnitEntity = EvaluateTarget();
		if (baseUnitEntity != null && baseUnitEntity.Facts.FindBySource(TargetFact, EtudeBracketTrigger.Etude) == null)
		{
			baseUnitEntity.Facts.Add(TargetFact.CreateFact(null, null))?.AddSource(EtudeBracketTrigger.Etude);
		}
	}

	private void TryRemoveFact()
	{
		BaseUnitEntity baseUnitEntity = EvaluateTarget();
		EntityFact fact = baseUnitEntity?.Facts.FindBySource(TargetFact, EtudeBracketTrigger.Etude);
		baseUnitEntity?.Facts.Remove(fact);
	}

	protected override void OnEnter()
	{
		TryAddFact();
	}

	protected override void OnExit()
	{
		TryRemoveFact();
	}

	public void OnAreaActivated()
	{
		TryAddFact();
	}

	public void HandleUnitSpawned()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && m_TargetUnit.Is(baseUnitEntity))
		{
			TryAddFact();
		}
	}

	public void HandleUnitDestroyed()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && m_TargetUnit.Is(baseUnitEntity))
		{
			TryRemoveFact();
		}
	}

	public void HandleUnitDeath()
	{
	}
}
