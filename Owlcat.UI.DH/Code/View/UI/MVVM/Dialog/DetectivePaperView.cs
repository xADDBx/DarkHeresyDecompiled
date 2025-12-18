using Owlcat.UI;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Code.View.UI.MVVM.Dialog;

public class DetectivePaperView : View<Unit>
{
	[SerializeField]
	private Image m_Raycaster;

	protected override void OnBind()
	{
		m_Raycaster.OnPointerClickAsObservable().Subscribe(delegate
		{
			base.transform.SetAsLastSibling();
		}).AddTo(this);
	}
}
