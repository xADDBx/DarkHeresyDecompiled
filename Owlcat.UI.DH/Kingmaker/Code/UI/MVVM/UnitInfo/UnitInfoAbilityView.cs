using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoAbilityView : View<UnitInfoAbilityVM>
{
	[SerializeField]
	private AbilityIconView m_IconView;

	protected override void OnBind()
	{
		m_IconView.Bind(base.ViewModel.AbilityIconVM);
		Disposable.Create(delegate
		{
			m_IconView.Unbind();
		}).AddTo(this);
	}
}
