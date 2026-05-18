using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Serializable]
[Obsolete]
[TypeId("781c3882fb08d5445b1588a4b0f4d9c7")]
public abstract class EtudeBracketOverrideInteraction : EtudeBracketTrigger, IEtudeBracketOverrideInteraction
{
	[SerializeField]
	private bool m_AllowInCombat;

	[SerializeField]
	private int m_Distance = 2;

	public override bool RequireLinkedArea => true;

	public int Distance => m_Distance;

	public abstract bool IsDialog { get; }

	public bool AllowInCombat => m_AllowInCombat;

	public abstract AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target);
}
