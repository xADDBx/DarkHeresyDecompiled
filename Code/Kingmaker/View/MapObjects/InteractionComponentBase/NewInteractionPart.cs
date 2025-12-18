using System;
using System.Collections.Generic;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.Interaction;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View.MapObjects.Traps;
using Kingmaker.Visual.Animation.Kingmaker;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionComponentBase;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class NewInteractionPart : AbstractInteractionPart, IHashable, IOwlPackable<NewInteractionPart>
{
	[JsonProperty]
	[OwlPackInclude]
	protected bool m_Enabled = true;

	public override InteractionType Type => InteractionType.Approach;

	public override UIInteractionType UIInteractionType => UIInteractionType.Info;

	public override int ApproachRadius => 2;

	public override float OvertipRevealDistance => Math.Max(ApproachRadius.Cells().Meters, 6.35f);

	public override List<BaseUnitEntity> UnitsCanInteract => null;

	public override int ActionPointsCost => 0;

	public override UnitAnimationInteractionType UseAnimationState => UnitAnimationInteractionType.UseObject;

	public override bool ShowOvertip => true;

	public override float OvertipVerticalCorrection => 0f;

	public override bool ShowHighlight => false;

	public override bool NotInCombat => true;

	public override string InteractionStopSound => null;

	public override InteractionSettings.InteractWithToolFXData InteractWithMeltaChargeFXData => null;

	public override TrapObjectData Trap => null;

	protected override bool UnlimitedInteractionsPerRound => false;

	public override bool Enabled
	{
		get
		{
			return m_Enabled;
		}
		set
		{
			if (m_Enabled != value)
			{
				m_Enabled = value;
				m_Enabled = Enabled;
				base.View.Or(null)?.UpdateHighlight();
				OnEnabledChanged();
				EventBus.RaiseEvent((IMapObjectEntity)base.Owner, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
				{
					h.HandleObjectInteractChanged();
				}, isCheckRuntime: true);
			}
		}
	}

	protected sealed override InteractionProcess InteractInternal(BaseUnitEntity user)
	{
		using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
		{
			InteractionProcess interactionProcess = OnInteract(user);
			interactionProcess.Run();
			return interactionProcess;
		}
	}

	protected abstract InteractionProcess OnInteract(BaseUnitEntity user);

	protected virtual void OnEnabledChanged()
	{
	}

	public override bool HasVisibleTrap()
	{
		TrapObjectData trap = Trap;
		if (trap != null && trap.IsAwarenessCheckPassed)
		{
			return trap.TrapActive;
		}
		return false;
	}

	public override void PlayStartSound(BaseUnitEntity user)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Enabled);
		return result;
	}
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class NewInteractionPart<TSettings> : NewInteractionPart, IHashable, IOwlPackable<NewInteractionPart<TSettings>> where TSettings : class, new()
{
	protected TSettings Settings { get; private set; } = new TSettings();


	public override void SetSource(IAbstractEntityPartComponent source)
	{
		IAbstractEntityPartComponent source2 = base.Source;
		base.SetSource(source);
		Settings = source.GetSettings() as TSettings;
		OnSettingsDidSet(source2 != source);
		ConfigureRestrictions();
	}

	protected virtual void OnSettingsDidSet(bool isNewSettings)
	{
	}

	protected virtual void ConfigureRestrictions()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
