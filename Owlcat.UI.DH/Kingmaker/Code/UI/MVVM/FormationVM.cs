using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Formations;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class FormationVM : ViewModel, IFormationUIHandlers, ISubscriber, IPreparationTurnBeginHandler
{
	public readonly SelectionGroupRadioVM<FormationSelectionItemVM> FormationSelector;

	private readonly List<FormationSelectionItemVM> m_FormationSelectionItemViewModels = new List<FormationSelectionItemVM>();

	private readonly ReactiveProperty<int> m_SelectedFormationPresetIndex = new ReactiveProperty<int>();

	public readonly List<FormationCharacterVM> Characters = new List<FormationCharacterVM>();

	private readonly ReactiveProperty<bool> m_IsPreserveFormation = new ReactiveProperty<bool>();

	private readonly ReactiveCommand<Unit> m_FormationUpdated = new ReactiveCommand<Unit>();

	private Action m_Close;

	public static FormationVM Instance;

	public ReadOnlyReactiveProperty<int> SelectedFormationPresetIndex => m_SelectedFormationPresetIndex;

	public bool IsCustomFormation => FormationManager.IsCustomFormation;

	private PartyFormationManager FormationManager => Game.Instance.Player.FormationManager;

	public FormationVM(Action close)
	{
		m_Close = close;
		for (int i = 0; i < ConfigRoot.Instance.Formations.PredefinedFormations.Length; i++)
		{
			m_FormationSelectionItemViewModels.Add(new FormationSelectionItemVM(i));
		}
		EventBus.Subscribe(this).AddTo(this);
		Instance = this;
		ReactiveProperty<FormationSelectionItemVM> reactiveProperty = new ReactiveProperty<FormationSelectionItemVM>(m_FormationSelectionItemViewModels[FormationManager.CurrentFormationIndex]);
		FormationSelector = new SelectionGroupRadioVM<FormationSelectionItemVM>(m_FormationSelectionItemViewModels, reactiveProperty).AddTo(this);
		reactiveProperty.Subscribe(OnFormationSelectedEntityChange).AddTo(this);
		List<BaseUnitEntity> list = Game.Instance.Player.PartyAndPets.Where((BaseUnitEntity u) => u.IsDirectlyControllable).ToList();
		int num = 0;
		foreach (BaseUnitEntity item in list)
		{
			Characters.Add(new FormationCharacterVM(num++, item, m_FormationUpdated));
		}
		EventBus.RaiseEvent(delegate(IUIEventHandler h)
		{
			h.HandleUIEvent(UIEventType.FormationWindowOpen);
		});
	}

	protected override void OnDispose()
	{
		m_FormationSelectionItemViewModels.ForEach(delegate(FormationSelectionItemVM s)
		{
			s.Dispose();
		});
		m_FormationSelectionItemViewModels.Clear();
		Characters.ForEach(delegate(FormationCharacterVM c)
		{
			c.Dispose();
		});
		Characters.Clear();
		m_Close = null;
		EventBus.RaiseEvent(delegate(IUIEventHandler h)
		{
			h.HandleUIEvent(UIEventType.FormationWindowClose);
		});
		Instance = null;
	}

	private void OnFormationSelectedEntityChange(FormationSelectionItemVM itemVM)
	{
		Game.Instance.GameCommandQueue.PartyFormationIndex(itemVM.FormationIndex);
	}

	public void CurrentFormationChanged(int currentFormationIndex)
	{
		m_SelectedFormationPresetIndex.Value = currentFormationIndex;
		m_IsPreserveFormation.Value = FormationManager.GetPreserveFormation();
		m_FormationUpdated?.Execute();
	}

	public void Close()
	{
		m_Close?.Invoke();
	}

	public void SwitchPreserveFormation()
	{
		FormationManager.SetPreserveFormation(!FormationManager.GetPreserveFormation());
		m_IsPreserveFormation.Value = FormationManager.GetPreserveFormation();
	}

	public void ResetCurrentFormation()
	{
		Game.Instance.GameCommandQueue.PartyFormationResetGameCommand();
	}

	public void HandleBeginPreparationTurn(bool canDeploy)
	{
		Close();
	}
}
