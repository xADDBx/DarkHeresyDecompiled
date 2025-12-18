using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.EntitySystem;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Buffs;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b7255b45d89e77149971bb7216d39673")]
public class ChangeUnitSize : UnitFactComponentDelegate
{
	private enum ChangeType
	{
		Delta,
		Value
	}

	[SerializeField]
	private ChangeType m_Type;

	[ShowIf("IsTypeDelta")]
	public int SizeDelta;

	[ShowIf("IsTypeValue")]
	public Size Size;

	[UsedImplicitly]
	private bool IsTypeValue => m_Type == ChangeType.Value;

	[UsedImplicitly]
	private bool IsTypeDelta => m_Type == ChangeType.Delta;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<UnitPartSizeModifier>().Add(base.Fact);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOrCreate<UnitPartSizeModifier>().Remove(base.Fact);
	}

	public Size GetUnitSize(EntityFactComponent runtime)
	{
		using (runtime.SetScope())
		{
			return m_Type switch
			{
				ChangeType.Delta => base.Owner.OriginalSize.Shift(SizeDelta), 
				ChangeType.Value => Size, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
	}
}
