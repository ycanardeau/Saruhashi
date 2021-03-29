using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using DrawingColor = System.Drawing.Color;
using DrawingPoint = System.Drawing.Point;
using DrawingPointF = System.Drawing.PointF;
using DrawingRectangle = System.Drawing.Rectangle;
using DrawingRectangleF = System.Drawing.RectangleF;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using XnaRectangleF = MonoGame.Extended.RectangleF;

namespace Aigamo.Saruhashi.MonoGame
{
	public sealed class MonoGameGraphics : Graphics
	{
		private sealed class DisposableScissor : IDisposable
		{
			private readonly SpriteBatch _spriteBatch;
			private readonly XnaRectangle _scissorRectangle;

			public DisposableScissor(SpriteBatch spriteBatch, XnaRectangle scissorRectangle, ViewportAdapter? viewportAdapter = null)
			{
				_spriteBatch = spriteBatch;
				_scissorRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;

				if (viewportAdapter != null)
				{
					var scale = new Vector2(viewportAdapter.ViewportWidth, viewportAdapter.ViewportHeight) / new Vector2(viewportAdapter.VirtualWidth, viewportAdapter.ViewportHeight);
					scissorRectangle = new XnaRectangleF(scissorRectangle.Location.ToVector2() * scale, scissorRectangle.Size.ToVector2() * scale).ToRectangle();
				}
				_spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;
			}

			public void Dispose()
			{
				_spriteBatch.GraphicsDevice.ScissorRectangle = _scissorRectangle;
			}
		}

		private static readonly RasterizerState s_rasterizerState = new() { ScissorTestEnable = true };

		public SpriteBatch SpriteBatch { get; }
		public ViewportAdapter? ViewportAdapter { get; }

		private readonly DisposableScissor _scissor;

		private DisposableScissor CreateDisposableScissor() => new(SpriteBatch, DrawingRectangle.Round(ClipBounds).ToXnaRectangle(), ViewportAdapter);

		public MonoGameGraphics(Control control, SpriteBatch spriteBatch, ViewportAdapter? viewportAdapter = null) : base(control)
		{
			SpriteBatch = spriteBatch;
			ViewportAdapter = viewportAdapter;

			_scissor = CreateDisposableScissor();
			SpriteBatch.Begin(blendState: BlendState.AlphaBlend, rasterizerState: s_rasterizerState, transformMatrix: ViewportAdapter?.GetScaleMatrix());
		}

		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				SpriteBatch.End();
				_scissor.Dispose();
			}

			_disposed = true;

			base.Dispose(disposing);
		}

		public override void DrawImage(IImage image, DrawingPoint point) => DrawImage((MonoGameImage)image, point);
		public override void DrawImage(IImage image, DrawingPointF point) => DrawImage(image, DrawingPoint.Round(point));

		private void DrawImage(MonoGameImage image, DrawingPoint point)
		{
			SpriteBatch.Draw(image.Texture, Control.PointToScreen(point).ToXnaPoint().ToVector2(), image.Color);
		}

		public override void DrawLine(Pen pen, DrawingPointF point1, DrawingPointF point2) => DrawLine(pen, DrawingPoint.Round(point1), DrawingPoint.Round(point2));
		public override void DrawLine(Pen pen, int x1, int y1, int x2, int y2) => DrawLine(pen, new DrawingPoint(x1, y1), new DrawingPoint(x2, y2));
		public override void DrawLine(Pen pen, float x1, float y1, float x2, float y2) => DrawLine(pen, new DrawingPointF(x1, y1), new DrawingPointF(x2, y2));

		public override void DrawLine(Pen pen, DrawingPoint point1, DrawingPoint point2)
		{
			if (pen.Color == DrawingColor.Transparent)
				return;

			SpriteBatch.DrawLine(Control.PointToScreen(point1).ToXnaPoint().ToVector2(), Control.PointToScreen(point2).ToXnaPoint().ToVector2(), pen.Color.ToXnaColor(), pen.Width);
		}

		public override void DrawRectangle(Pen pen, DrawingRectangle rectangle)
		{
			if (pen.Color == DrawingColor.Transparent)
				return;

			SpriteBatch.DrawRectangle(Control.RectangleToScreen(rectangle).ToXnaRectangle(), pen.Color.ToXnaColor(), pen.Width);
		}

		public override void DrawRectangle(Pen pen, int x, int y, int width, int height) => DrawRectangle(pen, new DrawingRectangle(x, y, width, height));
		public override void DrawRectangle(Pen pen, float x, float y, float width, float height) => DrawRectangle(pen, DrawingRectangle.Round(new DrawingRectangleF(x, y, width, height)));

		public override void DrawString(string? text, IFont font, Brush brush, DrawingPointF point) => DrawString(text, (IMonoGameFont)font, brush, point);

		private void DrawString(string? text, IMonoGameFont font, Brush brush, DrawingPointF point)
		{
			if (string.IsNullOrEmpty(text))
				return;

			switch (brush)
			{
				case SolidBrush b:
					if (b.Color == DrawingColor.Transparent)
						return;

					font.Draw(SpriteBatch, text, Control.PointToScreen(DrawingPoint.Round(point)).ToXnaPoint().ToVector2(), b.Color.ToXnaColor());
					break;

				default:
					throw new NotImplementedException();
			}
		}

		public override void FillRectangle(Brush brush, DrawingRectangle rectangle)
		{
			switch (brush)
			{
				case SolidBrush b:
					if (b.Color == DrawingColor.Transparent)
						return;

					SpriteBatch.FillRectangle(Control.RectangleToScreen(rectangle).ToXnaRectangle(), b.Color.ToXnaColor());
					break;

				default:
					throw new NotImplementedException();
			}
		}

		public override void FillRectangle(Brush brush, DrawingRectangleF rectangle) => FillRectangle(brush, DrawingRectangle.Round(rectangle));
		public override void FillRectangle(Brush brush, int x, int y, int width, int height) => FillRectangle(brush, new DrawingRectangle(x, y, width, height));
		public override void FillRectangle(Brush brush, float x, float y, float width, float height) => FillRectangle(brush, new DrawingRectangleF(x, y, width, height));

		public override SizeF MeasureString(string? text, IFont font) => MeasureString(text, (IMonoGameFont)font);

		private SizeF MeasureString(string? text, IMonoGameFont font) => ((Size2)font.MeasureString(text)).ToDrawingSize();
	}
}
