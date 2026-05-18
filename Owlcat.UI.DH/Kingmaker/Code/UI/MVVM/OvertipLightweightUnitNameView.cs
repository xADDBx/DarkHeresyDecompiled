using System.Collections.Generic;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipLightweightUnitNameView : View<LightweightOvertipNameBlockVM>
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
		base.ViewModel.MechanicEntityUIState.IsVisibleForPlayer.CombineLatest(base.ViewModel.MechanicEntityUIState.IsMouseOverUnit, base.ViewModel.MechanicEntityUIState.HideOvertip, base.ViewModel.MechanicEntityUIState.IsDeadOrUnconsciousIsDead, base.ViewModel.MechanicEntityUIState.HoverSelfTargetAbility, (bool _, bool _, bool _, bool _, bool _) => true).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate
		{
			UpdateVisibility();
		})
			.AddTo(this);
		base.ViewModel.MechanicEntityUIState.Name.Subscribe(SetName).AddTo(this);
		if (m_MultiSelectable != null)
		{
			base.ViewModel.MechanicEntityUIState.IsEnemy.Subscribe(delegate(bool value)
			{
				m_MultiSelectable.SetActiveLayer(value ? "Enemy" : (base.ViewModel.MechanicEntityUIState.IsPlayer.CurrentValue ? "Party" : "Ally"));
			}).AddTo(this);
		}
		m_NameText.SetHint(base.ViewModel.MechanicEntityUIState.Name, null, CharacterNameCanvasRenderer.GetColor()).AddTo(this);
	}

	private void SetName(string value)
	{
		((RectTransform)m_NameText.transform).sizeDelta = new Vector2(1000f, ((RectTransform)m_NameText.transform).sizeDelta.y);
		m_NameText.text = value;
		m_NameText.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
		((RectTransform)m_NameText.transform).sizeDelta = new Vector2(m_NameText.renderedWidth, ((RectTransform)m_NameText.transform).sizeDelta.y);
		foreach (RectTransform containers in m_ContainersList)
		{
			containers.sizeDelta = new Vector2(m_NameText.renderedWidth, containers.sizeDelta.y);
		}
	}

	private void UpdateVisibility()
	{
		m_Animator.PlayAnimation(base.ViewModel.IsVisible());
	}
}
