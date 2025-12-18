using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenContextVM : ViewModel, ICharGenInitiateUIHandler, ISubscriber, ICharacterSelectorHandler
{
	private readonly ReactiveProperty<CharGenVM> m_CharGenVM = new ReactiveProperty<CharGenVM>();

	private readonly ReactiveProperty<RespecVM> m_RespecVM = new ReactiveProperty<RespecVM>();

	private Action m_EnterNewGameAction;

	private Action m_CloseWithoutCompleteAction;

	private Action m_CloseSoundAction;

	private Action m_ShowNewGameAction;

	private Action<BaseUnitEntity> m_CompleteAction;

	public ReadOnlyReactiveProperty<CharGenVM> CharGenVM => m_CharGenVM;

	public ReadOnlyReactiveProperty<RespecVM> RespecVM => m_RespecVM;

	public CharGenContextVM(ReactiveProperty<CharGenVM> charGenVM, ReactiveProperty<RespecVM> respecVM)
	{
		m_CharGenVM = charGenVM;
		m_RespecVM = respecVM;
		EventBus.Subscribe(this).AddTo(this);
	}

	public void HandleStartCharGen(CharGenConfig config, bool isCustomCompanionChargen)
	{
		EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
		DisposeChargen();
		m_CloseWithoutCompleteAction = config.OnClose;
		m_CompleteAction = config.OnComplete;
		m_CloseSoundAction = config.OnCloseSoundAction;
		m_ShowNewGameAction = config.OnShowNewGameAction;
		if (config.Mode == CharGenMode.NewGame && !config.Unit.IsCustomCompanion() && !config.Unit.IsPet)
		{
			m_EnterNewGameAction = config.EnterNewGameAction;
			Game.Instance.Player.SetMainCharacter(config.Unit);
		}
		m_CharGenVM.Value = new CharGenVM(config, CloseWithoutComplete, CompleteCharGen, isCustomCompanionChargen).AddTo(this);
	}

	public void HandleSelectCharacter(List<BaseUnitEntity> characters, Action<BaseUnitEntity> successAction)
	{
		if (m_RespecVM.Value == null)
		{
			m_RespecVM.Value = new RespecVM(characters, successAction, DisposeRespec).AddTo(this);
		}
	}

	private void CloseWithoutComplete()
	{
		m_ShowNewGameAction?.Invoke();
		m_CloseWithoutCompleteAction?.Invoke();
		CloseCharGen();
	}

	private void CompleteCharGen(BaseUnitEntity resultUnit)
	{
		bool num = m_CharGenVM.Value.PhasesCollection.Any((CharGenPhaseBaseVM p) => p.PhaseType == CharGenPhaseType.LevelUpModification);
		m_CompleteAction?.Invoke(resultUnit);
		m_EnterNewGameAction?.Invoke();
		CloseCharGen();
		if (num)
		{
			EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
			{
				h.HandleOpenCharacterInfo(CharInfoPageType.Abilities, resultUnit);
			});
		}
	}

	private void CloseCharGen()
	{
		DisposeChargen();
		m_CloseSoundAction?.Invoke();
	}

	private void DisposeChargen()
	{
		m_CharGenVM.Value?.Dispose();
		m_CharGenVM.Value = null;
	}

	private void DisposeRespec()
	{
		m_RespecVM.Value?.Dispose();
		m_RespecVM.Value = null;
	}

	protected override void OnDispose()
	{
		DisposeRespec();
		DisposeChargen();
	}
}
