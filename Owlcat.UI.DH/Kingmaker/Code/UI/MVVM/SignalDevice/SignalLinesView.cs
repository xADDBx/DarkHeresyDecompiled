using System.Collections.Generic;
using Kingmaker.Blueprints;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.SignalDevice;

public class SignalLinesView : View<ReadOnlyReactiveProperty<float>>
{
	[SerializeField]
	private List<Image> m_LineImages = new List<Image>();

	[Header("Values")]
	[SerializeField]
	private Color m_DisabledColor = Color.gray;

	private Gradient LinesGradient => DetectiveClueSignalRoot.Instance.UISettings.ColorGradient;

	protected override void OnBind()
	{
		base.ViewModel.Subscribe(SetSignalPower).AddTo(this);
	}

	private void SetSignalPower(float power)
	{
		for (int i = 0; i < m_LineImages.Count; i++)
		{
			float num = 1f * (float)i / (float)m_LineImages.Count;
			if (num >= power)
			{
				m_LineImages[i].color = m_DisabledColor;
				continue;
			}
			Color color = LinesGradient.Evaluate(num);
			color.a = 0.5f;
			m_LineImages[i].color = color;
		}
	}
}
