using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Dialog;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Items;
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

public class LootContext : ViewModel, ILootInteractionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IGameModeHandler
{
	private readonly ReactiveProperty<LootVM> m_LootVM = new ReactiveProperty<LootVM>();

	private bool m_IsInitializing;

	public ReadOnlyReactiveProperty<LootVM> LootVM => m_LootVM;

	public bool IsShown => LootVM.CurrentValue != null;

	public LootContext(ReactiveProperty<LootVM> lootVM)
	{
		m_LootVM = lootVM;
		EventBus.Subscribe(this).AddTo(this);
	}

	public void HandleLootInteraction(EntityViewBase[] objects, LootContainerType containerType, Action closeCallback)
	{
		if (EventInvokerExtensions.BaseUnitEntity.IsMyNetRole())
		{
			if (IsShown)
			{
				DisposeLoot();
			}
			LootWindowMode mode = containerType switch
			{
				LootContainerType.OneSlot => LootWindowMode.OneSlot, 
				LootContainerType.PlayerChest => LootWindowMode.PlayerChest, 
				LootContainerType.Unit => LootWindowMode.ShortUnit, 
				_ => (objects.Length <= 1 && containerType != LootContainerType.Environment && containerType != 0) ? LootWindowMode.StandardChest : LootWindowMode.Short, 
			};
			closeCallback = (Action)Delegate.Combine(closeCallback, new Action(DisposeLoot));
			m_LootVM.Value = new LootVM(mode, objects, closeCallback);
		}
	}

	public void HandleSpaceLootInteraction(ILootable[] objects, LootContainerType containerType, Action closeCallback, SkillCheckResult skillCheckResult = null)
	{
		if (EventInvokerExtensions.BaseUnitEntity.IsMyNetRole())
		{
			if (IsShown)
			{
				DisposeLoot();
			}
			if (containerType == LootContainerType.StarSystemObject)
			{
				LootWindowMode mode = LootWindowMode.ShortUnit;
				closeCallback = (Action)Delegate.Combine(closeCallback, new Action(DisposeLoot));
				m_LootVM.Value = new LootVM(mode, objects, containerType, closeCallback, skillCheckResult);
			}
		}
	}

	public void HandleZoneLootInteraction(AreaTransitionPart areaTransition)
	{
		if (!EventInvokerExtensions.BaseUnitEntity.IsMyNetRole())
		{
			return;
		}
		if (IsShown)
		{
			if (LootVM.CurrentValue.Mode == LootWindowMode.ZoneExit)
			{
				return;
			}
			DisposeLoot();
		}
		BaseUnitEntity invokerEntity = EventInvokerExtensions.BaseUnitEntity;
		m_LootVM.Value = new LootVM(LootWindowMode.ZoneExit, MassLootHelper.GetMassLootFromCurrentArea(), delegate
		{
			bool isPlayerCommand = !ContextData<GameCommandContext>.Current && !ContextData<UnitCommandContext>.Current;
			AreaTransitionGroupCommand.ExecuteTransition(areaTransition, isPlayerCommand, invokerEntity);
		}, DisposeLoot);
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (IsShown && (gameMode == GameModeType.Cutscene || gameMode == GameModeType.GameOver || gameMode == GameModeType.Dialog))
		{
			DisposeLoot();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	private void DisposeLoot()
	{
		LootVM.CurrentValue?.Dispose();
		m_LootVM.Value = null;
	}
}
