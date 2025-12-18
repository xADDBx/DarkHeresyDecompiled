using System;
using System.Text;
using Code.Enums;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[Serializable]
[TypeId("201fdfba40fb4b01878e7c56cf37d3cc")]
public class ContextActionTickDOT : ContextAction
{
	public bool AllTypes;

	[HideIf("AllTypes")]
	public DOT Type;

	[Tooltip("Некоторые фичи заменяют урон от ДОТов на хил, эта галка говорит, что хила не будет")]
	public bool DamageOnly;

	protected override void RunAction()
	{
		if (base.Target.Entity == null)
		{
			return;
		}
		if (AllTypes)
		{
			foreach (DOT value in EnumUtils.GetValues<DOT>())
			{
				DOTLogic.TryDealDamage(base.Target.Entity, value, DamageOnly);
			}
			return;
		}
		DOTLogic.TryDealDamage(base.Target.Entity, Type, DamageOnly);
	}

	public override string GetCaption()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		if (AllTypes)
		{
			builder.Append("Tick all DOTs");
		}
		else
		{
			builder.Append("Tick [");
			builder.Append(Type.ToString());
			builder.Append("]");
			builder.Append(" DOT");
		}
		return builder.ToString();
	}
}
