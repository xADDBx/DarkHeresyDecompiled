using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using Owlcat.Fmw.Blueprints;
using UnityEngine;

namespace Kingmaker.Framework.Abilities.Utility;

[Serializable]
[JsonObject]
public sealed class AbilityModifierTags : IEnumerable<BlueprintAbilityTag>, IEnumerable
{
	[SerializeField]
	private bool _universal;

	[HideIf("_universal")]
	[SerializeField]
	private BpRef<BlueprintAbilityTag>[] _tags = new BpRef<BlueprintAbilityTag>[0];

	public bool IsUniversal => _universal;

	public bool Match(BpRefArray<BlueprintAbilityTag> tags)
	{
		if (!_universal)
		{
			BpRef<BlueprintAbilityTag>[] tags2 = _tags;
			if (tags2 == null || tags2.Length != 0)
			{
				return tags.Any((BlueprintAbilityTag i) => _tags.Contains(i));
			}
			return false;
		}
		return true;
	}

	public bool Match(BlueprintAbilityTag tag)
	{
		if (!_universal)
		{
			BpRef<BlueprintAbilityTag>[] tags = _tags;
			if (tags == null || tags.Length != 0)
			{
				return _tags.Contains(tag);
			}
			return false;
		}
		return true;
	}

	public IEnumerator<BlueprintAbilityTag> GetEnumerator()
	{
		if (_universal)
		{
			yield return ConfigRoot.Instance.AbilityRoot.UniversalTag.Blueprint;
			yield break;
		}
		BpRef<BlueprintAbilityTag>[] tags = _tags;
		foreach (BpRef<BlueprintAbilityTag> bpRef in tags)
		{
			yield return bpRef;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
