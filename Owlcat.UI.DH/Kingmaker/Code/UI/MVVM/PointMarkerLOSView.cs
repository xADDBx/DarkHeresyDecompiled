using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class PointMarkerLOSView : View<LineOfSightVM>
{
	[SerializeField]
	private LineOfSightColor[] m_ColorsTable;

	[SerializeField]
	private Image m_Image;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.ViewModel.IsVisible.Subscribe(base.gameObject.SetActive).AddTo(this);
		base.ViewModel.HitChance.Subscribe(delegate(float value)
		{
			m_Image.color = GetColorByHitChance(value);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}

	private Color GetColorByHitChance(float hitChance)
	{
		LineOfSightColor[] colorsTable = m_ColorsTable;
		foreach (LineOfSightColor lineOfSightColor in colorsTable)
		{
			if (hitChance <= lineOfSightColor.HitChance)
			{
				return lineOfSightColor.Color;
			}
		}
		return default(Color);
	}
}
