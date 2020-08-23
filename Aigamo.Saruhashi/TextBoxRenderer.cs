using System.Drawing;

namespace Aigamo.Saruhashi
{
	public class TextBoxRenderer : ITextBoxRenderer
	{
		public void DrawTextBox(Graphics graphics, Rectangle bounds, TextBoxState state)
		{
			switch (state)
			{
				case TextBoxState.Normal:
					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(67, 67, 70)))
						graphics.FillRectangle(brush, bounds);

					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(51, 51, 55)))
						graphics.FillRectangle(brush, Rectangle.Inflate(bounds, -1, -1));
					break;

				case TextBoxState.Hot:
				case TextBoxState.Selected:
					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(0, 122, 204)))
						graphics.FillRectangle(brush, bounds);

					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(63, 63, 70)))
						graphics.FillRectangle(brush, Rectangle.Inflate(bounds, -1, -1));
					break;

				case TextBoxState.Disabled:
					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(67, 67, 70)))
						graphics.FillRectangle(brush, bounds);

					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(45, 45, 48)))
						graphics.FillRectangle(brush, Rectangle.Inflate(bounds, -1, -1));
					break;
			}
		}
	}
}
