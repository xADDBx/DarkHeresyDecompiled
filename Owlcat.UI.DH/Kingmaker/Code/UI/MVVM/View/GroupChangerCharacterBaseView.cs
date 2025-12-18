using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class GroupChangerCharacterBaseView : View<GroupChangerCharacterVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	protected OwlcatMultiButton m_Button;

	[SerializeField]
	private Image m_Lock;

	[SerializeField]
	private GameObject m_LevelUp;

	[SerializeField]
	protected OwlcatMultiSelectable m_Selectable;

	[SerializeField]
	private TextMeshProUGUI m_CharacterName;

	[SerializeField]
	private TextMeshProUGUI m_CharacterLevel;

	[Header("Parts")]
	[SerializeField]
	private UnitPortraitPartPCView m_PortraitPartView;

	protected override void OnBind()
	{
		base.ViewModel.IsInParty.CombineLatest(base.ViewModel.IsLock, (bool isInParty, bool isLock) => new { isInParty, isLock }).Subscribe(value =>
		{
			SetState(value.isInParty, value.isLock);
		}).AddTo(this);
		m_Lock.SetHint(UIStrings.Instance.GroupChangerTexts.MustBeInPartyHint.Text).AddTo(this);
		m_PortraitPartView.Bind(base.ViewModel.PortraitPartVm);
		m_LevelUp.SetActive(base.ViewModel.IsLevelUp);
		m_CharacterName.text = base.ViewModel.CharacterName;
		TextMeshProUGUI characterLevel = m_CharacterLevel;
		int characterLevel2 = base.ViewModel.CharacterLevel;
		characterLevel.text = characterLevel2.ToString();
	}

	public UnitReference GetUnitReference()
	{
		return base.ViewModel.UnitRef;
	}

	public IConsoleNavigationEntity GetNavigationEntity()
	{
		return this;
	}

	public void SetFocus(bool value)
	{
		base.ViewModel.SetFocused(value);
		if (m_Selectable != null)
		{
			m_Selectable.SetFocus(value);
		}
	}

	public bool IsValid()
	{
		return true;
	}

	protected virtual void SetState(bool isInParty, bool isLock)
	{
		if (isLock)
		{
			m_Selectable.SetActiveLayer("Locked");
		}
		else
		{
			m_Selectable.SetActiveLayer(isInParty ? "Selected" : "Unselected");
		}
	}
}
