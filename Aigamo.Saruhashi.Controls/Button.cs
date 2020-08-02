﻿using System.Drawing;

namespace Aigamo.Saruhashi
{
	public class Button : ButtonBase
	{
		private PushButtonState DetermineState(bool up)
		{
			if (!up)
				return PushButtonState.Pressed;

			if (MouseIsOver)
				return PushButtonState.Hot;

			return PushButtonState.Normal;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			var clientRectangle = ClientRectangle;
			ButtonRenderer.DrawButton(e.Graphics, clientRectangle, DetermineState(!MouseIsDown));

			if (Font != null)
			{
				var text = GetText();
				var size = e.Graphics.MeasureString(text, Font);
				var bounds = new Rectangle(Point.Empty, Size);

				using (var brush = new SolidBrush(ForeColor))
					e.Graphics.DrawString(text, Font, brush, (PointF)bounds.Location + (bounds.Size - size) / 2);
			}
		}
	}
}
