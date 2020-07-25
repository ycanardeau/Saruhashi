using DrawingColor = System.Drawing.Color;
using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;
using DrawingSizeF = System.Drawing.SizeF;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaPoint = Microsoft.Xna.Framework.Point;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using XnaSize2 = MonoGame.Extended.Size2;

namespace Aigamo.Saruhashi.MonoGame
{
	public static class Extensions
	{
		public static DrawingColor ToDrawingColor(this XnaColor color) => DrawingColor.FromArgb(color.A, color.R, color.G, color.B);
		public static DrawingPoint ToDrawingPoint(this XnaPoint point) => new DrawingPoint(point.X, point.Y);
		public static DrawingRectangle ToDrawingRectangle(this XnaRectangle rectangle) => new DrawingRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		public static DrawingSizeF ToDrawingSize(this XnaSize2 size) => new DrawingSizeF(size.Width, size.Height);

		public static XnaColor ToXnaColor(this DrawingColor color) => new XnaColor(color.R, color.G, color.B, color.A);
		public static XnaPoint ToXnaPoint(this DrawingPoint point) => new XnaPoint(point.X, point.Y);
		public static XnaRectangle ToXnaRectangle(this DrawingRectangle rectangle) => new XnaRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		public static XnaSize2 ToXnaSize(this DrawingSizeF size) => new XnaSize2(size.Width, size.Height);
	}
}
