using UnityEngine;
using UnityEngine.Playables;

[ExecuteAlways]
public class TimelineVFXAutoPreviewOptimized : MonoBehaviour
{
	[Tooltip("Ваш PlayableDirector")]
	public PlayableDirector director;

	[Tooltip("Авто-превью ParticleSystem при ▶\ufe0f Play в Timeline")]
	public bool autoPreview;
}
