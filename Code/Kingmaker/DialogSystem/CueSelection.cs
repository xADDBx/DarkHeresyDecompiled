using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.DialogSystem;

[Serializable]
public class CueSelection
{
	public List<BlueprintCueBaseReference> Cues = new List<BlueprintCueBaseReference>();

	[Tooltip("In standard dialog will be selected the Cue with the fulfilled conditions will be by strategy: First or Random")]
	public Strategy Strategy;

	[CanBeNull]
	public BlueprintCueBase Select()
	{
		List<BlueprintCueBase> list = null;
		foreach (BlueprintCueBase item in Cues.Dereference())
		{
			if (item != null && item.CanShow())
			{
				if (Strategy == Strategy.First)
				{
					DialogDebug.Add(item, "selected");
					return item;
				}
				if (list == null)
				{
					list = new List<BlueprintCueBase>();
				}
				list.Add(item);
			}
		}
		if (list == null)
		{
			return null;
		}
		if (Strategy == Strategy.Random)
		{
			int index = PFStatefulRandom.DialogSystem.Range(0, list.Count);
			return list[index];
		}
		return null;
	}
}
