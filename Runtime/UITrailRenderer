using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace mitaywalle
{
	[ExecuteAlways]
	[RequireComponent(typeof(CanvasRenderer))]
	public class UITrailRenderer : MaskableGraphic
	{
		public enum TrailUVMode
		{
			Stretch,
			RepeatBySegment,
			RepeatByDistance
		}

		[SerializeField] private int maxSegments = 250;
		[SerializeField] private Texture trailTexture;

		public Transform Target;
		public float MinDistance = 0.05f;
		public float Width = 10f;
		public float TrailLifetime = 1.5f;
		public Gradient LifetimeGradient = CreateLifetimeGradient();
		public Gradient TrailGradient = CreateTrailGradient();
		public TrailUVMode UVMode = TrailUVMode.Stretch;
		public Vector2 UVTiling = Vector2.one;
		public Vector2 UVOffset = Vector2.zero;
		public Vector3 WorldOffset;
		public Vector3 LocalOffset;

		private readonly List<Vector3> points = new();
		private readonly List<float> times = new();
		private readonly List<float> distances = new();
		private Vector3 lastPoint;
		private bool hasLastPoint;

		public Texture TrailTexture
		{
			get => trailTexture;
			set
			{
				if (trailTexture == value)
					return;

				trailTexture = value;
				SetMaterialDirty();
				SetVerticesDirty();
			}
		}

		public override Texture mainTexture => trailTexture != null ? trailTexture : base.mainTexture;

		protected override void OnEnable()
		{
			base.OnEnable();
			hasLastPoint = false;
			EnsureGradients();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			points.Clear();
			times.Clear();
			distances.Clear();
			hasLastPoint = false;
			SetVerticesDirty();
		}

		private void OnValidate()
		{
			if (maxSegments < 1)
				maxSegments = 1;

			if (MinDistance < 0f)
				MinDistance = 0f;

			if (Width < 0f)
				Width = 0f;

			if (TrailLifetime < 0f)
				TrailLifetime = 0f;

			EnsureGradients();
			SetMaterialDirty();
			SetVerticesDirty();
		}

		private void LateUpdate()
		{
			Transform target = Target != null ? Target : transform;
			Vector3 worldPosition = target.TransformPoint(LocalOffset) + WorldOffset;
			Vector3 worldPoint = worldPosition;

			if (!hasLastPoint)
			{
				AddPoint(worldPoint);
			}
			else if ((worldPoint - lastPoint).sqrMagnitude >= MinDistance * MinDistance)
			{
				AddPoint(worldPoint);
			}

			RemoveOldPoints();
			SetVerticesDirty();
		}

		private void AddPoint(Vector3 point)
		{
			if (points.Count >= maxSegments + 1)
			{
				points.RemoveAt(0);
				times.RemoveAt(0);
				distances.RemoveAt(0);
			}

			float distance = 0f;
			if (points.Count > 0)
				distance = distances[points.Count - 1] + Vector3.Distance(points[points.Count - 1], point);

			points.Add(point);
			times.Add(Time.unscaledTime);
			distances.Add(distance);
			lastPoint = point;
			hasLastPoint = true;
		}

		private void RemoveOldPoints()
		{
			if (TrailLifetime <= 0f)
			{
				points.Clear();
				times.Clear();
				distances.Clear();
				hasLastPoint = false;
				return;
			}

			float now = Time.unscaledTime;
			int removeCount = 0;
			for (int i = 0; i < times.Count; i++)
			{
				if (now - times[i] > TrailLifetime)
					removeCount++;
				else
					break;
			}

			if (removeCount <= 0)
				return;

			points.RemoveRange(0, removeCount);
			times.RemoveRange(0, removeCount);
			distances.RemoveRange(0, removeCount);

			if (distances.Count > 0)
			{
				float offset = distances[0];
				for (int i = 0; i < distances.Count; i++)
					distances[i] -= offset;
			}

			if (points.Count == 0)
				hasLastPoint = false;
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			if (points.Count < 2 || Width <= 0f)
				return;

			float now = Time.unscaledTime;
			float halfWidth = Width * 0.5f;
			int count = points.Count;
			Vector2[] localPoints = new Vector2[count];
			float totalDistance = distances[count - 1] > 0f ? distances[count - 1] : 1f;

			for (int i = 0; i < count; i++)
				localPoints[i] = rectTransform.InverseTransformPoint(points[i]);

			for (int i = 0; i < count; i++)
			{
				Vector2 normal = GetPointNormal(localPoints, i);
				Vector2 p = localPoints[i];
				float trailT = count > 1 ? (float)i / (count - 1) : 0f;
				float age01 = TrailLifetime > 0f ? Mathf.Clamp01((now - times[i]) / TrailLifetime) : 1f;
				Color gradientColor = TrailGradient.Evaluate(trailT) * LifetimeGradient.Evaluate(age01);
				Color32 c = MultiplyColor(color, gradientColor);
				float u = GetU(i, totalDistance);
				float v0 = UVOffset.y;
				float v1 = UVTiling.y + UVOffset.y;

				UIVertex left = UIVertex.simpleVert;
				left.color = c;
				left.position = p - normal * halfWidth;
				left.uv0 = new Vector2(u, v0);

				UIVertex right = UIVertex.simpleVert;
				right.color = c;
				right.position = p + normal * halfWidth;
				right.uv0 = new Vector2(u, v1);

				vh.AddVert(left);
				vh.AddVert(right);
			}

			for (int i = 0; i < count - 1; i++)
			{
				int vi = i * 2;
				vh.AddTriangle(vi, vi + 1, vi + 2);
				vh.AddTriangle(vi + 1, vi + 3, vi + 2);
			}
		}

		private float GetU(int index, float totalDistance)
		{
			float baseU = UVMode switch
			{
				TrailUVMode.Stretch => points.Count > 1 ? (float)index / (points.Count - 1) : 0f,
				TrailUVMode.RepeatBySegment => index,
				TrailUVMode.RepeatByDistance => distances[index] / totalDistance,
				_ => 0f
			};

			return baseU * UVTiling.x + UVOffset.x;
		}

		private static Color32 MultiplyColor(Color32 a, Color b)
		{
			float r = a.r / 255f * b.r;
			float g = a.g / 255f * b.g;
			float bl = a.b / 255f * b.b;
			float al = a.a / 255f * b.a;
			return new Color(r, g, bl, al);
		}

		private static Gradient CreateLifetimeGradient()
		{
			var gradient = new Gradient();
			gradient.SetKeys(
				new[]
				{
					new GradientColorKey(Color.white, 0f),
					new GradientColorKey(Color.white, 1f)
				},
				new[]
				{
					new GradientAlphaKey(1f, 0f),
					new GradientAlphaKey(0f, 1f)
				}
			);
			return gradient;
		}

		private static Gradient CreateTrailGradient()
		{
			var gradient = new Gradient();
			gradient.SetKeys(
				new[]
				{
					new GradientColorKey(Color.white, 0f),
					new GradientColorKey(Color.white, 1f)
				},
				new[]
				{
					new GradientAlphaKey(1f, 0f),
					new GradientAlphaKey(1f, 1f)
				}
			);
			return gradient;
		}

		private void EnsureGradients()
		{
			if (LifetimeGradient == null)
				LifetimeGradient = CreateLifetimeGradient();

			if (TrailGradient == null)
				TrailGradient = CreateTrailGradient();
		}

		private Vector2 GetPointNormal(IReadOnlyList<Vector2> localPoints, int index)
		{
			Vector2 dir;
			if (index == 0)
				dir = localPoints[1] - localPoints[0];
			else if (index == localPoints.Count - 1)
				dir = localPoints[index] - localPoints[index - 1];
			else
				dir = (localPoints[index + 1] - localPoints[index - 1]) * 0.5f;

			if (dir.sqrMagnitude <= Mathf.Epsilon)
				return Vector2.up;

			dir.Normalize();
			return new Vector2(-dir.y, dir.x);
		}
	}
}
