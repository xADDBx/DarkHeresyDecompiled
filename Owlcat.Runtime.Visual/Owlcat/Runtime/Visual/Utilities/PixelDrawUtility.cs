using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Utilities;

[BurstCompile]
internal static class PixelDrawUtility
{
	public interface IPointPlotter
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void Plot(int x, int y);
	}

	public interface ILinePlotter
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void Plot(int xMin, int xMax, int y);
	}

	public static void PlotLine<T>(int x0, int y0, int x1, int y1, T plotter) where T : struct, IPointPlotter
	{
		PlotLine(x0, y0, x1, y1, ref plotter);
	}

	public static void PlotLine<T>(int x0, int y0, int x1, int y1, ref T plotter) where T : struct, IPointPlotter
	{
		int num = math.abs(x1 - x0);
		int num2 = ((x0 < x1) ? 1 : (-1));
		int num3 = -math.abs(y1 - y0);
		int num4 = ((y0 < y1) ? 1 : (-1));
		int num5 = num + num3;
		while (true)
		{
			plotter.Plot(x0, y0);
			int num6 = 2 * num5;
			if (num6 >= num3)
			{
				if (x0 == x1)
				{
					break;
				}
				num5 += num3;
				x0 += num2;
			}
			if (num6 <= num)
			{
				if (y0 == y1)
				{
					break;
				}
				num5 += num;
				y0 += num4;
			}
		}
	}

	public static void PlotCircle<T>(int centerX, int centerY, int radius, T plotter) where T : struct, IPointPlotter
	{
		PlotCircle(centerX, centerY, radius, ref plotter);
	}

	public static void PlotCircle<T>(int centerX, int centerY, int radius, ref T plotter) where T : struct, IPointPlotter
	{
		int num = radius / 16;
		int num2 = radius;
		int num3 = 0;
		while (num2 >= num3)
		{
			plotter.Plot(centerX + num2, centerY + num3);
			plotter.Plot(centerX - num2, centerY + num3);
			plotter.Plot(centerX + num2, centerY - num3);
			plotter.Plot(centerX - num2, centerY - num3);
			plotter.Plot(centerX + num3, centerY + num2);
			plotter.Plot(centerX - num3, centerY + num2);
			plotter.Plot(centerX + num3, centerY - num2);
			plotter.Plot(centerX - num3, centerY - num2);
			num3++;
			num += num3;
			int num4 = num - num2;
			if (num4 >= 0)
			{
				num = num4;
				num2--;
			}
		}
	}

	public static void FillCircle<T>(int centerX, int centerY, int radius, T plotter) where T : struct, ILinePlotter
	{
		FillCircle(centerX, centerY, radius, ref plotter);
	}

	public static void FillCircle<T>(int centerX, int centerY, int radius, ref T plotter) where T : struct, ILinePlotter
	{
		int num = radius / 16;
		int num2 = radius;
		int num3 = 0;
		while (num2 >= num3)
		{
			plotter.Plot(centerX - num2, centerX + num2, centerY + num3);
			plotter.Plot(centerX - num2, centerX + num2, centerY - num3);
			plotter.Plot(centerX - num3, centerX + num3, centerY + num2);
			plotter.Plot(centerX - num3, centerX + num3, centerY - num2);
			num3++;
			num += num3;
			int num4 = num - num2;
			if (num4 >= 0)
			{
				num = num4;
				num2--;
			}
		}
	}
}
