using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Kingmaker.UnitLogic.Levelup.Selections.CharacterName;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenSummaryPhaseVM : CharGenPhaseBaseVM, ICharGenSummaryPhaseHandler, ISubscriber
{
	public CharGenNameVM CharGenNameVM;

	public CharInfoSkillsBlockVM CharInfoSkillsBlockVM;

	private readonly ReactiveCommand<Action> m_InterruptHandler = new ReactiveCommand<Action>();

	public CharInfoLevelClassScoresVM LevelClassScoresVM;

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private SelectionStateCharacterName m_SelectionStateCharacterName;

	private bool m_Subscribed;

	private readonly AutoDisposingList<CareerPathVM> m_UnitCareers = new AutoDisposingList<CareerPathVM>();

	public Observable<Action> InterruptHandler => m_InterruptHandler;

	public CharGenSummaryPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, CharGenPhaseType.Summary)
	{
		base.HasPantograph = false;
		base.CanInterruptChargen = true;
		CreateTooltipSystem();
	}

	void ICharGenSummaryPhaseHandler.HandleSetName(string characterName)
	{
		if (!CharGenNameVM.UnitName.CurrentValue.Equals(characterName, StringComparison.Ordinal))
		{
			CharGenNameVM.SetName(characterName);
		}
		SetNameUI(characterName);
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		m_UnitCareers.Clear();
	}

	protected override bool CheckIsCompleted()
	{
		return CharGenContext.LevelUpManager.CurrentValue?.IsAllSelectionsMadeAndValid ?? false;
	}

	protected override void OnBeginDetailedView()
	{
		SetupTooltipTemplate();
		if (m_Subscribed)
		{
			SetDefaultNameIfNeeded();
			return;
		}
		AddDisposable(CharGenNameVM = new CharGenNameVM(CharGenContext.CurrentUnit, CharGenContext.LevelUpManager, GetRandomName, SetName));
		AddDisposable(LevelClassScoresVM = new CharInfoLevelClassScoresVM(CharGenNameVM.PreviewUnit));
		AddDisposable(CharInfoSkillsBlockVM = new CharInfoSkillsBlockVM(CharGenNameVM.PreviewUnit, null));
		AddDisposable(CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
		AddDisposable(CharGenContext.CurrentUnit.Subscribe(delegate(BaseUnitEntity unit)
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
		}));
		AddDisposable(EventBus.Subscribe(this));
		SetDefaultNameIfNeeded();
		m_Subscribed = true;
	}

	private void HandleLevelUpManager(LevelUpManager manager)
	{
		if (manager != null)
		{
			BlueprintCharacterNameSelection selectionByType = UtilityChargen.GetSelectionByType<BlueprintCharacterNameSelection>(manager.Path);
			if (selectionByType != null)
			{
				m_SelectionStateCharacterName = manager.GetSelectionState(manager.Path, selectionByType, 0) as SelectionStateCharacterName;
				UpdateIsCompleted();
			}
		}
	}

	private void SetName(string characterName)
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
		if (CharGenContext.Doll.Race == null)
		{
			return string.Empty;
		}
		return BlueprintCharGenRoot.Instance.PregenCharacterNames.GetRandomName(CharGenContext.Doll.Race.RaceId, CharGenContext.Doll.Gender, CharGenContext.CharGenConfig.Mode, CharGenNameVM.UnitName.CurrentValue);
	}

	private void SetDefaultNameIfNeeded()
	{
		if (CharGenContext.IsCustomCharacter.CurrentValue && CharGenContext.LevelUpManager.CurrentValue.PreviewUnit.GetDescriptionOptional()?.CustomName == null && CharGenContext.Doll?.Race != null)
		{
			string defaultName = BlueprintCharGenRoot.Instance.PregenCharacterNames.GetDefaultName(CharGenContext.Doll.Race.RaceId, CharGenContext.Doll.Gender, CharGenContext.CharGenConfig.Mode, CharGenNameVM.UnitName.CurrentValue);
			SetName(defaultName);
		}
	}

	public override void InterruptChargen(Action onComplete)
	{
		m_InterruptHandler.Execute(onComplete);
	}

	private void CreateTooltipSystem()
	{
		AddDisposable(InfoVM = new InfoSectionVM());
		AddDisposable(SecondaryInfoVM = new InfoSectionVM());
		AddDisposable(m_ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate));
	}

	private void SetupTooltipTemplate()
	{
		m_ReactiveTooltipTemplate.Value = GetTooltipTemplate();
	}

	private TooltipBaseTemplate GetTooltipTemplate()
	{
		LevelUpManager currentValue = CharGenContext.LevelUpManager.CurrentValue;
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
