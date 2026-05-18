using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Splines;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[TypeId("46e1f1b6025446cb9a8da45fe982be3f")]
public sealed class DetectiveSystemRoot : BlueprintScriptableObject, IBlueprintScanner, IScanOnBuild
{
	[Serializable]
	public class DetectiveTraceTypeCollection
	{
		public DetectiveTraceVisualType Type;

		public Texture TraceStepsTexture;

		public bool OrientStepsByCurve = true;

		public float DistanceBetweenSteps = 1f;

		public bool AlternateOffset;

		[ShowIf("AlternateOffset")]
		[Range(0f, 1f)]
		public float OffsetValue = 0.3f;

		public bool UseCustomColors;

		[ShowIf("UseCustomColors")]
		public Color FoundColor = Color.magenta;

		[ShowIf("UseCustomColors")]
		public Color FollowedBySkullColor = Color.cyan;
	}

	[ValidateNoNullEntries]
	public BpRef<BlueprintCase>[] Cases;

	public List<DetectiveTraceTypeCollection> TraceStepsVisual;

	public TangentMode StepsSplineType;

	[Range(0f, 1f)]
	public float SplineAutoTension = 1f / 3f;

	public PrefabLink ClueHighlightFx;

	public PrefabLink DetectiveObjectHighlight;

	public Color DefaultFoundTraceColor = new Color(0.2321184f, 95f / 106f, 0.08032215f);

	public Color DefaultFollowedBySkullTraceColor = new Color(0.8980392f, 0.3190215f, 0.07843137f);

	public static DetectiveSystemRoot Instance => ConfigRoot.Instance.DetectiveSystem;

	void IBlueprintScanner.Scan()
	{
		CollectCasesAndFixReferences();
	}

	void IScanOnBuild.Scan()
	{
		CollectCasesAndFixReferences();
	}

	private void CollectCasesAndFixReferences()
	{
	}
}
