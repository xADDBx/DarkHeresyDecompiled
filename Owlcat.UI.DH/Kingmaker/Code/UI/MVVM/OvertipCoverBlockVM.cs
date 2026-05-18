using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.Covers;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipCoverBlockVM : ViewModel, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEquipItemHandler, ISubscriber<IItemEntity>
{
	private readonly MechanicEntityUIState m_EntityUIState;

	private readonly ReactiveProperty<bool> m_ActingUnitHasRangedWeapon;

	private TurnController TurnController => Game.Instance.Controllers.TurnController;

	public ReadOnlyReactiveProperty<LosCalculations.CoverType?> CoverType { get; }

	public OvertipCoverBlockVM(MechanicEntityUIState mechanicEntityUIState)
	{
		m_ActingUnitHasRangedWeapon = new ReactiveProperty<bool>().AddTo(this);
		m_EntityUIState = mechanicEntityUIState;
		if (TurnController.IsInTurnBasedCombat())
		{
			MechanicEntity currentUnit = TurnController.CurrentUnit;
			m_ActingUnitHasRangedWeapon.Value = IsActingUnitHasRangedWeapon(currentUnit);
		}
		CoverType = m_EntityUIState.CoverType.CombineLatest(m_EntityUIState.IsInCombat, m_EntityUIState.IsVisibleForPlayer, m_EntityUIState.IsDeadOrUnconsciousIsDead, m_ActingUnitHasRangedWeapon, GetCoverType).ToReadOnlyReactiveProperty().AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	private LosCalculations.CoverType? GetCoverType(LosCalculations.CoverType coverType, bool isInCombat, bool isVisibleForPlayer, bool isDead, bool isRangedWeapon)
	{
		if (!isInCombat || !isVisibleForPlayer || isDead || coverType == LosCalculations.CoverType.Obstacle || m_EntityUIState.IsOvertipPartHiddenBySettings(UnitOvertipUIPart.Cover))
		{
			return null;
		}
		if (coverType == LosCalculations.CoverType.Cover && !isRangedWeapon)
		{
			return null;
		}
		return coverType;
	}

	private bool IsActingUnitHasRangedWeapon(MechanicEntity actingEntity)
	{
		if (!(actingEntity is BaseUnitEntity baseUnitEntity))
		{
			return false;
		}
		ItemEntityWeapon firstWeapon = baseUnitEntity.GetFirstWeapon();
		ItemEntityWeapon secondWeapon = baseUnitEntity.GetSecondWeapon();
		if (!IsRangedWeapon(firstWeapon))
		{
			return IsRangedWeapon(secondWeapon);
		}
		return true;
		static bool IsRangedWeapon(ItemEntityWeapon weapon)
		{
			return weapon?.Blueprint.IsRanged ?? false;
		}
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		m_ActingUnitHasRangedWeapon.Value = IsActingUnitHasRangedWeapon(EventInvokerExtensions.MechanicEntity);
	}

	void IEquipItemHandler.OnDidEquipped()
	{
		if (EventInvokerExtensions.MechanicEntity is ItemEntityWeapon itemEntityWeapon)
		{
			MechanicEntity currentUnit = TurnController.CurrentUnit;
			if (itemEntityWeapon.Wielder == currentUnit)
			{
				m_ActingUnitHasRangedWeapon.Value = IsActingUnitHasRangedWeapon(currentUnit);
			}
		}
	}

	void IEquipItemHandler.OnWillUnequip()
	{
	}
}
