using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Globalmap.Blueprints;
using Owlcat.Fmw.Blueprints;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapPointPosition : MonoBehaviour
{
	[SerializeField]
	private BpRef<BlueprintArea>[] m_Areas;

	[SerializeField]
	private BpRef<BlueprintMultiEntranceEntry>[] m_Entries = new BpRef<BlueprintMultiEntranceEntry>[0];

	public BpRef<BlueprintMultiEntranceEntry>[] Entries => m_Entries;

	public bool IsDecent(TransitionMapEntityVM vm)
	{
		if (vm.Kind == MapEntityKind.Area)
		{
			return m_Areas.Any((BpRef<BlueprintArea> a) => Game.Instance.CurrentlyLoadedArea == a.MaybeBlueprint);
		}
		return m_Entries.Any((BpRef<BlueprintMultiEntranceEntry> e) => e.MaybeBlueprint?.Name == vm.Name);
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}
}
