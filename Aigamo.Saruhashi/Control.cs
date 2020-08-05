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

		private ControlStyles _controlStyle;
		private bool _disposed;
		private IFont? _font;
		private WindowManager _windowManager = default!;

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

		public Control()
		{
			Controls = new ControlCollection(this);
			IsVisible = () => Visible;

			SetStyle(ControlStyles.StandardClick, true);
		}

		internal Control(WindowManager windowManager) : this()
		{
			_windowManager = windowManager;
		}

		~Control() => Dispose(false);

		public Color BackColor { get; set; } = Color.FromArgb(37, 37, 38);
		public Rectangle Bounds { get; set; }

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
		public ControlCollection Controls { get; }
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
		public Func<bool> IsVisible { get; set; }

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
		public bool Visible { get; set; } = true;

		public WindowManager WindowManager
		{
			get
			{
				if (_windowManager != null)
					return _windowManager;

				return _windowManager = Parent?.WindowManager ?? throw new InvalidOperationException();
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

		protected bool GetStyle(ControlStyles flag) => _controlStyle.HasFlag(flag);

		internal Control? HandleKeyDown(KeyEventArgs e)
		{
			if (!IsVisible())
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
			if (!IsVisible())
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
			if (!IsVisible())
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
			if (!IsVisible())
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
			if (!IsVisible())
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
						OnMouseClick(new MouseEventArgs(e.Button, e.Clicks, PointToClient(e.Location), e.Delta));
				}

				OnMouseUp(new MouseEventArgs(e.Button, e.Clicks, PointToClient(e.Location), e.Delta));

				Capture = false;
				return this;
			}

			return null;
		}

		protected virtual void OnGotFocus(EventArgs e) => GotFocus?.Invoke(this, e);
		protected virtual void OnKeyDown(KeyEventArgs e) => KeyDown?.Invoke(this, e);
		protected virtual void OnKeyPress(KeyPressEventArgs e) => KeyPress?.Invoke(this, e);
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

		public Point PointToClient(Point p) => p - (Size)ScreenLocation;
		public Point PointToScreen(Point p) => ScreenLocation + (Size)p;

		public Rectangle RectangleToClient(Rectangle r) => new Rectangle(PointToClient(r.Location), r.Size);
		public Rectangle RectangleToScreen(Rectangle r) => new Rectangle(PointToScreen(r.Location), r.Size);

		protected void SetStyle(ControlStyles flag, bool value) => _controlStyle = value ? (_controlStyle | flag) : (_controlStyle & ~flag);

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
