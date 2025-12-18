using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class LootCollectorVM : ViewModel
{
	public readonly string LootDisplayName;

	public readonly bool HasSkillCheck;

	private readonly LootVM m_Loot;

	private readonly ReactiveProperty<bool> m_ExtendedView = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_NoLoot = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsTrashMode = new ReactiveProperty<bool>();

	public string SkillCheckText { get; }

	public LootVM Loot => m_Loot;

	private LootWindowMode Mode => m_Loot.Mode;

	public ObservableList<LootObjectVM> ContextLoot => m_Loot?.ContextLoot;

	public ReadOnlyReactiveProperty<bool> ExtendedView => m_ExtendedView;

	public ReadOnlyReactiveProperty<bool> NoLoot => m_NoLoot;

	public ReadOnlyReactiveProperty<bool> IsTrashMode => m_IsTrashMode;

	public LootCollectorVM(LootVM lootVM)
	{
		m_Loot = lootVM;
		LootDisplayName = UIStrings.Instance.LootWindow.GetLootNameByContext(Mode);
		HasSkillCheck = lootVM.SkillCheckResult != null;
		SkillCheckText = UtilitySkillcheck.GetLootSkillCheck(lootVM.SkillCheckResult);
	}

	public void CollectAll()
	{
		TryCollect();
	}

	private void TryCollect()
	{
		m_Loot.TryCollectLoot();
		Close();
	}

	public void Close()
	{
		m_Loot.Close();
		TooltipHelper.HideTooltip();
	}

	public void ChangeView()
	{
		m_Loot.ChangeView();
	}

	public void SetTrashMode()
	{
		m_IsTrashMode.Value = !IsTrashMode.CurrentValue;
	}
}
