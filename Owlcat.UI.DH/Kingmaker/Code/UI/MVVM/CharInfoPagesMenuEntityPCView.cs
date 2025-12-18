using System;
using DG.Tweening;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoPagesMenuEntityPCView : SelectionGroupEntityView<CharInfoPagesMenuEntityVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	private Transform m_Lens;

	private float m_LensDistanceThreshold;

	private float m_LensAnimationDuration;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Label.text = base.ViewModel.Label;
	}

	protected override void DestroyViewImplementation()
	{
		UISounds.Instance.Sounds.Selector.SelectorStop.Play();
		UISounds.Instance.Sounds.Selector.SelectorLoopStop.Play();
		base.DestroyViewImplementation();
	}

	public void SetupLens(Transform lens, float lensDistanceThreshold, float lensAnimationDuration)
	{
		m_Lens = lens;
		m_LensDistanceThreshold = lensDistanceThreshold;
		m_LensAnimationDuration = lensAnimationDuration;
		DelayedInvoker.InvokeInFrames(InitializeLensPosition, 1);
	}

	private void InitializeLensPosition()
	{
		if (base.ViewModel != null && base.ViewModel.IsSelected.Value && !(m_Lens == null))
		{
			DOTween.Kill(m_Lens.transform);
			m_Lens.transform.localPosition = base.gameObject.transform.localPosition;
		}
	}

	private void UpdateLensPosition()
	{
		if (base.ViewModel.IsSelected.Value && !(m_Lens == null) && !(Math.Abs(m_Lens.transform.localPosition.x - base.gameObject.transform.localPosition.x) < m_LensDistanceThreshold))
		{
			DOTween.Kill(m_Lens.transform);
			UIUtilityLens.MoveXLensPosition(m_Lens.transform, base.gameObject.transform.localPosition.x, m_LensAnimationDuration);
		}
	}
}
