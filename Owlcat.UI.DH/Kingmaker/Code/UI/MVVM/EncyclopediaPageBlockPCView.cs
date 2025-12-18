using System.Collections.Generic;
using Owlcat.UI;
using TMPro;

namespace Kingmaker.Code.UI.MVVM;

public abstract class EncyclopediaPageBlockPCView<TBlockVM> : View<TBlockVM> where TBlockVM : ViewModel
{
	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
			DoInitialize();
		}
	}

	public virtual void DoInitialize()
	{
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}

	public abstract List<TextMeshProUGUI> GetLinksTexts();
}
