using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CombatLogSeparatorView : VirtualListElementViewBase<CombatLogSeparatorVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutElementSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutElementSettings;

	protected override void OnBind()
	{
	}

	protected override void OnUnbind()
	{
	}

	public void SetFocus(bool value)
	{
	}

	public bool IsValid()
	{
		return false;
	}
}
