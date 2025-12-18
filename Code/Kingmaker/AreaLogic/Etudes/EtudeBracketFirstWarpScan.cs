using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("2cc0176d77ee41d7a259d53d5844c86f")]
public class EtudeBracketFirstWarpScan : BlueprintComponent
{
	[SerializeField]
	private ActionList m_OnTriggerActions;
}
