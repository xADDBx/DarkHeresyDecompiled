using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("984f1f574d084e9ab538270e374c0768")]
public class TutorialTriggerExitToGlobalMap : BlueprintComponent
{
	private int m_CountNeeded = 2;
}
