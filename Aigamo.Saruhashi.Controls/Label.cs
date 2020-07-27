using System.Drawing;

namespace Aigamo.Saruhashi
{
	public class Label : ControlBase
	{
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (Font != null)
			{
				var text = GetText();

				// OPTIMIZE
				using (var brush = new SolidBrush(ForeColor))
					e.Graphics.DrawString(text, Font, brush, Point.Empty/* TODO */);
			}
		}
	}
}
