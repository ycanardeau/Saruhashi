using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace Aigamo.Saruhashi
{
	public class Control : IDisposable, IBindableComponent
	{
		public class ControlCollection : IEnumerable<Control>
		{
			public ControlCollection(Control owner)
			{
				Owner = owner;
			}

			private protected List<Control> InnerList { get; } = new();

			public virtual void Add(Control? value)
			{
				if (value is null)
					return;

				if (value._parent != null)
					value._parent.Controls.Remove(value);

				InnerList.Add(value);

				try
				{
					var oldParent = value._parent;
					try
					{
						value.AssignParent(Owner);
					}
					finally
					{
						if (oldParent != value._parent && Owner.Created)
						{
							if (value.Visible)
								value.CreateControl();
						}
					}
				}
				finally
				{
					// TODO
				}
			}

			public virtual void Remove(Control? value)
			{
				if (value is null)
					return;

				if (value.Parent == Owner)
				{
					InnerList.Remove(value);
					value.AssignParent(null);
				}
			}

			public IEnumerator<Control> GetEnumerator() => InnerList.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			public Control Owner { get; }
		}

		[Flags]
		private protected enum States
		{
			Created = 0x00000001,
			Visible = 0x00000002,
			Enabled = 0x00000004,
			TabStop = 0x00000008,
			Recreate = 0x00000010,
			Modal = 0x00000020,
			AllowDrop = 0x00000040,
			DropTarget = 0x00000080,
			NoZOrder = 0x00000100,
			LayoutDeferred = 0x00000200,
			UseWaitCursor = 0x00000400,
			Disposed = 0x00000800,
			Disposing = 0x00001000,
			MouseEnterPending = 0x00002000,
			TrackingMouseEvent = 0x00004000,
			ThreadMarshalPending = 0x00008000,
			SizeLockedByOS = 0x00010000,
			CausesValidation = 0x00020000,
			CreatingHandle = 0x00040000,
			TopLevel = 0x00080000,
			IsAccessible = 0x00100000,
			OwnCtlBrush = 0x00200000,
			ExceptionWhilePainting = 0x00400000,
			LayoutIsDirty = 0x00800000,
			CheckedHost = 0x01000000,
			HostedInDialog = 0x02000000,
			DoubleClickFired = 0x04000000,
			MousePressed = 0x08000000,
			ValidationCancelled = 0x10000000,
			ParentRecreating = 0x20000000,
			Mirrored = 0x40000000,
		}

		private ControlBindingsCollection? _bindings;
		private BindingContext? _context;
		private ControlCollection? _controls;
		private ControlStyles _controlStyle;
		private bool _disposed;
		private IFont? _font;
		private Control? _parent;
		private States _state = States.Visible | States.Enabled;
		private WindowManager _windowManager = default!;

		public event EventHandler? BindingContextChanged;
		public event EventHandler? Click;
		public event EventHandler? Disposed;
		public event EventHandler? GotFocus;
		public event EventHandler<KeyEventArgs>? KeyDown;
		public event EventHandler<KeyPressEventArgs>? KeyPress;
		public event EventHandler? LostFocus;
		public event EventHandler? MouseCaptureChanged;
		public event EventHandler<MouseEventArgs>? MouseClick;
		public event EventHandler<MouseEventArgs>? MouseDown;
		public event EventHandler? MouseEnter;
		public event EventHandler? MouseLeave;
		public event EventHandler<MouseEventArgs>? MouseMove;
		public event EventHandler<MouseEventArgs>? MouseUp;
		public event EventHandler<PaintEventArgs>? Paint;
		public event EventHandler? ParentChanged;
		public event EventHandler? VisibleChanged;

		public Control()
		{
			SetStyle(ControlStyles.StandardClick, true);
		}

		internal Control(WindowManager windowManager) : this()
		{
			_windowManager = windowManager;
		}

		~Control() => Dispose(false);

		public Color BackColor { get; set; } = Color.FromArgb(37, 37, 38);

		public virtual BindingContext? BindingContext
		{
			get
			{
				var context = _context;
				if (context != null)
					return context;

				var p = Parent;
				if (p != null && p.CanAccessProperties)
					return p.BindingContext;

				return null;
			}
			set
			{
				var oldContext = _context;
				var newContext = value;

				if (oldContext != newContext)
				{
					_context = newContext;

					OnBindingContextChanged(EventArgs.Empty);
				}
			}
		}

		public Rectangle Bounds { get; set; }
		internal virtual bool CanAccessProperties => true;

		public bool Capture
		{
			get => WindowManager.Capture == this;
			set
			{
				WindowManager.Capture = value ? this : null;

				if (!value)
					OnMouseCaptureChanged(EventArgs.Empty);
			}
		}

		// TODO
		public Rectangle ClientRectangle => new Rectangle(Point.Empty, Size);
		internal Rectangle ClipRectangle => GetClipRectangle(ScreenRectangle);
		public ControlCollection Controls => _controls ??= new ControlCollection(this);
		public bool Created => _state.HasFlag(States.Created);
		public ControlBindingsCollection DataBindings => _bindings ??= new ControlBindingsCollection(this);
		internal bool DesiredVisibility => GetState(States.Visible);
		public bool Focused => WindowManager.GetFocus() == this;

		public IFont? Font
		{
			get
			{
				if (_font != null)
					return _font;

				return _font = Parent?.Font ?? WindowManager.DefaultFont;
			}
			set => _font = value;
		}

		public Color ForeColor { get; set; } = Color.White;

		public Point Location
		{
			get => Bounds.Location;
			set => Bounds = new Rectangle(value, Size);
		}

		public string Name { get; set; } = string.Empty;

		public Control? Parent
		{
			get => _parent;
			set
			{
				if (_parent != value)
				{
					if (value != null)
						value.Controls.Add(this);
					else
						_parent?.Controls.Remove(this);
				}
			}
		}

		private Point ScreenLocation => (Parent?.ScreenLocation ?? Point.Empty) + (Size)Location;
		private Rectangle ScreenRectangle => new Rectangle(ScreenLocation, Size);
		public ISite? Site { get; set; }

		public Size Size
		{
			get => Bounds.Size;
			set => Bounds = new Rectangle(Location, value);
		}

		public string Text { get; set; } = string.Empty;

		public bool Visible
		{
			get
			{
				if (!DesiredVisibility)
					return false;

				return Parent?.Visible ?? true;
			}
			set => SetVisibleCore(value);
		}

		public WindowManager WindowManager
		{
			get
			{
				if (_windowManager != null)
					return _windowManager;

				return _windowManager = Parent?.WindowManager ?? throw new InvalidOperationException();
			}
		}

		internal virtual void AssignParent(Control? value)
		{
			if (CanAccessProperties)
			{
				_parent = value;
				OnParentChanged(EventArgs.Empty);

				if (_context is null && Created)
					OnBindingContextChanged(EventArgs.Empty);
			}
			else
			{
				_parent = value;
				OnParentChanged(EventArgs.Empty);
			}
		}

		public void CreateControl()
		{
			var controlIsAlreadyCreated = Created;
			CreateControl(false);

			if (_context is null && Parent != null && !controlIsAlreadyCreated)
				OnBindingContextChanged(EventArgs.Empty);
		}

		internal void CreateControl(bool fIgnoreVisible)
		{
			var ready = !Created && Visible;

			if (ready || fIgnoreVisible)
			{
				_state |= States.Created;
				try
				{
					foreach (var c in Controls)
						c.CreateControl(fIgnoreVisible);
				}
				finally
				{
					// TODO
				}
				OnCreateControl();
			}
		}

		public Graphics CreateGraphics() => WindowManager.GraphicsFactory.Create(this);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;
		}

		internal void Draw()
		{
			if (!Visible)
				return;

			using (var graphics = CreateGraphics())
				OnPaint(new PaintEventArgs(graphics, RectangleToClient(ClipRectangle)));

			foreach (var c in Controls.AsEnumerable().Reverse())
				c.Draw();
		}

		public bool Focus()
		{
			var focus = WindowManager.GetFocus();

			if (focus != this)
				focus?.OnLostFocus(EventArgs.Empty);

			var ret = WindowManager.SetFocus(this);

			if (focus != this)
				OnGotFocus(EventArgs.Empty);

			return ret;
		}

		internal Rectangle GetClipRectangle(Rectangle rectangle)
		{
			var ret = Rectangle.Intersect(rectangle, ScreenRectangle);
			if (ret.IsEmpty)
				return ret;

			return Parent?.GetClipRectangle(ret) ?? ret;
		}

		private protected bool GetState(States flag) => _state.HasFlag(flag);

		protected bool GetStyle(ControlStyles flag) => _controlStyle.HasFlag(flag);

		internal Control? HandleKeyDown(KeyEventArgs e)
		{
			if (!Visible)
				return null;

			foreach (var c in Controls)
			{
				var handler = c.HandleKeyDown(e);
				if (handler != null)
					return handler;
			}

			if (Focused)
			{
				OnKeyDown(e);
				return this;
			}

			return null;
		}

		internal Control? HandleKeyPress(KeyPressEventArgs e)
		{
			if (!Visible)
				return null;

			foreach (var c in Controls)
			{
				var handler = c.HandleKeyPress(e);
				if (handler != null)
					return handler;
			}

			if (Focused)
			{
				OnKeyPress(e);
				return this;
			}

			return null;
		}

		internal Control? HandleMouseDown(MouseEventArgs e)
		{
			if (!Visible)
				return null;

			foreach (var c in Controls)
			{
				var handler = c.HandleMouseDown(e);
				if (handler != null)
					return handler;
			}

			if (ClipRectangle.Contains(e.Location))
			{
				Capture = true;
				OnMouseDown(new MouseEventArgs(e.Button, e.Clicks, PointToClient(e.Location), e.Delta));
				return this;
			}

			return null;
		}

		internal Control? HandleMouseMove(MouseEventArgs e)
		{
			if (!Visible)
				return null;

			if (!Capture)
			{
				foreach (var c in Controls)
				{
					var handler = c.HandleMouseMove(e);
					if (handler != null)
						return handler;
				}
			}

			if (Capture || ClipRectangle.Contains(e.Location))
			{
				WindowManager.MouseOver = this;
				if (WindowManager.MouseOver != WindowManager.PreviousMouseOver)
				{
					WindowManager.PreviousMouseOver?.OnMouseLeave(EventArgs.Empty);
					WindowManager.MouseOver?.OnMouseEnter(EventArgs.Empty);
				}
				WindowManager.PreviousMouseOver = WindowManager.MouseOver;

				OnMouseMove(new MouseEventArgs(e.Button, e.Clicks, PointToClient(e.Location), e.Delta));
				return this;
			}

			return null;
		}

		internal Control? HandleMouseUp(MouseEventArgs e)
		{
			if (!Visible)
				return null;

			if (!Capture)
			{
				foreach (var c in Controls)
				{
					var handler = c.HandleMouseUp(e);
					if (handler != null)
						return handler;
				}
			}

			if (Capture || ClipRectangle.Contains(e.Location))
			{
				if (GetStyle(ControlStyles.StandardClick))
				{
					if (Capture && WindowManager.WindowFromPoint(e.Location) == this)
					{
						OnClick(EventArgs.Empty);
						OnMouseClick(new MouseEventArgs(e.Button, e.Clicks, PointToClient(e.Location), e.Delta));
					}
				}

				OnMouseUp(new MouseEventArgs(e.Button, e.Clicks, PointToClient(e.Location), e.Delta));

				Capture = false;
				return this;
			}

			return null;
		}

		protected virtual void OnBindingContextChanged(EventArgs e)
		{
			if (_bindings != null)
				UpdateBindings();

			BindingContextChanged?.Invoke(this, e);

			foreach (var c in Controls)
				c.OnParentBindingContextChanged(e);
		}

		protected virtual void OnClick(EventArgs e) => Click?.Invoke(this, e);
		protected virtual void OnCreateControl() { }
		protected virtual void OnGotFocus(EventArgs e) => GotFocus?.Invoke(this, e);

		protected virtual void OnKeyDown(KeyEventArgs e)
		{
			var parent = Parent;
			while (parent != null)
			{
				if (parent is Form form && form.KeyPreview)
					form.OnKeyDown(e);

				parent = parent.Parent;
			}

			KeyDown?.Invoke(this, e);
		}

		protected virtual void OnKeyPress(KeyPressEventArgs e)
		{
			var parent = Parent;
			while (parent != null)
			{
				if (parent is Form form && form.KeyPreview)
					form.OnKeyPress(e);

				parent = parent.Parent;
			}

			KeyPress?.Invoke(this, e);
		}

		protected virtual void OnLostFocus(EventArgs e) => LostFocus?.Invoke(this, e);
		protected virtual void OnMouseCaptureChanged(EventArgs e) => MouseCaptureChanged?.Invoke(this, e);
		protected virtual void OnMouseClick(MouseEventArgs e) => MouseClick?.Invoke(this, e);

		protected virtual void OnMouseDown(MouseEventArgs e)
		{
			Focus();

			MouseDown?.Invoke(this, e);
		}

		protected virtual void OnMouseEnter(EventArgs e) => MouseEnter?.Invoke(this, e);
		protected virtual void OnMouseLeave(EventArgs e) => MouseLeave?.Invoke(this, e);
		protected virtual void OnMouseMove(MouseEventArgs e) => MouseMove?.Invoke(this, e);
		protected virtual void OnMouseUp(MouseEventArgs e) => MouseUp?.Invoke(this, e);

		protected virtual void OnPaint(PaintEventArgs e)
		{
			// OPTIMIZE
			using (var brush = new SolidBrush(BackColor))
				e.Graphics.FillRectangle(brush, ClientRectangle);

			Paint?.Invoke(this, e);
		}

		protected virtual void OnParentBindingContextChanged(EventArgs e)
		{
			if (_context is null)
				OnBindingContextChanged(e);
		}

		protected virtual void OnParentChanged(EventArgs e) => ParentChanged?.Invoke(this, e);

		protected virtual void OnParentVisibleChanged(EventArgs e)
		{
			if (DesiredVisibility)
				OnVisibleChanged(e);
		}

		protected virtual void OnVisibleChanged(EventArgs e)
		{
			var visible = Visible;

			if (Parent != null && visible && !Created)
				CreateControl();

			VisibleChanged?.Invoke(this, e);

			foreach (var c in Controls)
			{
				if (c.Visible)
					c.OnParentVisibleChanged(e);
			}
		}

		public Point PointToClient(Point p) => p - (Size)ScreenLocation;
		public Point PointToScreen(Point p) => ScreenLocation + (Size)p;

		public Rectangle RectangleToClient(Rectangle r) => new Rectangle(PointToClient(r.Location), r.Size);
		public Rectangle RectangleToScreen(Rectangle r) => new Rectangle(PointToScreen(r.Location), r.Size);

		private protected void SetState(States flag, bool value) => _state = value ? (_state | flag) : (_state & ~flag);

		protected void SetStyle(ControlStyles flag, bool value) => _controlStyle = value ? (_controlStyle | flag) : (_controlStyle & ~flag);

		protected virtual void SetVisibleCore(bool value)
		{
			if (value == Visible)
				return;

			SetState(States.Visible, value);
			OnVisibleChanged(EventArgs.Empty);
		}

		public void Show() => Visible = true;

		private void UpdateBindings()
		{
			foreach (var b in DataBindings.Cast<Binding>())
				BindingContext.UpdateBinding(BindingContext, b);
		}

		internal Control? WindowFromPoint(Point point)
		{
			if (!Visible)
				return null;

			foreach (var c in Controls)
			{
				var handler = c.WindowFromPoint(point);
				if (handler != null)
					return handler;
			}

			return ClipRectangle.Contains(point) ? this : null;
		}
	}
}
