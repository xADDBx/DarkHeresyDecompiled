using System;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.ResourceLinks;

[Serializable]
public class ScriptableObjectLink<T, TLink> : WeakResourceLink<T>, IHashable where T : ScriptableObject where TLink : ScriptableObjectLink<T, TLink>, new()
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
