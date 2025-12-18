using System;
using DG.Tweening;
using Kingmaker.EntitySystem;
using Kingmaker.UI.InputSystems;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class PartyCharacterPCView : PartyCharacterBaseView, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler
{
	[Header("Parts Block")]
	[SerializeField]
	private UnitHealthPartProgressView m_HealthProgressView;

	[SerializeField]
	private UnitHealthPartTextPCView m_HealthTextView;

	[SerializeField]
	private BuffsBlockView m_BuffView;

	[SerializeField]
	private UnitBarkPartView m_BarkBlockView;

	[Header("Buttons Part")]
	[SerializeField]
	private OwlcatMultiButton m_LevelUpButton;

	[Header("Encumbrance Part")]
	[SerializeField]
	private GameObject m_EncumbranceIndicator;

	[SerializeField]
	private Color m_EncumbranceHeavyLoad = Color.yellow;

	[SerializeField]
	private Color m_EncumbranceOverload = Color.red;

	[SerializeField]
	private Image m_PersonalEncumbranceIcon;

	[SerializeField]
	private GameObject m_PersonalEncumbranceObject;

	private Vector2 m_OriginalLocalPointerPosition;

	private Vector3 m_OriginalPanelLocalPosition;

	private bool m_IsDragging;

	private Action<PartyCharacterBaseView> m_SwitchAction;

	protected override void Awake()
	{
		base.Awake();
		m_HealthTextView.gameObject.SetActive(value: false);
	}

	public void SetSwitchAction(Action<PartyCharacterBaseView> switchAction)
	{
		m_SwitchAction = switchAction;
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_HealthTextView.Bind(base.ViewModel.HealthPartVM);
		if ((bool)m_HealthProgressView)
		{
			m_HealthProgressView.Bind(base.ViewModel.HealthPartVM);
		}
		if ((bool)m_BuffView)
		{
			m_BuffView.Bind(base.ViewModel.BuffBlockVM);
		}
		if ((bool)m_BarkBlockView)
		{
			m_BarkBlockView.Bind(base.ViewModel.BarkPartVM);
		}
		base.ViewModel.CharacterEncumbrance.CombineLatest(base.ViewModel.PartyEncumbrance, (Encumbrance _, Encumbrance _) => true).Subscribe(delegate
		{
			UpdateEncumbrance();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_LevelUpButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.LevelUp();
		}).AddTo(this);
		base.ViewModel.IsLevelUp.CombineLatest(base.ViewModel.IsLevelUpCurrent, base.ViewModel.IsLevelUpInProgress, (bool _, bool _, bool _) => true).Subscribe(delegate
		{
			UpdateLevelUp();
		}).AddTo(this);
		KeyboardAccess keyboard = Game.Instance.Keyboard;
		int index = base.ViewModel.Index;
		keyboard.Bind("SelectCharacter" + index, delegate
		{
			base.ViewModel.HandleUnitClick();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		if ((bool)m_HealthProgressView)
		{
			m_HealthProgressView.Unbind();
		}
		if ((bool)m_BuffView)
		{
			m_BuffView.Unbind();
		}
		if ((bool)m_BarkBlockView)
		{
			m_BarkBlockView.Unbind();
		}
	}

	protected override void OnHover(bool hover)
	{
		base.OnHover(hover);
		m_HealthTextView.gameObject.SetActive(hover);
	}

	protected override void OnSingleLeftClick()
	{
		if (!m_IsDragging)
		{
			base.OnSingleLeftClick();
			base.ViewModel.HandleUnitClick(isDoubleClick: true);
		}
	}

	protected override void OnDoubleLeftClick()
	{
		if (!m_IsDragging)
		{
			base.OnDoubleLeftClick();
			base.ViewModel.SelectAll();
		}
	}

	public void OnBeginDrag(PointerEventData data)
	{
		UpdateBasePosition();
		if (base.ViewModel.CanSwitch)
		{
			m_IsDragging = true;
			m_OriginalPanelLocalPosition = m_RectTransform.localPosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ParentTransform, data.position, data.pressEventCamera, out m_OriginalLocalPointerPosition);
			m_RectTransform.transform.SetAsLastSibling();
		}
	}

	public void OnEndDrag(PointerEventData data)
	{
		m_IsDragging = false;
		m_RectTransform.DOLocalMoveX(BasePositionX, 0.2f).SetUpdate(isIndependentUpdate: true);
	}

	public void OnDrag(PointerEventData data)
	{
		if (m_IsDragging && base.ViewModel.CanSwitch)
		{
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ParentTransform, data.position, data.pressEventCamera, out var localPoint))
			{
				SetTargetAnchoredPosition(localPoint);
			}
			ClampToWindow();
			m_SwitchAction?.Invoke(this);
		}
	}

	private void SetTargetAnchoredPosition(Vector2 localPointerPosition)
	{
		Vector3 vector = localPointerPosition - m_OriginalLocalPointerPosition;
		vector.y = 0f;
		m_RectTransform.localPosition = m_OriginalPanelLocalPosition + vector;
	}

	private void ClampToWindow()
	{
		Vector3 localPosition = m_RectTransform.localPosition;
		Vector3 vector = m_ParentTransform.rect.min - m_RectTransform.rect.min;
		Vector3 vector2 = m_ParentTransform.rect.max - m_RectTransform.rect.max;
		localPosition.x = Mathf.Clamp(m_RectTransform.localPosition.x, vector.x, vector2.x);
		localPosition.y = Mathf.Clamp(m_RectTransform.localPosition.y, vector.y, vector2.y);
		m_RectTransform.localPosition = localPosition;
	}

	private void UpdateEncumbrance()
	{
		bool flag = base.ViewModel.CharacterEncumbrance.CurrentValue == Encumbrance.Overload;
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		bool flag2 = (loadedAreaState == null || !loadedAreaState.Settings.CapitalPartyMode) && base.ViewModel.PartyEncumbrance.CurrentValue == Encumbrance.Overload;
		if (!flag2 && (bool)m_PersonalEncumbranceIcon)
		{
			if (base.ViewModel.CharacterEncumbrance.CurrentValue == Encumbrance.Heavy)
			{
				m_PersonalEncumbranceIcon.color = m_EncumbranceHeavyLoad;
			}
			if (base.ViewModel.CharacterEncumbrance.CurrentValue == Encumbrance.Overload)
			{
				m_PersonalEncumbranceIcon.color = m_EncumbranceOverload;
			}
		}
		m_PersonalEncumbranceObject.Or(null)?.SetActive(!flag2);
		m_EncumbranceIndicator.Or(null)?.SetActive(flag2 || flag);
	}

	private void UpdateLevelUp()
	{
		m_LevelUpButton.gameObject.SetActive(base.ViewModel.IsLevelUp.CurrentValue && !RootUIContext.Instance.IsChargenShown);
	}
}
