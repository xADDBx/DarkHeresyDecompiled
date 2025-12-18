using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ChestStashView : VirtualListElementViewBase<LootObjectVM>, IConsoleEntity
{
	[SerializeField]
	private ItemSlotsGroupView m_SlotsGroup;

	[SerializeField]
	private PersonalChestSlotView m_SlotViewPrefab;

	private bool m_Init;

	public ItemSlotsGroupView SlotsGroup => m_SlotsGroup;

	public void Initialize()
	{
		if (!m_Init)
		{
			m_SlotsGroup.Initialize(m_SlotViewPrefab);
			m_Init = true;
		}
	}

	protected override void BindViewImplementation()
	{
		Initialize();
		SetupSlots();
	}

	private void SetupSlots()
	{
		m_SlotsGroup.Bind(base.ViewModel.SlotsGroup);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
