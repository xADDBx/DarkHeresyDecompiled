using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.Components.Camera;

public class UIPostProcessMember : MonoBehaviour, IDisposable
{
	[SerializeField]
	private RawImage TargetImage;

	[SerializeField]
	private Transform SourceObjectsStructure;

	[SerializeField]
	private Transform SourceObjectsContainer;

	private bool m_WasPushed;

	public IDisposable Bind()
	{
		Push();
		return this;
	}

	public void Dispose()
	{
		if (Application.isPlaying)
		{
			Pop();
		}
	}

	private void Push()
	{
		if (!m_WasPushed)
		{
			TargetImage.enabled = true;
			UIPostProcessSpace.Instance.Push(SourceObjectsStructure, SourceObjectsContainer, TargetImage);
			m_WasPushed = true;
		}
	}

	private void Pop()
	{
		if (m_WasPushed)
		{
			TargetImage.enabled = false;
			UIPostProcessSpace.Instance.Pop(SourceObjectsStructure);
			m_WasPushed = false;
		}
	}
}
