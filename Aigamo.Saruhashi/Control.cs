using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Aigamo.Saruhashi
{
	public class Control : IDisposable
	{
		public sealed class ControlCollection : IEnumerable<Control>
		{
			private readonly Control _control;
			private readonly List<Control> _controls = new();

			public ControlCollection(Control control)
			{
				_control = control;
			}

			public void Add(Control item)
			{
				_controls.Add(item);
				item.Parent = _control;
			}

			public void Clear() => _controls.Clear();

			public IEnumerator<Control> GetEnumerator() => _controls.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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

		private ControlCollection? _controls;
		private ControlStyles _controlStyle;
		private bool _disposed;
		private IFont? _font;
		private States _state = States.Visible | States.Enabled;
		private WindowManager _windowManager = default!;

		public event EventHandler? Click;
		public event EventHandler? Disposed;
		public event EventHandler? EnabledChanged;
		public event EventHandler? GotFocus;
		public event EventHandler<KeyEventArgs>? KeyDown;
		public event EventHandler<KeyPressEventArgs>? KeyPress;
		public event EventHandler<KeyEventArgs>? KeyUp;
		public event EventHandler? LostFocus;
		public event EventHandler? MouseCaptureChanged;
		public event EventHandler<MouseEventArgs>? MouseClick;
		public event EventHandler<MouseEventArgs>? MouseDown;
		public event EventHandler? MouseEnter;
		public event EventHandler? MouseLeave;
		public event EventHandler<MouseEventArgs>? MouseMove;
		public event EventHandler<MouseEventArgs>? MouseUp;
		public event EventHandler<PaintEventArgs>? Paint;
		public event EventHandler<PaintEventArgs>? PaintBackground;
		public event EventHandler? ParentChanged;
		public event EventHandler? VisibleChanged;

		public Control()
		{
			GetText = () => Text;
			IsEnabled = () => Enabled;
			IsVisible = () => Visible;
			Size = DefaultSize;

			SetStyle(ControlStyles.StandardClick, true);
		}

		internal Control(WindowManager windowManager) : this()
		{
			_windowManager = windowManager;
		}

		~Control() => Dispose(false);

		public Color BackColor { get; set; } = Color.FromArgb(37, 37, 38);
		public Rectangle Bounds { get; set; }
		public bool CanFocus => IsVisible() && IsEnabled();

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
		protected virtual Size DefaultSize => Size.Empty;
		internal bool DesiredVisibility => GetState(States.Visible);

		public bool Enabled
		{
			get
			{
				if (!GetState(States.Enabled))
					return false;

				return Parent?.IsEnabled() ?? true;
			}
			set
			{
				var oldValue = Enabled;
				SetState(States.Enabled, value);

				if (oldValue != value)
					OnEnabledChanged(EventArgs.Empty);
			}
		}

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

		public Color ForeColor { get; set; } = Color.FromArgb(241, 241, 241);
		private protected Func<string> GetText { get; set; }

		public int Height
		{
			get => Size.Height;
			set => Size = new Size(Width, value);
		}

		private protected Func<bool> IsEnabled { get; set; }
		private protected Func<bool> IsVisible { get; set; }

		public Point Location
		{
			get => Bounds.Location;
			set => Bounds = new Rectangle(value, Size);
		}

		public string Name { get; set; } = string.Empty;
		public Control? Parent { get; private set; }
		private Point ScreenLocation => (Parent?.ScreenLocation ?? Point.Empty) + (Size)Location;
		private Rectangle ScreenRectangle => new Rectangle(ScreenLocation, Size);

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

				return Parent?.IsVisible() ?? true;
			}
			set => SetVisibleCore(value);
		}

		public int Width
		{
			get => Size.Width;
			set => Size = new Size(value, Height);
		}

		/// <remarks>
		/// The WindowManager property is available only after the control is added to a window manager.
		/// This is because it's possible for an application to have more than one window manager.
		/// </remarks>
		public WindowManager WindowManager
		{
			get
			{
				if (_windowManager != null)
					return _windowManager;

				return _windowManager = Parent?.WindowManager ?? throw new InvalidOperationException();
			}
		}

		public int X
		{
			get => Location.X;
			set => Location = new Point(value, Y);
		}

		public int Y
		{
			get => Location.Y;
			set => Location = new Point(X, value);
		}

		public void CreateControl()
		{
			CreateControl(false);
		}

		internal void CreateControl(bool fIgnoreVisible)
		{
			var ready = !Created && IsVisible();

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
			if (!IsVisible())
				return;

			using (var graphics = CreateGraphics())
			{
				var e = new PaintEventArgs(graphics, RectangleToClient(ClipRectangle));
				OnPaintBackground(e);
				OnPaint(e);
			}

			foreach (var c in Controls.AsEnumerable().Reverse())
				c.Draw();
		}

		public bool Focus()
		{
			if (CanFocus)
			{
				var focus = WindowManager.GetFocus();

				if (focus != this)
				{
					focus?.OnLostFocus(EventArgs.Empty);
					WindowManager.SetFocus(this);
					OnGotFocus(EventArgs.Empty);
				}
			}

			return Focused;
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
			if (!IsVisible() || !IsEnabled())
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
			if (!IsVisible() || !IsEnabled())
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

		internal Control? HandleKeyUp(KeyEventArgs e)
		{
			if (!IsVisible() || !IsEnabled())
				return null;

			foreach (var c in Controls)
			{
				var handler = c.HandleKeyUp(e);
				if (handler != null)
					return handler;
			}

			if (Focused)
			{
				OnKeyUp(e);
				return this;
			}

			return null;
		}

		internal Control? HandleMouseDown(MouseEventArgs e)
		{
			if (!IsVisible() || !IsEnabled())
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
			if (!IsVisible() || !IsEnabled())
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
			if (!IsVisible() || !IsEnabled())
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

		protected virtual void OnEnabledChanged(EventArgs e) => EnabledChanged?.Invoke(this, e);

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

		protected virtual void OnKeyUp(KeyEventArgs e)
		{
			var parent = Parent;
			while (parent != null)
			{
				if (parent is Form form && form.KeyPreview)
					form.OnKeyUp(e);

				parent = parent.Parent;
			}

			KeyUp?.Invoke(this, e);
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
		protected virtual void OnPaint(PaintEventArgs e) => Paint?.Invoke(this, e);

		protected virtual void OnPaintBackground(PaintEventArgs e)
		{
			// OPTIMIZE
			using (var brush = new SolidBrush(BackColor))
				e.Graphics.FillRectangle(brush, ClientRectangle);

			PaintBackground?.Invoke(this, e);
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

		internal Control? WindowFromPoint(Point point)
		{
			if (!IsVisible())
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
