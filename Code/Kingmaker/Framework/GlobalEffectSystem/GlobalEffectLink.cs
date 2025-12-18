using System;
using Kingmaker.ResourceLinks;
using Owlcat.Runtime.Visual.Effects.GlobalEffects;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Framework.GlobalEffectSystem;

[Serializable]
public sealed class GlobalEffectLink : WeakResourceLink<GlobalEffect>, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
