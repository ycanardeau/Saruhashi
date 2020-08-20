﻿using System;

namespace Aigamo.Saruhashi
{
	public class CheckBox : ButtonBase
	{
		private bool _checked;

		public CheckBox() : base()
		{
			SetStyle(ControlStyles.StandardClick, false);
		}

		public Appearance Appearance { get; set; }
		public bool AutoCheck { get; set; } = true;

		public bool Checked
		{
			get => _checked;
			set
			{
				if (value == _checked)
					return;

				_checked = value;
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

				if (!Enabled)
					return CheckBoxState.CheckedDisabled;

				return CheckBoxState.CheckedNormal;
			}
			else
			{
				if (!up)
					return CheckBoxState.UncheckedPressed;

				if (MouseIsOver)
					return CheckBoxState.UncheckedHot;

				if (!Enabled)
					return CheckBoxState.UncheckedDisabled;

				return CheckBoxState.UncheckedNormal;
			}
		}

		protected virtual void OnCheckedChanged(EventArgs e) => CheckedChanged?.Invoke(this, e);

		protected override void OnClick(EventArgs e)
		{
			if (AutoCheck)
				Checked = !Checked;

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
					PushButtonState pushButtonState = DetermineState(!MouseIsDown) switch
					{
						CheckBoxState.UncheckedNormal => PushButtonState.Normal,
						CheckBoxState.UncheckedHot => PushButtonState.Hot,
						CheckBoxState.UncheckedPressed => PushButtonState.Pressed,
						CheckBoxState.UncheckedDisabled => PushButtonState.Disabled,
						CheckBoxState.CheckedNormal => PushButtonState.Pressed,
						CheckBoxState.CheckedHot => PushButtonState.Pressed,
						CheckBoxState.CheckedPressed => PushButtonState.Pressed,
						CheckBoxState.CheckedDisabled => PushButtonState.Disabled,
						_ => 0,
					};
					ButtonRenderer.DrawButton(e.Graphics, ClientRectangle, Text, Font, focused: false, pushButtonState);
					break;
			}

			base.OnPaint(e);
		}
	}
}
