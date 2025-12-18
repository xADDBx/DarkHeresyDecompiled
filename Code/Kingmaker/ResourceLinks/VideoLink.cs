using System;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Video;

namespace Kingmaker.ResourceLinks;

[Serializable]
public class VideoLink : WeakResourceLink<VideoClip>, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
