using Kingmaker.Code.UI.MVVM;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.ServiceWindows.AbilityModification;

public class ModificationSlotBaseView : View<ModificationSlotVM>, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IDropHandler
{
	private const string NORMAL = "Normal";

	private const string SELECTED = "Selected";

	private const string EQUIPPED = "Equipped";

	private const string SUITED = "Suited";

	[SerializeField]
	protected TextMeshProUGUI m_ModificationName;

	[SerializeField]
	protected TextMeshProUGUI m_Tag;

	[SerializeField]
	protected Image m_ModificationIcon;

	[SerializeField]
	protected OwlcatMultiButton m_ModificationButton;

	protected override void OnBind()
	{
		m_ModificationName.text = base.ViewModel.ModifierName;
		m_Tag.text = base.ViewModel.Tag;
		m_ModificationIcon.sprite = base.ViewModel.ModifierIcon;
		ObservableSubscribeExtensions.Subscribe(m_ModificationButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClick();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ModificationButton.OnLeftDoubleClickAsObservable(), delegate
		{
			base.ViewModel.OnDoubleClick();
		}).AddTo(this);
		m_ModificationButton.OnHoverAsObservable().Subscribe(base.ViewModel.OnHover).AddTo(this);
		base.ViewModel.IsSelected.Subscribe(delegate
		{
			UpdateSlotVisual();
		}).AddTo(this);
		base.ViewModel.IsEquipped.Subscribe(delegate
		{
			UpdateSlotVisual();
		}).AddTo(this);
		base.ViewModel.IsSuitedAbilityHover.Subscribe(delegate
		{
			UpdateSlotVisual();
		}).AddTo(this);
		base.ViewModel.IsSuitable.Subscribe(base.gameObject.SetActive).AddTo(this);
		m_ModificationButton.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		base.ViewModel.OnDrag(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		base.ViewModel.OnDrag(eventData);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		base.ViewModel.OnDrag(eventData, isDragEnd: true);
	}

	public void OnDrop(PointerEventData eventData)
	{
		base.ViewModel.OnDrag(eventData);
	}

	private void UpdateSlotVisual()
	{
		m_ModificationButton.SetActiveLayer(base.ViewModel.IsSuitedAbilityHover.CurrentValue ? "Suited" : (base.ViewModel.IsSelected.CurrentValue ? "Selected" : (base.ViewModel.IsEquipped.CurrentValue ? "Equipped" : "Normal")));
	}
}
