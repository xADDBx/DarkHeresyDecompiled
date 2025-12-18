using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.View.Visual.Animation;

[CreateAssetMenu(fileName = "AnimationLibraryLabelsList", menuName = "Animation Manager/Animation library labels list")]
public class AnimationClipWrapperAnimLibraryLabels : ScriptableObject
{
	public List<string> labelsGenerated = new List<string>();

	public List<string> labelsCustom = new List<string>();
}
