using System.Collections.Generic;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class HeightLineView : View<int>
{
	[Header("Elements")]
	[SerializeField]
	private List<TMP_Text> m_HeightTexts = new List<TMP_Text>();

	[SerializeField]
	private OwlcatMultiSelectable m_ZeroDigitSelectable;

	protected override void OnBind()
	{
		m_HeightTexts.ForEach(delegate(TMP_Text t)
		{
			t.text = ((base.ViewModel > 0) ? base.ViewModel.ToString() : string.Empty);
		});
		m_ZeroDigitSelectable.SetActiveLayer((base.ViewModel % 10 != 0) ? 1 : 0);
	}
}
