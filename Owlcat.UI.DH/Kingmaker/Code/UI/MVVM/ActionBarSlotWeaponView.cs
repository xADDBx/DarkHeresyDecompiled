using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class ActionBarSlotWeaponView : View<ItemSlotVM>
{
	[SerializeField]
	private Image m_WeaponIcon;

	[SerializeField]
	private Color m_FakeColor;

	[SerializeField]
	private Color m_NormalColor;

	[SerializeField]
	private Sprite m_UnarmedIcon;

	[SerializeField]
	private Image m_FakeBackground;

	[SerializeField]
	private TextMeshProUGUI m_AmmoCountText;

	private ReactiveProperty<bool> m_FakeSlot = new ReactiveProperty<bool>(value: false);

	private RectTransform m_TooltipCustomPlace;

	protected List<Vector2> m_TooltipPriorityPivots;

	protected RectTransform TooltipPlace
	{
		get
		{
			if (!(m_TooltipCustomPlace != null))
			{
				return base.transform as RectTransform;
			}
			return m_TooltipCustomPlace;
		}
	}

	public void Initialize()
	{
	}

	protected override void OnBind()
	{
		base.ViewModel.Icon.Subscribe(delegate(Sprite icon)
		{
			m_WeaponIcon.sprite = ((icon != null) ? icon : m_UnarmedIcon);
		}).AddTo(this);
		m_FakeSlot.Subscribe(delegate(bool val)
		{
			m_WeaponIcon.color = (val ? m_FakeColor : m_NormalColor);
			m_FakeBackground.Or(null)?.gameObject.SetActive(val || !base.ViewModel.HasItem);
		}).AddTo(this);
	}

	public void SetFakeMode(bool state)
	{
		m_FakeSlot.Value = state;
	}

	public void SetTooltipCustomPosition(RectTransform rectTransform, List<Vector2> pivots = null)
	{
		m_TooltipCustomPlace = rectTransform;
		m_TooltipPriorityPivots = pivots;
	}

	protected override void OnUnbind()
	{
	}
}
