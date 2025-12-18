using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class AddendumLabel : View<BlueprintClueAddendum>
{
	[SerializeField]
	private TMP_Text m_Label;

	protected override void OnBind()
	{
		m_Label.text = "<b>•</b> " + base.ViewModel.Name.Text;
	}
}
