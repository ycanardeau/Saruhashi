using System;
using System.Drawing;

namespace Aigamo.Saruhashi
{
	public class CheckBox : ControlBase
	{
		private CheckBoxState _state = CheckBoxState.UncheckedNormal;

		public CheckBox() : base()
		{
			IsChecked = () => Checked;
		}

		public Appearance Appearance { get; set; }
		public bool Checked { get; set; }
		public Func<bool> IsChecked { get; set; }

		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);
			
			switch (e.Button)
			{
				case MouseButtons.Left:
					Checked = !Checked;
					break;
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			var isChecked = IsChecked();
			_state = isChecked ? CheckBoxState.CheckedPressed : CheckBoxState.UncheckedPressed;
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);

			var isChecked = IsChecked();
			_state = isChecked ? CheckBoxState.CheckedHot : CheckBoxState.UncheckedHot;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			var isChecked = IsChecked();
			_state = isChecked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			var isChecked = IsChecked();
			_state = Capture && ClientRectangle.Contains(e.Location)
				? (isChecked ? CheckBoxState.CheckedPressed : CheckBoxState.UncheckedPressed)
				: (isChecked ? CheckBoxState.CheckedHot : CheckBoxState.UncheckedHot);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			var isChecked = IsChecked();
			_state = isChecked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			switch (Appearance)
			{
				case Appearance.Normal:
					throw new NotImplementedException();

				case Appearance.Button:
					OnPaintButton(e);
					break;
			}
		}

		private void OnPaintButton(PaintEventArgs e)
		{
			var clientRectangle = ClientRectangle;

			switch (_state)
			{
				case CheckBoxState.UncheckedNormal:
					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(45, 45, 48)))
						e.Graphics.FillRectangle(brush, clientRectangle);
					break;

				case CheckBoxState.UncheckedHot:
					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(62, 62, 64)))
						e.Graphics.FillRectangle(brush, clientRectangle);
					break;

				case CheckBoxState.UncheckedPressed:
					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(0, 122, 204)))
						e.Graphics.FillRectangle(brush, clientRectangle);
					break;

				case CheckBoxState.CheckedNormal:
					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(51, 153, 255)))
						e.Graphics.FillRectangle(brush, clientRectangle);

					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(45, 45, 48)))
						e.Graphics.FillRectangle(brush, Rectangle.Inflate(clientRectangle, -1, -1));
					break;

				case CheckBoxState.CheckedHot:
				case CheckBoxState.CheckedPressed:
					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(51, 153, 255)))
						e.Graphics.FillRectangle(brush, clientRectangle);

					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(62, 62, 64)))
						e.Graphics.FillRectangle(brush, Rectangle.Inflate(clientRectangle, -1, -1));
					break;
			}

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
