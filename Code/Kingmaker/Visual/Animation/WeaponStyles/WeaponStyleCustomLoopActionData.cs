using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[Serializable]
public class WeaponStyleCustomLoopActionData : IWeaponStyleAnimationClipsProvider
{
	[Serializable]
	public class Entry
	{
		public BlueprintCustomLoopActionType.Reference Type;

		public AnimationClipWrapper In;

		public AnimationClipWrapper Out;

		public AnimationClipWrapper Loop;
	}

	[SerializeField]
	[ProvideNameWithProperty("Type")]
	private List<Entry> m_Entries;

	public Entry this[BlueprintCustomLoopActionType type] => m_Entries.SingleOrDefault((Entry x) => x.Type.Blueprint == type);

	public IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		foreach (Entry entry in m_Entries)
		{
			yield return entry.In;
			yield return entry.Out;
			yield return entry.Loop;
		}
	}
}
