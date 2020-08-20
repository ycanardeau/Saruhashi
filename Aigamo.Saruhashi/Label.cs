using System.Drawing;

namespace Aigamo.Saruhashi
{
	public class Label : Control
	{
		private ITextRenderer? _textRenderer;

		// TODO
		protected override Size DefaultSize => new Size(100, 23);

		// OPTIMIZE
		public ITextRenderer TextRenderer
		{
			get => _textRenderer ??= new TextRenderer();
			set => _textRenderer = value;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor, TextFormatFlags.Default);

			base.OnPaint(e);
		}
	}
}
