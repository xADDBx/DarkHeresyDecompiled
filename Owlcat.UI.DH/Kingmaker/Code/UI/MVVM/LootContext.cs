using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class LootContext : ViewModel, ILootInteractionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	private readonly ReactiveProperty<LootVM> m_LootVM;

	private bool m_IsInitializing;

	public ReadOnlyReactiveProperty<LootVM> LootVM => m_LootVM;

	public bool IsShown => LootVM.CurrentValue != null;

	public LootContext(ReactiveProperty<LootVM> lootVM)
	{
		m_LootVM = lootVM;
		GameUIState.Instance.GameMode.Subscribe(OnGameModeStart).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	public void HandleLootInteraction(EntityViewBase[] objects, LootContainerType containerType, Action closeCallback)
	{
		if (CanHandleLoot())
		{
			DisposeLoot();
			LootWindowMode mode = containerType switch
			{
				LootContainerType.OneSlot => LootWindowMode.OneSlot, 
				LootContainerType.PlayerChest => LootWindowMode.PlayerChest, 
				LootContainerType.Unit => LootWindowMode.ShortUnit, 
				_ => GetLootMode(containerType, objects), 
			};
			closeCallback = (Action)Delegate.Combine(closeCallback, new Action(DisposeLoot));
			m_LootVM.Value = new LootVM(mode, objects, closeCallback);
		}
	}

	public void HandleZoneLootInteraction(AreaTransitionPart areaTransition)
	{
		if (CanHandleLoot() && (!IsShown || LootVM.CurrentValue.Mode != LootWindowMode.ZoneExit))
		{
			DisposeLoot();
			BaseUnitEntity invokerEntity = EventInvokerExtensions.BaseUnitEntity;
			m_LootVM.Value = new LootVM(LootWindowMode.ZoneExit, MassLootHelper.GetMassLootFromCurrentArea(), delegate
			{
				bool isPlayerCommand = !ContextData<GameCommandContext>.Current && !ContextData<UnitCommandContext>.Current;
				AreaTransitionGroupCommand.ExecuteTransition(areaTransition, isPlayerCommand, invokerEntity);
			}, DisposeLoot);
		}
	}

	private static LootWindowMode GetLootMode(LootContainerType containerType, EntityViewBase[] objects)
	{
		if (objects.Length <= 1 && containerType != LootContainerType.Environment && containerType != 0)
		{
			return LootWindowMode.StandardChest;
		}
		return LootWindowMode.Short;
	}

	private bool CanHandleLoot()
	{
		if (!EventInvokerExtensions.BaseUnitEntity.IsMyNetRole())
		{
			return false;
		}
		return true;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.GameOver || gameMode == GameModeType.Dialog)
		{
			DisposeLoot();
		}
	}

	private void DisposeLoot()
	{
		LootVM.CurrentValue?.Dispose();
		m_LootVM.Value = null;
	}
}
