using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("314915f554a44185965ce69e8045ef71")]
public class EtudeBracketDeactivateStarSystems : BlueprintComponent
{
	[SerializeField]
	private List<BlueprintSectorMapPointReference> m_StarSystems;
}
