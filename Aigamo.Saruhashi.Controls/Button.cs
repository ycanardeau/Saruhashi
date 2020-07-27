using System;
using System.Drawing;

namespace Aigamo.Saruhashi
{
	public class Button : ControlBase
	{
		private PushButtonState _state = PushButtonState.Normal;

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			_state = PushButtonState.Pressed;
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);

			_state = PushButtonState.Hot;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			_state = PushButtonState.Normal;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			_state = Capture && ClientRectangle.Contains(e.Location) ? PushButtonState.Pressed : PushButtonState.Hot;
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			_state = PushButtonState.Normal;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			var clientRectangle = ClientRectangle;
			ButtonRenderer.DrawButton(e.Graphics, clientRectangle, _state);

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
