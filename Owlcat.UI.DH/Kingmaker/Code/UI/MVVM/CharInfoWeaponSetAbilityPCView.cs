using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoWeaponSetAbilityPCView : View<CharInfoWeaponSetAbilityVM>
{
	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	public SimpleConsoleNavigationEntity NavigationEntity { get; private set; }

	protected override void OnBind()
	{
		base.ViewModel.Icon.Subscribe(delegate(Sprite value)
		{
			m_Icon.sprite = value;
		}).AddTo(this);
		m_Icon.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		NavigationEntity = new SimpleConsoleNavigationEntity(m_Button, base.ViewModel.Tooltip);
	}
}
