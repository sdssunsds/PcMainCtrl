using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Basler
{
    public class ImageAnnotation
	{
		public enum AnnotationType
		{
			Point,
			Line,
			Rectangle,
			Ellipse,	
			TextOnly
		}

		public enum AnnotationMarkerType
		{
			Circle,
			Cross,
			Diamond,
			Dot,
			Plus,
			Rectangle,
			Triangle
		}

		public enum ArrowType
		{
			None,
			Begin,
			End,
			Both
		}

		public enum AnnotationPositionType
		{
			Relative,
			Absolute
		}

		AnnotationType _type = AnnotationType.TextOnly;
		public AnnotationPositionType _positionType = AnnotationPositionType.Relative;
		List<PointF> _points = new List<PointF>();
		PointF _centre;
		float _width;
		float _height;
		float _rotation;
		Color _color = Color.Red;
		Color _fillColor = Color.Transparent;
		int _lineWidth = 1;
		int _markerSize = 0;

		AnnotationMarkerType _markerType = AnnotationMarkerType.Plus;
		ArrowType _lineArrowType = ArrowType.None;

		PointF _textPoint;
		Color _textBlockOutline = Color.Black;
		Color _textBlockFill = Color.FromArgb(192, Color.White);
		Font _textFont = new System.Drawing.Font("Segeo UI", 9);

		public bool IsSelected = false;
		public bool IsHovered = false;

		public string Name { get; set; }

		public Color Color
		{
			get { return _color; }
			set { _color = value; }
		}

		public int LineWidth
		{
			get { return _lineWidth; }
			set { _lineWidth = value; }
		}

		public AnnotationType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		public string Text { get; set; }

		public object Tag { get; set; }

		public PointF Centre
		{
			get { return _centre; }
			set { _centre = value; }
		}

		public float Width
		{
			get { return _width; }
			set { _width = value; }
		}

		public float Height
		{
			get { return _height; }
			set { _height = value; }
		}

		public List<PointF> Points
		{
			get { return _points; }
			set { _points = value; }
		}

		public PointF TextPoint
		{
			get { return _textPoint; }
			set { _textPoint = value; }
		}

		public int MarkerSize
		{
			get { return _markerSize; }
			set { _markerSize = value; }
		}

		public AnnotationMarkerType MarkerType
		{
			get { return _markerType; }
			set { _markerType = value; }
		}

		public ArrowType LineArrowType
		{
			get { return _lineArrowType; }
			set { _lineArrowType = value; }
		}

		public Color TextBlockOutline
		{
			get { return _textBlockOutline; }
			set { _textBlockOutline = value; }
		}

		public Color TextBlockFill
		{
			get { return _textBlockFill; }
			set { _textBlockFill = value; }
		}

		public Font TextFont
		{
			get { return _textFont; }
			set { _textFont = value; }
		}

		public float Rotation
		{
			get { return _rotation; }
			set { _rotation = value; }
		}

		public AnnotationPositionType PositionType { get => _positionType; set => _positionType = value; }
		public Color FillColor { get => _fillColor; set => _fillColor = value; }

		public ImageAnnotation()
		{
		}

		public ImageAnnotation(Point p)
		{
			_type = AnnotationType.Point;
			_centre = p;
			_textPoint = _centre;
			_markerSize = 5;
		}

		public ImageAnnotation(PointF p)
		{
			_type = AnnotationType.Point;
			_centre = p;
			_textPoint = _centre;
			_markerSize = 5;
		}

		public ImageAnnotation(PointF p1, PointF p2)
		{
			_type = AnnotationType.Line;
			_points.Add(p1);
			_points.Add(p2);
			_centre = new PointF((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
			_textPoint = p2;
		}

		public ImageAnnotation(Point p1, Point p2)
		{
			_type = AnnotationType.Line;
			_points.Add((PointF)p1);
			_points.Add((PointF)p2);
			_centre = new PointF((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
			_textPoint = p2;
		}

		public ImageAnnotation(RectangleF rect)
		{
			_type = AnnotationType.Rectangle;
			_width = rect.Width;
			_height = rect.Height;
			_points.Add(new PointF(rect.X, rect.Y));
			_centre = new PointF(rect.X + _width / 2, rect.Y + _height / 2);
			_textPoint = _centre;
		}

		public ImageAnnotation(Rectangle rect)
		{
			_type = AnnotationType.Rectangle;
			_width = rect.Width;
			_height = rect.Height;
			_points.Add(new PointF(rect.X, rect.Y));
			_centre = new PointF(rect.X + _width / 2, rect.Y + _height / 2);
			_textPoint = _centre;
		}

		public ImageAnnotation(Point centre, int radius)
		{
			_type = AnnotationType.Ellipse;
			_centre = centre;
			_textPoint = centre;
			_points.Add(new Point(centre.X - radius, centre.Y - radius));
			_width = radius * 2;
			_height = radius * 2;
			_markerSize = 5;
		}

		public ImageAnnotation(PointF centre, float radius)
		{
			_type = AnnotationType.Ellipse;
			_centre = centre;
			_textPoint = centre;
			_points.Add(new PointF(centre.X - radius, centre.Y - radius));
			_width = radius * 2;
			_height = radius * 2;
			_markerSize = 5;
		}

		public ImageAnnotation(Point centre, int radiusX, int radiusY)
		{
			_type = AnnotationType.Ellipse;
			_centre = centre;
			_textPoint = centre;
			_points.Add(new Point(centre.X - radiusX, centre.Y - radiusY));
			_width = radiusX * 2;
			_height = radiusY * 2;
			_markerSize = 5;
		}

		public ImageAnnotation(PointF centre, float radiusX, float radiusY)
		{
			_type = AnnotationType.Ellipse;
			_centre = centre;
			_textPoint = centre;
			_points.Add(new PointF(centre.X - radiusX, centre.Y - radiusY));
			_width = radiusX * 2;
			_height = radiusY * 2;
			_markerSize = 5;
		}

		public ImageAnnotation(PointF centre, float width, float height, float rotation, bool isEllipse = true)
		{
			if (isEllipse)
				_type = AnnotationType.Ellipse;
			else
				_type = AnnotationType.Rectangle;
			_centre = centre;
			_textPoint = centre;
			_points.Add(new PointF(centre.X - width / 2, centre.Y - height / 2));
			_width = width;
			_height = height;
			_rotation = rotation;
			_markerSize = 5;
		}

		public ImageAnnotation(List<Point> points, bool isClosed = false)
		{
			_type = AnnotationType.Line;
			_centre = new Point((int)points.Average(x => x.X), (int)points.Average(x => x.Y));
			_points = new List<PointF>(points.Select(p => new PointF(Convert.ToInt32(p.X), Convert.ToInt32(p.Y))));
			if (isClosed)
				_points.Add(points[0]);
			_width = points.Max(x => x.X) - points.Min(x => x.X);
			_height = points.Max(x => x.Y) - points.Min(x => x.Y);
		}

		public ImageAnnotation(List<PointF> points, bool isClosed = false)
		{
			_type = AnnotationType.Line;
			_centre = new Point((int)points.Average(x => x.X), (int)points.Average(x => x.Y));
			_points = new List<PointF>(points);
			if (isClosed)
				_points.Add(points[0]);
			_width = points.Max(x => x.X) - points.Min(x => x.X);
			_height = points.Max(x => x.Y) - points.Min(x => x.Y);
		}

		public ImageAnnotation(Point point, string text)
		{
			_type = AnnotationType.TextOnly;
			this.Text = text;
			_textPoint = point;
			_textBlockFill = Color.Transparent;
			_textBlockOutline = Color.Transparent;
		}
	}
}
