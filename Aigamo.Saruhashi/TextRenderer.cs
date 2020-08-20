using System.Drawing;

namespace Aigamo.Saruhashi
{
	public static class TextRenderer
	{
		public static void DrawText(Graphics graphics, string? text, IFont? font, Rectangle bounds, Color foreColor, TextFormatFlags flags)
		{
			if (string.IsNullOrEmpty(text) || foreColor == Color.Transparent)
				return;

			if (font == null)
				return;

			var textBounds = graphics.MeasureString(text, font);

			float x;
			if (flags.HasFlag(TextFormatFlags.HorizontalCenter))
				x = (bounds.Width - textBounds.Width) / 2;
			else if (flags.HasFlag(TextFormatFlags.Right))
				x = bounds.Width - textBounds.Width;
			else
				x = bounds.X;

			float y;
			if (flags.HasFlag(TextFormatFlags.VerticalCenter))
				y = (bounds.Height - textBounds.Height) / 2;
			else if (flags.HasFlag(TextFormatFlags.Bottom))
				y = bounds.Height - textBounds.Height;
			else
				y = bounds.Y;

			var point = new PointF(x, y);

			// OPTIMIZE
			using (var brush = new SolidBrush(foreColor))
				graphics.DrawString(text, font, brush, point);
		}
	}
}
