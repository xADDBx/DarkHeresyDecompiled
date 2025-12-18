using System;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Exploration;

[Obsolete]
[TypeId("b243b88abf7c442981843c74cfbdbe21")]
public class AnomalyDialog : AnomalyInteraction
{
	[SerializeField]
	private BlueprintDialogReference m_Dialog;

	public BlueprintDialog Dialog => m_Dialog?.Get();
}
