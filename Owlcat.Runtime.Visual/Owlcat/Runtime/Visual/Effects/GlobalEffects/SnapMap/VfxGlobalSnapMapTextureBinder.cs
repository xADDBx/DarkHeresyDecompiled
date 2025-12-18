using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.SnapMap;

[VFXBinder("Global Effects/VfxGlobalSnapMapTextureBinder")]
public class VfxGlobalSnapMapTextureBinder : VFXBinderBase
{
	[VFXPropertyBinding(new string[] { "UnityEngine.Texture2D" })]
	public ExposedProperty SnapMapTextureProperty = "SnapMapTexture";

	[VFXPropertyBinding(new string[] { "UnityEngine.Vector3" })]
	public ExposedProperty SnapMapBoundsSizeProperty = "SnapMapBoundsSize";

	[VFXPropertyBinding(new string[] { "UnityEngine.Vector3" })]
	public ExposedProperty SnapMapBoundsCenterProperty = "SnapMapBoundsCenter";

	[VFXPropertyBinding(new string[] { "UnityEngine.Vector2" })]
	public ExposedProperty SnapMapTexelSizeProperty = "SnapMapTexelSize";

	[VFXPropertyBinding(new string[] { "UnityEngine.Vector2" })]
	public ExposedProperty SnapMapTextureSizeProperty = "SnapMapTextureSize";

	public override bool IsValid(VisualEffect component)
	{
		if (component.HasTexture(SnapMapTextureProperty) && component.HasVector3(SnapMapBoundsSizeProperty) && component.HasVector3(SnapMapBoundsCenterProperty) && component.HasVector2(SnapMapTexelSizeProperty))
		{
			return component.HasVector2(SnapMapTextureSizeProperty);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		Texture2D texture = VfxGlobalSnapMap.GetTexture();
		Bounds textureBounds = VfxGlobalSnapMap.GetTextureBounds();
		Vector2 texelSize = VfxGlobalSnapMap.GetTexelSize();
		if (texture != null)
		{
			component.SetTexture(SnapMapTextureProperty, VfxGlobalSnapMap.GetTexture());
			component.SetVector2(v: new Vector2(texture.width, texture.height), nameID: SnapMapTextureSizeProperty);
		}
		component.SetVector3(SnapMapBoundsCenterProperty, textureBounds.center);
		component.SetVector3(SnapMapBoundsSizeProperty, textureBounds.size);
		component.SetVector2(SnapMapTexelSizeProperty, texelSize);
	}
}
