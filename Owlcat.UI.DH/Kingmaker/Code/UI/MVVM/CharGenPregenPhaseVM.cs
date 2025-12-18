using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenPregenPhaseVM : CharGenPhaseBaseVM, ICharGenPregenHandler, ISubscriber
{
	private readonly ReactiveProperty<bool> m_CurrentPageIsFirst = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CurrentPageIsLast = new ReactiveProperty<bool>();

	private readonly AutoDisposingList<CareerPathVM> m_PregenCareers = new AutoDisposingList<CareerPathVM>();

	private readonly ObservableList<CharGenPregenSelectorItemVM> m_PregenEntitiesList = new ObservableList<CharGenPregenSelectorItemVM>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<CharGenPregenSelectorItemVM> m_SelectedPregenEntity = new ReactiveProperty<CharGenPregenSelectorItemVM>();

	public ReadOnlyReactiveProperty<bool> CurrentPageIsFirst => m_CurrentPageIsFirst;

	public ReadOnlyReactiveProperty<bool> CurrentPageIsLast => m_CurrentPageIsLast;

	public ReadOnlyReactiveProperty<CharGenPregenSelectorItemVM> SelectedPregenEntity => m_SelectedPregenEntity;

	public SelectionGroupRadioVM<CharGenPregenSelectorItemVM> PregenSelectionGroup { get; }

	public ReadOnlyReactiveProperty<bool> IsCustomCharacter => CharGenContext.IsCustomCharacter;

	private bool IsPregen => SelectedPregenEntity.CurrentValue?.ChargenUnit != null;

	public CharGenPregenPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, CharGenPhaseType.Pregen)
	{
		PregenSelectionGroup = new SelectionGroupRadioVM<CharGenPregenSelectorItemVM>(m_PregenEntitiesList, m_SelectedPregenEntity);
		AddDisposable(PregenSelectionGroup);
		AddDisposable(SelectedPregenEntity.Subscribe(SetPregen));
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(SelectedPregenEntity.Subscribe(delegate(CharGenPregenSelectorItemVM value)
		{
			m_CurrentPageIsFirst.Value = m_PregenEntitiesList.FirstOrDefault() == value;
			m_CurrentPageIsLast.Value = m_PregenEntitiesList.LastOrDefault() == value;
		}));
		CreateTooltipSystem();
		if (CharGenContext.CharGenConfig.Mode == CharGenMode.NewGame)
		{
			ConfigRoot.Instance.CharGenRoot.EnsureNewGamePregens(UnitsCallback);
		}
		else
		{
			ConfigRoot.Instance.CharGenRoot.EnsureCompanionPregens(UnitsCallback, CharGenContext.CharGenConfig.CompanionType);
		}
		void UnitsCallback(List<ChargenUnit> units)
		{
			m_PregenEntitiesList.Add(AddDisposableAndReturn(new CharGenPregenSelectorItemVM(null, isCustomCharacter: true)));
			foreach (ChargenUnit unit in units)
			{
				m_PregenEntitiesList.Add(AddDisposableAndReturn(new CharGenPregenSelectorItemVM(unit)));
			}
			if (m_PregenEntitiesList.Count > 1)
			{
				PregenSelectionGroup.TrySelectEntity(m_PregenEntitiesList[1]);
			}
			else
			{
				PregenSelectionGroup.TrySelectFirstValidEntity();
			}
		}
	}

	void ICharGenPregenHandler.HandleSetPregen(BaseUnitEntity unit)
	{
		if (!UtilityNet.IsControlMainCharacter())
		{
			m_SelectedPregenEntity.Value = m_PregenEntitiesList.FirstOrDefault((CharGenPregenSelectorItemVM item) => unit == item.ChargenUnit?.Unit);
		}
		SetUnit(unit);
		UpdatePhaseName();
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		m_PregenCareers.Clear();
		ConfigRoot.Instance.CharGenRoot.DisposeUnitsForChargen();
	}

	protected override bool CheckIsCompleted()
	{
		return true;
	}

	protected override void OnBeginDetailedView()
	{
	}

	private void CreateTooltipSystem()
	{
		AddDisposable(InfoVM = new InfoSectionVM());
		AddDisposable(SecondaryInfoVM = new InfoSectionVM());
		AddDisposable(m_ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate));
	}

	private void SetPregen(CharGenPregenSelectorItemVM pregenVM)
	{
		if (pregenVM == null)
		{
			SetUnit(null);
			return;
		}
		BaseUnitEntity unit = pregenVM.ChargenUnit?.Unit;
		Game.Instance.GameCommandQueue.CharGenSetPregen(unit);
	}

	private void SetUnit([CanBeNull] BaseUnitEntity unit)
	{
		CharGenContext.SetPregenUnit(unit);
		SetShowVisualSettings(unit != null);
		SetupTooltipTemplate();
	}

	private void SetupTooltipTemplate()
	{
		m_ReactiveTooltipTemplate.Value = TooltipTemplate();
	}

	private TooltipBaseTemplate TooltipTemplate()
	{
		if (!IsPregen)
		{
			CharGenConfig charGenConfig = CharGenContext.CharGenConfig;
			return new TooltipTemplateChargenCustomCharacter(charGenConfig.Mode, charGenConfig.CompanionType);
		}
		BaseUnitEntity unit = SelectedPregenEntity.CurrentValue.ChargenUnit.Unit;
		m_PregenCareers.Clear();
		m_PregenCareers.AddRange(from BlueprintCareerPath careerBp in from f in unit.Progression.Features.Visible
				where f.Blueprint is BlueprintCareerPath
				select f.Blueprint
			select new CareerPathVM(unit, careerBp, null));
		return new TooltipTemplateChargenUnitInformation(unit, CharGenContext.LevelUpManager.CurrentValue, m_PregenCareers, expandedView: true);
	}

	public void UpdatePhaseName()
	{
		m_PhaseName.Value = (IsPregen ? UIStrings.Instance.CharGen.Pregen : UIStrings.Instance.CharGen.CustomCharacterPregen);
	}

	public bool GoNextPage()
	{
		return PregenSelectionGroup.SelectNextValidEntity();
	}

	public bool GoPrevPage()
	{
		return PregenSelectionGroup.SelectPrevValidEntity();
	}
}
