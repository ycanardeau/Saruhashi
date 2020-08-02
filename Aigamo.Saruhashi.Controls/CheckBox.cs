﻿using System;
using System.Drawing;

namespace Aigamo.Saruhashi
{
	public class CheckBox : ButtonBase
	{
		private bool _checked;

		public CheckBox() : base()
		{
			IsChecked = () => Checked;
		}

		public Appearance Appearance { get; set; }

		public bool Checked
		{
			get => _checked;
			set
			{
				var oldChecked = Checked;
				_checked = value;

				if (oldChecked != Checked)
					OnCheckedChanged(EventArgs.Empty);
			}
		}

		public Func<bool> IsChecked { get; set; }

		public event EventHandler? CheckedChanged;

		private CheckBoxState DetermineState(bool up)
		{
			var isChecked = IsChecked();

			if (isChecked)
			{
				if (!up)
					return CheckBoxState.CheckedPressed;

				if (MouseIsOver)
					return CheckBoxState.CheckedHot;

				return CheckBoxState.CheckedNormal;
			}
			else
			{
				if (!up)
					return CheckBoxState.UncheckedPressed;

				if (MouseIsOver)
					return CheckBoxState.UncheckedHot;

				return CheckBoxState.UncheckedNormal;
			}
		}

		protected virtual void OnCheckedChanged(EventArgs e) => CheckedChanged?.Invoke(this, e);

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

			switch (DetermineState(!MouseIsDown))
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
