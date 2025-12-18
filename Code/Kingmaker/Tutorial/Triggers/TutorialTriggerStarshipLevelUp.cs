using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("0f1d13b6f0674519bef963e4457dc164")]
public class TutorialTriggerStarshipLevelUp : BlueprintComponent
{
	[SerializeField]
	private int m_Level = 2;
}
