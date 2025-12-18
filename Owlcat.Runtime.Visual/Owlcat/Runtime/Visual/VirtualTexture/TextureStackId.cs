using System;

namespace Owlcat.Runtime.Visual.VirtualTexture;

public struct TextureStackId : IEquatable<TextureStackId>
{
	public Guid Layer0;

	public Guid Layer1;

	public Guid Layer2;

	public Guid Layer3;

	public Guid this[int index]
	{
		get
		{
			return index switch
			{
				0 => Layer0, 
				1 => Layer1, 
				2 => Layer2, 
				3 => Layer3, 
				_ => throw new ArgumentOutOfRangeException("index"), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				Layer0 = value;
				break;
			case 1:
				Layer1 = value;
				break;
			case 2:
				Layer2 = value;
				break;
			case 3:
				Layer3 = value;
				break;
			default:
				throw new ArgumentOutOfRangeException("index");
			}
		}
	}

	static TextureStackId()
	{
	}

	public bool IsEmpty()
	{
		if (IsGuidEmpty(in Layer0) && IsGuidEmpty(in Layer1) && IsGuidEmpty(in Layer2))
		{
			return IsGuidEmpty(in Layer3);
		}
		return false;
	}

	private bool IsGuidEmpty(in Guid guid)
	{
		return guid == Guid.Empty;
	}

	public bool HasLayer(in Guid layer)
	{
		if (!(Layer0 == layer) && !(Layer1 == layer) && !(Layer2 == layer))
		{
			return Layer3 == layer;
		}
		return true;
	}

	public bool Equals(TextureStackId other)
	{
		if (Layer0 == other.Layer0 && Layer1 == other.Layer1 && Layer2 == other.Layer2)
		{
			return Layer3 == other.Layer3;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((17 * 23 + Layer0.GetHashCode()) * 23 + Layer1.GetHashCode()) * 23 + Layer2.GetHashCode()) * 23 + Layer3.GetHashCode();
	}

	public override string ToString()
	{
		return $"{Layer0}\n{Layer1}\n{Layer2}\n{Layer3}";
	}
}
