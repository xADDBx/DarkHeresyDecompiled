using System;
using Kingmaker.Visual.Animation.Kingmaker;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.ResourceLinks;

[Serializable]
public class UnitAnimationActionLink : ScriptableObjectLink<UnitAnimationAction, UnitAnimationActionLink>, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
