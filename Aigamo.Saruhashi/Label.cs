using System.Drawing;

namespace Aigamo.Saruhashi
{
	public class Label : Control
	{
		// TODO
		protected override Size DefaultSize => new Size(100, 23);

		protected override void OnPaint(PaintEventArgs e)
		{
			if (Font != null)
			{
				var text = Text;

				// OPTIMIZE
				using (var brush = new SolidBrush(ForeColor))
					e.Graphics.DrawString(text, Font, brush, Point.Empty/* TODO */);
			}

			base.OnPaint(e);
		}
	}
}
