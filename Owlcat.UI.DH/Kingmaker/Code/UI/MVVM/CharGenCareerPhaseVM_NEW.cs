using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Progression.Paths;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenCareerPhaseVM_NEW : CharGenPhaseBaseVM
{
	private readonly ObservableList<CharGenCareerSelectionItemVM> Items = new ObservableList<CharGenCareerSelectionItemVM>();

	private readonly ReactiveProperty<CharGenCareerSelectionItemVM> m_SelectedItem = new ReactiveProperty<CharGenCareerSelectionItemVM>();

	public readonly SelectionGroupRadioVM<CharGenCareerSelectionItemVM> SelectionGroup;

	private readonly BaseUnitEntity Unit;

	public ReadOnlyReactiveProperty<CharGenCareerSelectionItemVM> SelectedItem => m_SelectedItem;

	public CharGenCareerPhaseVM_NEW(CharGenContext charGenContext)
		: base(charGenContext, CharGenPhaseType.Career)
	{
		Unit = charGenContext.LevelUpManager.CurrentValue.PreviewUnit;
		SelectionGroup = new SelectionGroupRadioVM<CharGenCareerSelectionItemVM>(Items, m_SelectedItem);
		AddDisposable(SelectionGroup);
		AddDisposable(SelectedItem.Subscribe(HandleSelectedItem));
		CreateItemList();
	}

	protected override bool CheckIsCompleted()
	{
		return SelectedItem.CurrentValue != null;
	}

	protected override void OnBeginDetailedView()
	{
	}

	private void CreateItemList()
	{
		foreach (BlueprintCareerPath careerPath in ConfigRoot.Instance.Progression.CareerPaths)
		{
			CharGenCareerSelectionItemVM charGenCareerSelectionItemVM = new CharGenCareerSelectionItemVM(careerPath);
			AddDisposable(charGenCareerSelectionItemVM);
			Items.Add(charGenCareerSelectionItemVM);
		}
	}

	private void HandleSelectedItem(CharGenCareerSelectionItemVM item)
	{
		UpdateIsCompleted();
		if (item != null)
		{
			CharGenContext.LevelUpManager.CurrentValue.SelectCareerPath(item.CareerPath);
			EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
			{
				h.HandleUISelectCareerPath();
			});
		}
	}
}
