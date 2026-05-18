using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("a2e11c4716d04cc49a14c053c99825e4")]
public class WarhammerStudyTarget : UnitBuffComponentDelegate, ITurnBasedModeHandler, ISubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>
{
	[SerializeField]
	private BlueprintBuffReference m_StudiedBuff;

	public bool Permanent;

	public ContextDurationValue DurationValue;

	public BlueprintBuff StudiedBuff => m_StudiedBuff?.Get();

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			base.Buff.Remove();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
	}
}
