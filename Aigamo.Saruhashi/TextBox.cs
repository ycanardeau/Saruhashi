// Code from: https://github.com/OpenRA/OpenRA/blob/7943f4deb6b49f549b4d4ee9afcbfb23bc67dcc5/OpenRA.Mods.Common/Widgets/TextFieldWidget.cs

using System;
using System.Drawing;

namespace Aigamo.Saruhashi
{
	public class TextBox : Control
	{
		private const int LeftMargin = 3;
		private const int RightMargin = 3;

		private TextBoxState _state = TextBoxState.Normal;
		private ITextBoxRenderer? _textBoxRenderer;

		protected override Size DefaultSize => new Size(100, 23/* TODO */);
		public int SelectionStart { get; set; }

		private int GetCaretIndex(Graphics graphics, int x)
		{
			if (Font == null)
				return 0;

			var textSize = graphics.MeasureString(Text, Font).ToSize();
			var start = LeftMargin;
			if (textSize.Width > ClientRectangle.Width - LeftMargin - RightMargin && Focused)
				start += ClientRectangle.Width - LeftMargin - RightMargin - textSize.Width;

			var minIndex = -1;
			var minValue = int.MaxValue;
			for (var i = 0; i <= Text.Length; i++)
			{
				var distance = Math.Abs(start + graphics.MeasureString(Text.Substring(0, i), Font).Width - x);
				if (distance > minValue)
					break;
				minValue = (int)distance;
				minIndex = i;
			}
			return minIndex;
		}

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

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			switch (e.KeyCode)
			{
				case Keys.Back:
					if (SelectionStart > 0)
					{
						SelectionStart--;
						Text = Text.Remove(SelectionStart, 1);
					}
					break;

				case Keys.Left:
					if (SelectionStart > 0)
						SelectionStart--;
					break;

				case Keys.Right:
					if (SelectionStart < Text.Length)
						SelectionStart++;
					break;

				case Keys.Delete:
					if (SelectionStart < Text.Length)
						Text = Text.Remove(SelectionStart, 1);
					break;
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);

			if (!char.IsControl(e.KeyChar))
			{
				Text = Text.Insert(SelectionStart, e.KeyChar.ToString());
				SelectionStart++;
			}
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);

			_state = TextBoxState.Normal;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			using (var graphics = CreateGraphics())
				SelectionStart = GetCaretIndex(graphics, e.Location.X);
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

			if (Font != null)
			{
				var textSize = e.Graphics.MeasureString(!string.IsNullOrEmpty(Text) ? Text : " ", Font).ToSize();
				var textPosition = new Point(LeftMargin, (clientRectangle.Height - textSize.Height) / 2);
				if (textSize.Width > clientRectangle.Width - LeftMargin - RightMargin && Focused)
					textPosition.X += clientRectangle.Width - LeftMargin - RightMargin - textSize.Width;

				// OPTIMIZE
				using (var brush = new SolidBrush(ForeColor))
					e.Graphics.DrawString(Text, Font, brush, textPosition);

				var caretPosition = new Point(e.Graphics.MeasureString(Text.Substring(0, SelectionStart), Font).ToSize());

				if (Focused)
				{
					// OPTIMIZE
					using (var brush = new SolidBrush(ForeColor))
						e.Graphics.FillRectangle(brush, new Rectangle(textPosition.X + caretPosition.X, textPosition.Y, 1, textSize.Height));
				}
			}
		}
	}
}
