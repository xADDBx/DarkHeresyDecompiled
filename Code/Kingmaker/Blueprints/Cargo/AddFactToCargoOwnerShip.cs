using System;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Cargo;

[Obsolete]
[AllowedOn(typeof(BlueprintCargo))]
[AllowMultipleComponents]
[TypeId("3b132dd1bc5e46559f7fa8d38ec622f5")]
public class AddFactToCargoOwnerShip : BlueprintComponent
{
	[SerializeField]
	private BlueprintUnitFactReference m_Fact;
}
