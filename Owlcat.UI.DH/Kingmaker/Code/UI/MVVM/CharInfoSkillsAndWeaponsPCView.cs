using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoSkillsAndWeaponsPCView : CharInfoSkillsAndWeaponsBaseView
{
	[Header("Buttons")]
	[SerializeField]
	private OwlcatMultiButton m_SkillsButton;

	[SerializeField]
	private OwlcatMultiButton m_WeaponsButton;

	[SerializeField]
	private Transform m_TabSelector;

	private float m_CurrentAngle;

	protected override void OnBind()
	{
		base.OnBind();
		m_SkillsButton.Or(null)?.SetActiveLayer((CurrentSection.Value == CharInfoComponentType.Skills) ? 1 : 0);
		m_WeaponsButton.Or(null)?.SetActiveLayer((CurrentSection.Value == CharInfoComponentType.Weapons) ? 1 : 0);
		m_CurrentAngle = ((CurrentSection.Value == CharInfoComponentType.Skills) ? 0f : 180f);
		if (m_TabSelector != null)
		{
			m_TabSelector.localRotation = Quaternion.Euler(m_TabSelector.localRotation.x, m_CurrentAngle, m_TabSelector.localRotation.z);
		}
		ObservableSubscribeExtensions.Subscribe(m_SkillsButton.OnLeftClickAsObservable(), delegate
		{
			CurrentSection.Value = CharInfoComponentType.Skills;
			m_SkillsButton.SetActiveLayer(1);
			m_WeaponsButton.SetActiveLayer(0);
			m_CurrentAngle = 0f;
			if (m_TabSelector != null)
			{
				m_TabSelector.localRotation = Quaternion.Euler(m_TabSelector.localRotation.x, m_CurrentAngle, m_TabSelector.localRotation.z);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_WeaponsButton.OnLeftClickAsObservable(), delegate
		{
			CurrentSection.Value = CharInfoComponentType.Weapons;
			m_SkillsButton.SetActiveLayer(0);
			m_WeaponsButton.SetActiveLayer(1);
			m_CurrentAngle = 180f;
			if (m_TabSelector != null)
			{
				m_TabSelector.localRotation = Quaternion.Euler(m_TabSelector.localRotation.x, m_CurrentAngle, m_TabSelector.localRotation.z);
			}
		}).AddTo(this);
	}

	protected override void InternalBindSection(CharInfoComponentType section)
	{
		if (section == CharInfoComponentType.Skills)
		{
			m_WeaponsBlockPCView.UnbindSection();
			m_SkillsBlockPCView.BindSection(base.ViewModel.SkillsBlockVM);
		}
		else
		{
			m_SkillsBlockPCView.UnbindSection();
			m_WeaponsBlockPCView.BindSection(base.ViewModel.WeaponsBlockVM);
		}
	}
}
