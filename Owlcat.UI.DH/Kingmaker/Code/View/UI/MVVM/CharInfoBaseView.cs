using Assets.Code.View.UI.MVVM;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.Common;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM;

public class CharInfoBaseView : View<CharacterInfoVM>
{
	[Header("Menu")]
	[SerializeField]
	private MenuBaseView m_MenuView;

	[SerializeField]
	protected PartyPCWindowsView m_PartyView;

	[SerializeField]
	private CharInfoBigPortraitView m_BigPortraitView;

	[SerializeField]
	private CharInfoAbilitiesTabView m_Abilities;

	[SerializeField]
	private CharInfoArchetypeTabView m_Archetype;

	[SerializeField]
	private CharInfoCharacteristicsTabView m_Characteristics;

	[SerializeField]
	private CharInfoConvictionsTabView m_Convictions;

	[SerializeField]
	private GameObject m_HasModifierUpgrade;

	public void Initialize()
	{
		m_PartyView.Initialize();
		m_Abilities.Initialize();
		m_Archetype.Initialize();
		m_Characteristics.Initialize();
		m_Convictions.Initialize();
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_MenuView.Bind(base.ViewModel.MenuVM);
		m_PartyView.Bind(base.ViewModel.PartyVM);
		m_BigPortraitView.Bind(base.ViewModel.PortraitVM);
		base.ViewModel.AbilitiesVM.Subscribe(m_Abilities.Bind).AddTo(this);
		base.ViewModel.ArchetypeVM.Subscribe(m_Archetype.Bind).AddTo(this);
		base.ViewModel.ConvictionsVM.Subscribe(m_Convictions.Bind).AddTo(this);
		base.ViewModel.CharacteristicsVM.Subscribe(m_Characteristics.Bind).AddTo(this);
		base.ViewModel.HasModifierUpgrade.Subscribe(m_HasModifierUpgrade.SetActive).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		m_MenuView.Unbind();
		m_PartyView.Unbind();
		m_BigPortraitView.Unbind();
		m_Abilities.Unbind();
		m_Archetype.Unbind();
		m_Convictions.Unbind();
		m_Characteristics.Unbind();
	}
}
