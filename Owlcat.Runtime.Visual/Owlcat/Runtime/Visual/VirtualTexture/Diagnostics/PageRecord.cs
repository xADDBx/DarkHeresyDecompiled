namespace Owlcat.Runtime.Visual.VirtualTexture.Diagnostics;

public struct PageRecord
{
	public int VirtualX;

	public int VirtualY;

	public int TextureId;

	public int MipLevel;

	public int PhysX;

	public int PhysY;

	public int PhysSlice;

	public bool IsLoading;

	public int FrameId;

	public bool IsReady;
}
