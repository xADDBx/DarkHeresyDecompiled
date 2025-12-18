using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InteractionSlotPartView : View<InteractionSlotPartVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	protected LootSlotView m_SlotView;

	public void Initialize()
	{
		Hide();
	}

	protected override void OnBind()
	{
		Show();
		SetLabels();
		BindSlot();
	}

	private void SetLabels()
	{
		m_Title.text = base.ViewModel.Name;
	}

	private void BindSlot()
	{
		base.ViewModel.ItemSlot.Subscribe(m_SlotView.Bind).AddTo(this);
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnUnbind()
	{
		Hide();
	}
}
