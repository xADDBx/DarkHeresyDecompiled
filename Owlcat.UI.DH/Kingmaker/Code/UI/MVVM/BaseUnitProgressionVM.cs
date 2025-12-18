using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class BaseUnitProgressionVM : CharInfoComponentVM
{
	protected readonly ReactiveProperty<CareerPathVM> m_CurrentCareer = new ReactiveProperty<CareerPathVM>();

	protected readonly ReactiveProperty<IRankEntrySelectItem> m_CurrentRankEntryItem = new ReactiveProperty<IRankEntrySelectItem>();

	protected readonly ReactiveProperty<IRankEntrySelectItem> m_FirstAvailableEntryItem = new ReactiveProperty<IRankEntrySelectItem>();

	protected readonly ReactiveCommand<IRankEntrySelectItem> m_OnRepeatedCurrentRankEntryItem = new ReactiveCommand<IRankEntrySelectItem>();

	protected readonly ReactiveProperty<LevelUpManager> m_LevelUpManager;

	public ReadOnlyReactiveProperty<CareerPathVM> CurrentCareer => m_CurrentCareer;

	public ReadOnlyReactiveProperty<IRankEntrySelectItem> CurrentRankEntryItem => m_CurrentRankEntryItem;

	public ReadOnlyReactiveProperty<IRankEntrySelectItem> FirstAvailableEntryItem => m_FirstAvailableEntryItem;

	public Observable<IRankEntrySelectItem> OnRepeatedCurrentRankEntryItem => m_OnRepeatedCurrentRankEntryItem;

	public LevelUpManager LevelUpManager => m_LevelUpManager?.Value;

	protected BaseUnitProgressionVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReactiveProperty<LevelUpManager> levelUpManager)
		: base(unit)
	{
		m_LevelUpManager = levelUpManager;
	}

	public abstract void SetCareerPath(CareerPathVM careerPathVM, bool force = false);

	public abstract void SetRankEntry(IRankEntrySelectItem rankEntryItem);

	public abstract void Commit();
}
