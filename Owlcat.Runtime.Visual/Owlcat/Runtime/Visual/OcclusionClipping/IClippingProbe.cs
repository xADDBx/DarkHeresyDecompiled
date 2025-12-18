using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

public interface IClippingProbe
{
	float4 BoundingSphere { get; }
}
