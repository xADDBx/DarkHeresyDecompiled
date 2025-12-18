using System.Collections.Generic;
using DG.Tweening;
using Owlcat.UI;
using R3;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ChannelingLineView : View<ChannelingLineVM>
{
	[SerializeField]
	private int m_Density = 3;

	[SerializeField]
	public int m_MinPoint = 10;

	[SerializeField]
	[Range(0f, 2f)]
	private float m_FadeTime = 0.2f;

	[SerializeField]
	private LineRenderer m_LineRenderer;

	[SerializeField]
	private Gradient m_DefaultGradient;

	[SerializeField]
	private float m_ArcHight = 0.5f;

	protected override void OnBind()
	{
		base.ViewModel.StartPos.CombineLatest(base.ViewModel.EndPos, (Vector3 start, Vector3 end) => new { start, end }).Subscribe(value =>
		{
			SetArcLine(value.start, value.end);
		}).AddTo(this);
		base.ViewModel.IsVisible.Subscribe(m_LineRenderer.gameObject.SetActive).AddTo(this);
		m_DefaultGradient = m_LineRenderer.colorGradient;
		FadeInOut(0f, visible: true);
	}

	protected override void OnUnbind()
	{
		FadeInOut(null, visible: false).OnComplete(delegate
		{
			WidgetFactory.DisposeWidget(this);
		});
	}

	private Tweener FadeInOut(float? startAlpha, bool visible)
	{
		Color color = m_LineRenderer.material.color;
		color.a = startAlpha ?? color.a;
		m_LineRenderer.material.color = color;
		return m_LineRenderer.material.DOFade(visible ? 1f : 0f, m_FadeTime);
	}

	private void SetArcLine(Vector3 startPos, Vector3 endPos)
	{
		startPos += base.ViewModel.StartObjectOffset;
		endPos += base.ViewModel.EndObjectOffset;
		m_LineRenderer.colorGradient = m_DefaultGradient;
		m_LineRenderer.sharedMaterial.SetFloat(ShaderProps._TimeEditor, Time.unscaledTime - Time.time);
		Vector3 vector = (startPos + endPos) / 2f;
		Vector3 vector2 = new Vector3(0f, m_ArcHight, 0f);
		Vector3 control = vector + vector2;
		float num = Vector3.Distance(startPos, endPos);
		int num2 = Mathf.Max((int)Mathf.Floor((float)m_Density * num), m_MinPoint);
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i <= num2; i++)
		{
			float t = (float)i / (float)num2;
			Vector3 item = CalculateQuadraticBezierPoint(t, startPos, control, endPos);
			list.Add(item);
		}
		m_LineRenderer.positionCount = list.Count;
		m_LineRenderer.SetPositions(list.ToArray());
	}

	private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 start, Vector3 control, Vector3 end)
	{
		float num = 1f - t;
		float num2 = t * t;
		return num * num * start + 2f * num * t * control + num2 * end;
	}
}
