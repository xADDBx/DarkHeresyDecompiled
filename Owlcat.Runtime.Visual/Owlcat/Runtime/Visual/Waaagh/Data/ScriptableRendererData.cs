using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures;
using UnityEngine;
using UnityEngine.Serialization;

namespace Owlcat.Runtime.Visual.Waaagh.Data;

public abstract class ScriptableRendererData : ScriptableObject
{
	[FormerlySerializedAs("m_RefactoringRendererFeatures")]
	[SerializeField]
	internal List<RendererFeatureAsset> m_RendererFeatures = new List<RendererFeatureAsset>();

	internal bool IsInvalidated { get; set; }

	public List<RendererFeatureAsset> RendererFeatures => m_RendererFeatures;

	internal IPipelineRenderer InternalCreateRenderer()
	{
		IsInvalidated = false;
		return Create();
	}

	protected abstract IPipelineRenderer Create();

	public new void SetDirty()
	{
		IsInvalidated = true;
	}

	protected virtual void OnValidate()
	{
		SetDirty();
	}

	protected virtual void OnEnable()
	{
		SetDirty();
	}
}
