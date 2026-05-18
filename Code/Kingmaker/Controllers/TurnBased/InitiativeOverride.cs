using System;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Fmw.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

[Serializable]
public class InitiativeOverride : IHashable
{
	public int InnerPriority;

	public AbstractUnitEvaluator UnitEvaluator;

	public int PercentPosition = -1;

	public bool EtudeEnforcement;

	public BpRef<BlueprintUnitFact> Source;

	[NonSerialized]
	public MechanicEntity FollowEntity;

	public bool Persistent;

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}
}
