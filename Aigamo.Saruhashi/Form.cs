using System;

namespace Aigamo.Saruhashi
{
	public class Form : Control
	{
		public event EventHandler? Load;

		public Form() : base()
		{
			SetState(States.Visible, false);
		}

		public override BindingContext? BindingContext
		{
			get
			{
				var bm = base.BindingContext;
				if (bm is null)
				{
					bm = new BindingContext();
					BindingContext = bm;
				}

				return bm;
			}
			set => base.BindingContext = value;
		}

		public bool KeyPreview { get; set; }

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			OnLoad(EventArgs.Empty);
		}

		protected virtual void OnLoad(EventArgs e) => Load?.Invoke(this, e);
	}
}
