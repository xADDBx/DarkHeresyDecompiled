using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View.MapObjects.Traps;

[KnowledgeDatabaseID("41994797e4d57064dbaefbc28636988c")]
public class NodeLinkView : MapObjectView
{
	public WarhammerNodeLinkViewSettings Settings;

	private Transform m_StairsInteractionOriginalParent;

	public override void OnAreaDidLoad()
	{
		base.OnAreaDidLoad();
		_ = (bool)Settings.StairsInteractionView;
		m_StairsInteractionOriginalParent = Settings.StairsInteractionView.ViewTransform.parent;
		Settings.StairsInteractionView.ViewTransform.SetParent(base.ViewTransform, worldPositionStays: true);
	}

	public override void OnAreaBeginUnloading()
	{
		base.OnAreaBeginUnloading();
		if ((bool)m_StairsInteractionOriginalParent)
		{
			Settings.StairsInteractionView.Or(null)?.ViewTransform.SetParent(m_StairsInteractionOriginalParent, worldPositionStays: true);
		}
	}

	public void OnDeactivated()
	{
		Settings.StairsInteractionView.Or(null)?.ViewTransform.SetParent(m_StairsInteractionOriginalParent, worldPositionStays: true);
	}

	public void OnBeforeMechanicsReload()
	{
	}

	public void OnMechanicsReloaded()
	{
		if ((bool)Settings.StairsInteractionView && m_StairsInteractionOriginalParent == null)
		{
			m_StairsInteractionOriginalParent = Settings.StairsInteractionView.ViewTransform.parent;
			Settings.StairsInteractionView.ViewTransform.SetParent(base.ViewTransform, worldPositionStays: true);
		}
	}
}
