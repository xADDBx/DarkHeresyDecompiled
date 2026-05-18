using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class PartyCharacterBaseView : View<PartyCharacterVM>, IScrollHandler, IEventSystemHandler
{
	[Header("Base View")]
	[SerializeField]
	private UnitPortraitPartPCView m_PortraitView;

	[SerializeField]
	private OwlcatMultiButton m_CharacterButton;

	[SerializeField]
	private Image m_NetLock;

	private bool? m_IsSelected;

	protected RectTransform m_RectTransform;

	protected RectTransform m_ParentTransform;

	public float BasePositionX;

	public RectTransform RectTransform => m_RectTransform;

	public bool HasUnit => base.ViewModel?.IsEnable.CurrentValue ?? false;

	public BaseUnitEntity UnitEntityData => base.ViewModel?.UnitEntityData;

	protected virtual void Awake()
	{
		base.gameObject.SetActive(value: false);
		m_RectTransform = base.transform as RectTransform;
		m_ParentTransform = base.transform.parent as RectTransform;
	}

	protected override void OnBind()
	{
		m_PortraitView.Bind(base.ViewModel.PortraitPartVM);
		m_CharacterButton.OnHoverAsObservable().Subscribe(OnHover).AddTo(this);
		m_CharacterButton.OnSingleLeftClickAsObservable().Subscribe(OnSingleLeftClick).AddTo(this);
		m_CharacterButton.OnLeftDoubleClickAsObservable().Subscribe(OnDoubleLeftClick).AddTo(this);
		base.ViewModel.IsEnable.Subscribe(base.gameObject.SetActive).AddTo(this);
		base.ViewModel.IsSelected.Skip(1).Subscribe(SetSelected).AddTo(this);
		m_IsSelected = base.ViewModel.IsSelected.CurrentValue;
		m_CharacterButton.SetActiveLayer(base.ViewModel.IsSelected.CurrentValue ? 1 : 0);
		if (m_NetLock != null)
		{
			base.ViewModel.NetAvatar.Subscribe(delegate(Sprite value)
			{
				m_NetLock.gameObject.SetActive(value != null);
				m_NetLock.sprite = value;
			}).AddTo(this);
		}
	}

	protected virtual void OnHover(bool hover)
	{
		base.ViewModel.OnCharacterHover(hover);
		SetMouseHighlighted(hover);
	}

	protected virtual void OnSingleLeftClick()
	{
	}

	protected virtual void OnDoubleLeftClick()
	{
		ServiceWindowsSounds.Instance.Character.SelectAll.Play();
	}

	private void SetSelected(bool value)
	{
		if (value != m_IsSelected)
		{
			m_IsSelected = value;
			m_CharacterButton.SetActiveLayer(value ? 1 : 0);
			if (value)
			{
				ServiceWindowsSounds.Instance.Character.Select.Play();
			}
		}
	}

	public void SetMouseHighlighted(bool value)
	{
		base.ViewModel.UnitEntityData.View.MouseHoverHighlighting = value;
	}

	public void OnScroll(PointerEventData eventData)
	{
		float y = eventData.scrollDelta.y;
		if (!(Mathf.Abs(y) < Mathf.Epsilon))
		{
			ButtonsSounds.Instance.Default.Click.Play();
			base.ViewModel.NextPrev(y < 0f);
		}
	}

	public void UpdateBasePosition()
	{
		BasePositionX = m_RectTransform.localPosition.x;
	}
}
