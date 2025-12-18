using System;
using Kingmaker.Visual.Animation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.ResourceLinks;

[Serializable]
public class AnimationClipWrapperLink : ScriptableObjectLink<AnimationClipWrapper, AnimationClipWrapperLink>, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
