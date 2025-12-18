using Animancer;
using Kingmaker.ResourceLinks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.Animation;

[ExecuteAlways]
public class StaticPose : MonoBehaviour
{
	[FormerlySerializedAs("m_OriginalPrefab")]
	[SerializeField]
	public GameObject OriginalPrefab;

	[SerializeField]
	private AnimationClipWrapperLink m_AnimationLink;

	[SerializeField]
	private int m_Frame;

	private AnimancerComponent m_Animancer;

	private AnimancerState m_AnimancerState;

	private void Start()
	{
		m_Animancer = GetComponent<AnimancerComponent>() ?? GetComponentInChildren<AnimancerComponent>();
		UpdateClip();
		UpdateFrame();
	}

	private void UpdateClip()
	{
		AnimationClip animationClip = m_AnimationLink.Load()?.AnimationClip;
		if (!(animationClip == null))
		{
			if (m_AnimancerState == null)
			{
				m_AnimancerState = m_Animancer.Play(animationClip);
			}
			else
			{
				m_AnimancerState.Clip = animationClip;
			}
		}
	}

	private void UpdateFrame()
	{
		if (m_AnimancerState != null)
		{
			AnimationClip clip = m_AnimancerState.Clip;
			m_AnimancerState.Time = Mathf.Clamp((float)m_Frame / clip.frameRate, 0f, clip.length);
			m_AnimancerState.Speed = 0f;
		}
	}
}
