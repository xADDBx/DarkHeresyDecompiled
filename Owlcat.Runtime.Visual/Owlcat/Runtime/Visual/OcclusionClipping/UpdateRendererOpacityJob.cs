using Unity.Burst;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[BurstCompile]
internal struct UpdateRendererOpacityJob : IJob
{
	public State State;

	public float OpacityDelta;

	public void Execute()
	{
		foreach (int dirtyOpacityRendererId in State.DirtyOpacityRendererIds)
		{
			RendererData value = State.Renderers[dirtyOpacityRendererId];
			if (value.ClipCounter > 0)
			{
				if (value.Opacity > 0f)
				{
					value.Opacity -= OpacityDelta;
					if (value.Opacity <= 0f)
					{
						value.Opacity = 0f;
					}
					else
					{
						State.NextDirtyOpacityRendererIds.Add(dirtyOpacityRendererId);
					}
				}
			}
			else if (value.Opacity < 1f)
			{
				value.Opacity += OpacityDelta;
				if (value.Opacity >= 1f)
				{
					value.Opacity = 1f;
				}
				else
				{
					State.NextDirtyOpacityRendererIds.Add(dirtyOpacityRendererId);
				}
			}
			State.Renderers[dirtyOpacityRendererId] = value;
		}
	}
}
