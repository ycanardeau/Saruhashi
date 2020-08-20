using System.Drawing;

namespace Aigamo.Saruhashi
{
	public class Label : Control
	{
		// TODO
		protected override Size DefaultSize => new Size(100, 23);

		protected override void OnPaint(PaintEventArgs e)
		{
			TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor, TextFormatFlags.Default);

			base.OnPaint(e);
		}
	}
}
