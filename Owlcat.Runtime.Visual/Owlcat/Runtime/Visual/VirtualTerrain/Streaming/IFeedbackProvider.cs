using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal interface IFeedbackProvider
{
	void GetFeedback(Vector3 probePosition, Span<int> layerLods);
}
