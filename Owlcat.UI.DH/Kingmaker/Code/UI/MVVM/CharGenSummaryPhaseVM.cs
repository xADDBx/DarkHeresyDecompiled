using System;
using System.Linq;
using Code.View.UI.MVVM;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Kingmaker.UnitLogic.Levelup.Selections.CharacterName;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenSummaryPhaseVM : CharGenPhaseBaseVM, ICharGenSummaryPhaseHandler, ISubscriber
{
	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly AutoDisposingList<CareerPathVM> m_UnitCareers = new AutoDisposingList<CareerPathVM>();

	private readonly ReactiveCommand<Action> m_InterruptHandler = new ReactiveCommand<Action>();

	private SelectionStateCharacterName m_SelectionStateCharacterName;

	private bool m_Subscribed;

	public CharGenNameVM CharGenNameVM { get; private set; }

	public SummaryBackgroundFeaturesVM BackgroundFeaturesVM { get; private set; }

	public Observable<Action> InterruptHandler => m_InterruptHandler;

	public CharGenSummaryPhaseVM(CharGenContext charGenContext, SelectionStateCharacterName selectionStateName)
		: base(charGenContext, CharGenPhaseType.Summary)
	{
		base.DisplayMode = CharGenDisplayMode.Both;
		base.DollPosition = CharacterDollPosition.Result;
		base.HasSmallPortrait = true;
		m_SelectionStateCharacterName = selectionStateName;
		m_PhaseName.Value = ((BlueprintCharacterNameSelection)selectionStateName.Blueprint).Title;
		CreateTooltipSystem();
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		m_UnitCareers.Clear();
	}

	void ICharGenSummaryPhaseHandler.HandleSetName(string characterName)
	{
		if (!CharGenNameVM.CurrentDisplayName.CurrentValue.Equals(characterName, StringComparison.Ordinal))
		{
			SetName(characterName, force: false);
		}
		SetNameUI(characterName);
	}

	protected override bool CheckIsCompleted()
	{
		return m_CharGenContext.LevelUpManager.CurrentValue?.IsAllSelectionsMadeAndValid ?? false;
	}

	protected override void OnBeginDetailedView()
	{
		if (m_Subscribed)
		{
			BackgroundFeaturesVM.UpdateFeatures();
			return;
		}
		CharGenNameVM = new CharGenNameVM(m_CharGenContext.CurrentUnit, m_CharGenContext.LevelUpManager, GetRandomName, SetName).AddTo(this);
		BackgroundFeaturesVM = new SummaryBackgroundFeaturesVM(m_CharGenContext.CurrentUnit, m_CharGenContext.LevelUpManager).AddTo(this);
		CharGenNameVM.CurrentDisplayName.Subscribe(delegate
		{
			UpdateHint();
		}).AddTo(this);
		m_CharGenContext.CurrentUnit.Subscribe(delegate(BaseUnitEntity unit)
		{
			PregenUnitComponent component = unit.Blueprint.GetComponent<PregenUnitComponent>();
			if (component != null)
			{
				SetName(component.PregenName, force: true);
			}
			else
			{
				SetNameUI(string.Empty);
			}
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		m_Subscribed = true;
	}

	public void SetName(string characterName)
	{
		SetName(characterName, force: false);
	}

	private void SetName(string characterName, bool force)
	{
		Game.Instance.GameCommandQueue.CharGenSetName(characterName, force);
	}

	private void SetNameUI(string characterName)
	{
		m_SelectionStateCharacterName?.SelectName(characterName);
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUISelectionChanged();
		});
		UpdateIsCompleted();
	}

	private string GetRandomName()
	{
		Gender gender = ((UnityEngine.Random.Range(0, 2) != 0) ? Gender.Female : Gender.Male);
		return BlueprintCharGenRoot.Instance.PregenCharacterNames.GetRandomName(Race.Human, gender, m_CharGenContext.CharGenConfig.Mode, CharGenNameVM.CurrentDisplayName.CurrentValue);
	}

	private void UpdateHint()
	{
		if (!string.IsNullOrEmpty(CharGenNameVM.CurrentDisplayName.CurrentValue))
		{
			SetPhaseHint(string.Empty);
		}
		else
		{
			SetPhaseHint(UIStrings.Instance.CharGen.ChooseName.Text);
		}
	}

	public override void InterruptChargen(Action onComplete)
	{
		m_InterruptHandler.Execute(onComplete);
	}

	private void CreateTooltipSystem()
	{
	}

	private void SetupTooltipTemplate()
	{
		m_ReactiveTooltipTemplate.Value = GetTooltipTemplate();
	}

	private TooltipBaseTemplate GetTooltipTemplate()
	{
		LevelUpManager currentValue = m_CharGenContext.LevelUpManager.CurrentValue;
		m_UnitCareers.Clear();
		if (currentValue?.PreviewUnit != null)
		{
			BaseUnitEntity unit = currentValue.PreviewUnit;
			m_UnitCareers.AddRange(from BlueprintCareerPath careerBp in from f in unit.Progression.Features.Visible
					where f.Blueprint is BlueprintCareerPath
					select f.Blueprint
				select new CareerPathVM(unit, careerBp, null));
			return new TooltipTemplateChargenUnitInformation(unit, currentValue, m_UnitCareers);
		}
		return null;
	}
}
