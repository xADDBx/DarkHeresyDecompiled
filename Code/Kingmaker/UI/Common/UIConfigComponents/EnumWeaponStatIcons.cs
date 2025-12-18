using System;
using System.Collections.Generic;
using Kingmaker.Enums;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI.Common.UIConfigComponents;

[TypeId("a5f065079b064e6b92c63d9cfb7d355c")]
public class EnumWeaponStatIcons : EnumSpritesBlueprint<WeaponStat>
{
	[Serializable]
	public class MyEntry : Entry, IHashable
	{
		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	public MyEntry[] Entries;

	public EnumWeaponStatIcons()
	{
		Entries = CreateEntries<MyEntry>();
	}

	protected override IEnumerable<Entry> GetEntries()
	{
		return Entries;
	}
}
