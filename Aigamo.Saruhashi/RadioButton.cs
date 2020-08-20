using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace Aigamo.Saruhashi
{
	public class RadioButton : ButtonBase
	{
		private bool _autoCheck = true;
		private bool _checked;

		public RadioButton() : base()
		{
			SetStyle(ControlStyles.StandardClick, false);
		}

		public Appearance Appearance { get; set; }

		public bool AutoCheck
		{
			get => _autoCheck;
			set
			{
				if (_autoCheck == value)
					return;

				_autoCheck = value;
				PerformAutoUpdates();
			}
		}

		public bool Checked
		{
			get => _checked;
			set
			{
				if (_checked == value)
					return;

				_checked = value;
				PerformAutoUpdates();
				OnCheckedChanged(EventArgs.Empty);
			}
		}

		public event EventHandler? CheckedChanged;

		private CheckBoxState DetermineState(bool up)
		{
			var isChecked = Checked;

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

		protected override void OnClick(EventArgs e)
		{
			if (AutoCheck)
				Checked = true;

			base.OnClick(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (Capture && WindowManager.WindowFromPoint(PointToScreen(e.Location)) == this)
				{
					OnClick(EventArgs.Empty);
					OnMouseClick(new MouseEventArgs(e.Button, e.Clicks, PointToClient(e.Location), e.Delta));
				}
			}

			base.OnMouseUp(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			switch (Appearance)
			{
				case Appearance.Normal:
					throw new NotImplementedException();

				case Appearance.Button:
					OnPaintButton(e);
					break;
			}

			base.OnPaint(e);
		}

		private void OnPaintButton(PaintEventArgs e)
		{
			switch (DetermineState(!MouseIsDown))
			{
				case CheckBoxState.UncheckedNormal:
					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(45, 45, 48)))
						e.Graphics.FillRectangle(brush, ClientRectangle);
					break;

				case CheckBoxState.UncheckedHot:
					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(62, 62, 64)))
						e.Graphics.FillRectangle(brush, ClientRectangle);
					break;

				case CheckBoxState.UncheckedPressed:
					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(0, 122, 204)))
						e.Graphics.FillRectangle(brush, ClientRectangle);
					break;

				case CheckBoxState.CheckedNormal:
					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(51, 153, 255)))
						e.Graphics.FillRectangle(brush, ClientRectangle);

					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(45, 45, 48)))
						e.Graphics.FillRectangle(brush, Rectangle.Inflate(ClientRectangle, -1, -1));
					break;

				case CheckBoxState.CheckedHot:
				case CheckBoxState.CheckedPressed:
					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(51, 153, 255)))
						e.Graphics.FillRectangle(brush, ClientRectangle);

					// OPTIMIZE
					using (var brush = new SolidBrush(Color.FromArgb(62, 62, 64)))
						e.Graphics.FillRectangle(brush, Rectangle.Inflate(ClientRectangle, -1, -1));
					break;
			}

			TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
		}

		private void PerformAutoUpdates()
		{
			if (!_autoCheck)
				return;

			if (Checked && Parent != null)
			{
				var radioButtons = Parent.Controls
					.Where(c => c != this && c is RadioButton)
					.Select(c => c as RadioButton)
					.Where(b => b.AutoCheck && b.Checked);
				foreach (var b in radioButtons)
				{
					var propDesc = TypeDescriptor.GetProperties(this)[nameof(Checked)];
					propDesc.SetValue(b, false);
				}
			}
		}
	}
}
