using System.Drawing;

namespace Aigamo.Saruhashi
{
	public class Label : Control
	{
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (Font != null)
			{
				// OPTIMIZE
				using (var brush = new SolidBrush(ForeColor))
					e.Graphics.DrawString(Text, Font, brush, Point.Empty/* TODO */);
			}
		}
	}
}
