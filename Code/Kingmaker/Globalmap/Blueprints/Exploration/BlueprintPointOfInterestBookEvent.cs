using System;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[Obsolete]
[TypeId("88f183371f584e10b325337ce9862d68")]
public class BlueprintPointOfInterestBookEvent : BlueprintPointOfInterest
{
	[SerializeField]
	private BlueprintDialogReference m_BookEvent;

	public bool IsRepeating;

	public BlueprintDialog BookEvent => m_BookEvent?.Get();
}
