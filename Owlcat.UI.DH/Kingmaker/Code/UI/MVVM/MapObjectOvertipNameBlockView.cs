using System.Collections.Generic;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class MapObjectOvertipNameBlockView : View<OvertipMapObjectVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_NameText;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private List<RectTransform> m_ContainersList;

	protected override void OnBind()
	{
		base.ViewModel.Name.Subscribe(SetName).AddTo(this);
		base.ViewModel.MapObjectIsHighlighted.CombineLatest(base.ViewModel.ForceHotKeyPressed, base.ViewModel.IsMouseOverUI, (bool hover, bool force, bool mouseOver) => hover || force || mouseOver).Subscribe(delegate(bool value)
		{
			SetVisible(value && !string.IsNullOrEmpty(base.ViewModel.Name.CurrentValue));
		}).AddTo(this);
	}

	private void SetName(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return;
		}
		float x = UtilityBark.CalculateBarkWidth(value, m_NameText.fontSize);
		m_NameText.text = value;
		RectTransform rectTransform = (RectTransform)m_NameText.transform;
		rectTransform.sizeDelta = new Vector2(x, rectTransform.sizeDelta.y);
		foreach (RectTransform containers in m_ContainersList)
		{
			containers.sizeDelta = new Vector2(x, containers.sizeDelta.y);
		}
	}

	private void SetVisible(bool state)
	{
		m_FadeAnimator.PlayAnimation(state);
	}
}
