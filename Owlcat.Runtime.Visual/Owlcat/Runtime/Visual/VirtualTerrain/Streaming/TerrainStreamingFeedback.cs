using System;
using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal static class TerrainStreamingFeedback
{
	private static readonly List<IFeedbackProvider> s_FeedbackProviders = new List<IFeedbackProvider>();

	public static Vector3 FeedbackPosition { get; set; }

	public static void RegisterFeedbackProvider(IFeedbackProvider provider)
	{
		s_FeedbackProviders.Add(provider);
	}

	public static void UnregisterFeedbackProvider(IFeedbackProvider provider)
	{
		s_FeedbackProviders.Remove(provider);
	}

	public static void GetFeedback(Span<int> layerLods)
	{
		foreach (IFeedbackProvider s_FeedbackProvider in s_FeedbackProviders)
		{
			s_FeedbackProvider.GetFeedback(FeedbackPosition, layerLods);
		}
	}
}
