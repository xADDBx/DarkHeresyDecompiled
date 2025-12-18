using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.Parts.AdditionalCombat;

public class OvertipAdditionalCombatObjectBlockView : View<OvertipAdditionalCombatObjectBlockVM>
{
	[SerializeField]
	private CanvasGroup m_NewIcon;

	public void HideInstant()
	{
		m_NewIcon.alpha = 0f;
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.IsNewAdditionalCombatObject.Subscribe(delegate(bool value)
		{
			m_NewIcon.alpha = (value ? 1f : 0f);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		HideInstant();
	}
}
