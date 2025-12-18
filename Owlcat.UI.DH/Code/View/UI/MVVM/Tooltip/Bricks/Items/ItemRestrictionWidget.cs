using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public class ItemRestrictionWidget : View<string>
{
	[SerializeField]
	private TMP_Text m_Text;

	protected override void OnBind()
	{
		m_Text.text = base.ViewModel;
	}
}
