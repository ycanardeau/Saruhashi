using System;
using System.Drawing;

namespace Aigamo.Saruhashi
{
	public class TextBox : Control
	{
		private TextBoxState _state = TextBoxState.Normal;
		private ITextBoxRenderer? _textBoxRenderer;

		protected override Size DefaultSize => new Size(100, 23/* TODO */);
		public int SelectionStart { get; }

		// OPTIMIZE
		public ITextBoxRenderer TextBoxRenderer
		{
			get => _textBoxRenderer ??= new TextBoxRenderer();
			set => _textBoxRenderer = value;
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);

			_state = TextBoxState.Selected;
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);

			_state = TextBoxState.Normal;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);

			_state = TextBoxState.Hot;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			_state = Focused ? TextBoxState.Selected : TextBoxState.Normal;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			var clientRectangle = ClientRectangle;
			TextBoxRenderer.DrawTextBox(e.Graphics, clientRectangle, _state);
		}
	}
}
