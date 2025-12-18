using System;
using System.Collections.Generic;
using Code.GameCore.Mics;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Stores.DlcInterfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[ComponentName("Root/DlcRoot")]
[TypeId("17c68ef1d18e67246a5ef2b3fc337e48")]
public class DlcRoot : BlueprintScriptableObject, IDlcRootService, IDlcRoot, InterfaceService
{
	[SerializeField]
	private BlueprintDlcReference[] m_Dlcs;

	public IEnumerable<IBlueprintDlc> Dlcs => m_Dlcs?.Dereference();

	public override void OnEnable()
	{
		base.OnEnable();
		InterfaceServiceLocator.UnregisterService(typeof(IDlcRootService));
		InterfaceServiceLocator.RegisterService(this, typeof(IDlcRootService));
	}
}
