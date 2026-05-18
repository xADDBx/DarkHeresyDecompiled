using Kingmaker.UI.Sound;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class LootCollectorVM : ViewModel
{
	private readonly LootVM m_Loot;

	private readonly ReactiveProperty<bool> m_ExtendedView = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_NoLoot = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsTrashMode = new ReactiveProperty<bool>();

	public LootVM Loot => m_Loot;

	public ObservableList<LootObjectVM> ContextLoot => m_Loot?.ContextLoot;

	public ReadOnlyReactiveProperty<bool> ExtendedView => m_ExtendedView;

	public ReadOnlyReactiveProperty<bool> NoLoot => m_NoLoot;

	public ReadOnlyReactiveProperty<bool> IsTrashMode => m_IsTrashMode;

	public LootCollectorVM(LootVM lootVM)
	{
		m_Loot = lootVM;
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

	public void ToggleTrashMode()
	{
		m_IsTrashMode.Value = !IsTrashMode.CurrentValue;
		ModalWindowsSounds.UISoundLoot loot = ModalWindowsSounds.Instance.Loot;
		(m_IsTrashMode.Value ? loot.ActivateTrashMode : loot.DeactivateTrashMode).Play();
	}
}
