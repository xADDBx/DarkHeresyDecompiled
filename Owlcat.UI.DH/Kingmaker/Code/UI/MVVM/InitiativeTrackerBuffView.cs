using JetBrains.Annotations;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class InitiativeTrackerBuffView : View<BuffVM>
{
	[SerializeField]
	[UsedImplicitly]
	private Image m_Icon;

	protected override void OnBind()
	{
		m_Icon.sprite = base.ViewModel.Icon;
	}
}
