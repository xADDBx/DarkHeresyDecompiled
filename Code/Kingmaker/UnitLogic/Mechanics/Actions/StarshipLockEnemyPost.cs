using System;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("4748805e9e9351444a59978e21b7352e")]
public class StarshipLockEnemyPost : BlueprintComponent
{
	[Tooltip("Random range of rounds to lock post")]
	public int lockTurnsMin = 1;

	[Tooltip("Random range of rounds to lock post")]
	public int lockTurnsMax = 5;

	[Tooltip("How many lock attempts to do")]
	public int lockNum = 1;

	[SerializeField]
	private BlueprintBuffReference[] m_LockBuffs;

	public int LockBuffsCnt
	{
		get
		{
			BlueprintBuffReference[] lockBuffs = m_LockBuffs;
			if (lockBuffs == null)
			{
				return 0;
			}
			return lockBuffs.Length;
		}
	}

	public BlueprintBuff LockBuff(int idx)
	{
		return m_LockBuffs?.Get(idx);
	}
}
