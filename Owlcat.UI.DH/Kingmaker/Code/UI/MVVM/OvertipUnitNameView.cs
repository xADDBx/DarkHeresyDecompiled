using System.Collections.Generic;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipUnitNameView : View<OvertipNameBlockVM>
{
	[SerializeField]
	private TextMeshProUGUI m_NameText;

	private CanvasRenderer m_CharacterNameCanvasRenderer;

	[SerializeField]
	private OwlcatMultiSelectable m_MultiSelectable;

	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private List<RectTransform> m_ContainersList;

	private CanvasRenderer CharacterNameCanvasRenderer => m_CharacterNameCanvasRenderer ?? (m_CharacterNameCanvasRenderer = m_NameText.GetComponent<CanvasRenderer>());

	protected override void OnBind()
	{
		base.ViewModel.Name.Subscribe(SetName).AddTo(this);
		base.ViewModel.IsVisible.Subscribe(SetVisible).AddTo(this);
		if (m_MultiSelectable != null)
		{
			base.ViewModel.IsEnemy.Subscribe(delegate(bool value)
			{
				m_MultiSelectable.SetActiveLayer(value ? "Enemy" : (base.ViewModel.IsPlayer.CurrentValue ? "Party" : "Ally"));
			}).AddTo(this);
		}
		m_NameText.SetHint(base.ViewModel.Name, null, CharacterNameCanvasRenderer.GetColor()).AddTo(this);
	}

	private void SetName(string value)
	{
		RectTransform rectTransform = (RectTransform)m_NameText.transform;
		rectTransform.sizeDelta = new Vector2(1000f, rectTransform.sizeDelta.y);
		m_NameText.text = value;
		m_NameText.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
		if (string.IsNullOrEmpty(value))
		{
			return;
		}
		rectTransform.sizeDelta = new Vector2(m_NameText.renderedWidth, rectTransform.sizeDelta.y);
		foreach (RectTransform containers in m_ContainersList)
		{
			containers.sizeDelta = new Vector2(m_NameText.renderedWidth, containers.sizeDelta.y);
		}
	}

	private void SetVisible(bool isVisible)
	{
		m_Animator.PlayAnimation(isVisible);
	}
}
