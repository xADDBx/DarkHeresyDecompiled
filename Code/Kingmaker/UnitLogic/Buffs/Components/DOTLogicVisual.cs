using System;
using Code.Enums;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Serializable]
[AllowedOn(typeof(BlueprintBuff))]
[ComponentName("UI/DOTLogicVisual")]
[TypeId("154b566cab8b4735bd573f4bea26019f")]
public class DOTLogicVisual : BlueprintComponent
{
	public DOT Type;
}
