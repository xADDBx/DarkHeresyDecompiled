using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Newtonsoft.Json;
using Owlcat.UI;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class MechanicActionBarSlot : IHashable, IOwlPackable, IOwlPackable<MechanicActionBarSlot>
{
	[JsonProperty]
	[OwlPackInclude]
	protected EntityRef<BaseUnitEntity> m_UnitRef;

	private bool m_IsCastingActive;

	public bool HoverState;

	[CanBeNull]
	public abstract MechanicEntityFact AbilityFact { get; }

	public BaseUnitEntity Unit
	{
		get
		{
			return m_UnitRef.Entity;
		}
		set
		{
			m_UnitRef = value;
		}
	}

	public abstract string KeyName { get; }

	protected virtual bool IsNotAvailable => false;

	protected int ResourceCount { get; private set; }

	protected int ResourceCost { get; private set; }

	protected int ResourceAmount { get; private set; }

	public virtual bool IsPossibleActive
	{
		get
		{
			bool flag = !IsDisabled(GetResource()) && !IsNotAvailable && (!TurnController.IsInTurnBasedCombat() || CanUseIfTurnBased());
			if (flag && UtilityNet.InLobbyAndPlaying)
			{
				flag = ((Game.Instance.CurrentModeType == GameModeType.SpaceCombat) ? UtilityNet.IsControlMainCharacter() : (Unit != null && Unit.IsMyNetRole()));
			}
			return flag;
		}
	}

	public bool IsPossibleActiveWithoutNetRole
	{
		get
		{
			if (!IsDisabled(GetResource()) && !IsNotAvailable)
			{
				if (TurnController.IsInTurnBasedCombat())
				{
					return CanUseIfTurnBased();
				}
				return true;
			}
			return false;
		}
	}

	public virtual bool IsDisabled(int resourceCount)
	{
		if (!Unit.LifeState.IsConscious)
		{
			return true;
		}
		if (resourceCount == -1)
		{
			return false;
		}
		return resourceCount == 0;
	}

	private static bool CanEndTurnAndNoActing()
	{
		if (Game.Instance.Controllers.TurnController.CurrentUnit is BaseUnitEntity { IsDirectlyControllable: not false } baseUnitEntity)
		{
			return baseUnitEntity.Commands.Empty;
		}
		return false;
	}

	protected virtual bool CanUseIfTurnBased()
	{
		if (Game.Instance.Controllers.TurnController.CurrentUnit != Unit || Unit.IsProne || !CanEndTurnAndNoActing())
		{
			return false;
		}
		return true;
	}

	public virtual void OnClick()
	{
		PlaySound();
		TryShowWarning(Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(Unit));
		EventBus.RaiseEvent(delegate(IClickMechanicActionBarSlotHandler h)
		{
			h.HandleClickMechanicActionBarSlot(this);
		});
	}

	public void PlaySound()
	{
		UISounds.Instance.Play(IsPossibleActive ? UISounds.Instance.Sounds.Combat.ActionBarSlotClick : UISounds.Instance.Sounds.Combat.ActionBarCanNotSlotClick);
	}

	public virtual void TryShowWarning(Vector3 castPosition)
	{
		if (IsPossibleActive)
		{
			return;
		}
		string message = WarningMessage(castPosition);
		if (!string.IsNullOrEmpty(message))
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(message, addToLog: true, WarningNotificationFormat.Attention);
			});
		}
	}

	protected virtual string WarningMessage(Vector3 castPosition)
	{
		if (!CanUseIfTurnBased())
		{
			return UIStrings.Instance.TurnBasedTexts.NotEnoughActionsMessage;
		}
		return string.Empty;
	}

	public virtual bool IsActive()
	{
		return false;
	}

	public virtual int ActionPointCost()
	{
		return -1;
	}

	public virtual void OnHover(bool state)
	{
		HoverState = state;
	}

	public abstract int GetResource();

	public abstract int GetResourceCost();

	public abstract int GetResourceAmount();

	public virtual bool HasWeaponAbilityGroup()
	{
		return false;
	}

	public abstract object GetContentData();

	public virtual bool IsBad()
	{
		return false;
	}

	public abstract Sprite GetIcon();

	public virtual Sprite GetForeIcon()
	{
		return null;
	}

	public virtual bool NeedUpdate()
	{
		return true;
	}

	public abstract string GetTitle();

	public abstract string GetDescription();

	public void UpdateResourceCount()
	{
		if (Unit != null)
		{
			ResourceCount = GetResource();
		}
	}

	public void UpdateResourceCost()
	{
		if (Unit != null)
		{
			ResourceCost = GetResourceCost();
		}
	}

	public void UpdateResourceAmount()
	{
		if (Unit != null)
		{
			ResourceAmount = GetResourceAmount();
		}
	}

	public abstract bool IsCasting();

	public virtual IEnumerable<AbilityData> GetConvertedAbilityData()
	{
		return Enumerable.Empty<AbilityData>();
	}

	public void TrySetAbilityToPrediction(bool state)
	{
	}

	public virtual TooltipBaseTemplate GetTooltipTemplate()
	{
		return null;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<BaseUnitEntity> obj = m_UnitRef;
		Hash128 val = StructHasher<EntityRef<BaseUnitEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		return result;
	}

	public abstract void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter;

	public abstract void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
}
