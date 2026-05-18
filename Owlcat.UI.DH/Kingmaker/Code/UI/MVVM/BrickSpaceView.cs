using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickSpaceView : BrickBaseView<BrickSpaceVM>
{
	[SerializeField]
	private LayoutElement m_LayoutElement;

	private float m_DefaultHeight = 15f;

	private void Awake()
	{
		m_DefaultHeight = m_LayoutElement.minHeight;
	}

	protected override void OnBind()
	{
		if (base.ViewModel.Height.HasValue)
		{
			m_LayoutElement.minHeight = base.ViewModel.Height.Value;
		}
	}

	protected override void OnUnbind()
	{
		m_LayoutElement.minHeight = m_DefaultHeight;
	}
}
