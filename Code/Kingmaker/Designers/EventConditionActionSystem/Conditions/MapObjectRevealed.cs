using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("787799ad384735e46b3535b236523644")]
public class MapObjectRevealed : Condition
{
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public override string GetDescription()
	{
		return "Заметил ли игрок мапобжект. Если мапобжект не под персепшеном, то он считается замеченным когда игрок его увидел в тумане войны";
	}

	protected override string GetConditionCaption()
	{
		return $"MapObject {MapObject} revealed";
	}

	protected override bool CheckCondition()
	{
		return MapObject.GetValue().IsRevealed;
	}
}
