using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class PartySelectorItemConsoleView : View<PartyCharacterVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private UnitPortraitPartPCView m_PortraitView;

	[SerializeField]
	private TextMeshProUGUI m_CharacterName;

	[SerializeField]
	private GameObject m_ConnectedIcon;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private GameObject m_LevelUpObject;

	public BaseUnitEntity UnitEntityData => base.ViewModel.UnitEntityData;

	public ReadOnlyReactiveProperty<bool> IsLinked => base.ViewModel.IsLinked;

	public ReadOnlyReactiveProperty<bool> IsLevelUp => base.ViewModel.IsLevelUp;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_PortraitView.Bind(base.ViewModel.PortraitPartVM);
		base.ViewModel.CharacterName.Subscribe(delegate(string n)
		{
			m_CharacterName.text = n;
		}).AddTo(this);
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		if ((loadedAreaState != null && loadedAreaState.Settings.CapitalPartyMode) || !UnitEntityData.IsDirectlyControllable())
		{
			m_ConnectedIcon.SetActive(value: false);
		}
		else
		{
			base.ViewModel.IsLinked.Subscribe(m_ConnectedIcon.SetActive).AddTo(this);
		}
		base.ViewModel.IsLevelUp.Subscribe(m_LevelUpObject.SetActive).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetSelected()
	{
		base.ViewModel.HandleUnitClick();
	}

	public void LevelUp()
	{
		base.ViewModel.LevelUp();
	}

	public void SetLink()
	{
		base.ViewModel.ToggleLinkUnit();
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
		SetSelected();
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}
}
