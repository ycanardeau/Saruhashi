using System;
using System.Drawing;

namespace Aigamo.Saruhashi
{
	public class Form : Control
	{
		public event EventHandler? Load;

		public Form() : base()
		{
			SetState(States.Visible, false);
		}

		protected override Size DefaultSize => new Size(300, 300);
		public DialogResult DialogResult { get; set; }
		public bool KeyPreview { get; set; }

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			OnLoad(EventArgs.Empty);
		}

		protected virtual void OnLoad(EventArgs e) => Load?.Invoke(this, e);
	}
}
