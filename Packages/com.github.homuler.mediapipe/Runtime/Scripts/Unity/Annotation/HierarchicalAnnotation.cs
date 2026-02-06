// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace Mediapipe.Unity
{
	public interface IHierachicalAnnotation
	{
		IHierachicalAnnotation root { get; }
		Transform transform { get; }
		UnityEngine.Rect GetScreenRect();
	}

	public abstract class HierarchicalAnnotation : MonoBehaviour, IHierachicalAnnotation
	{
		private IHierachicalAnnotation _root;
		private RectTransform _cachedScreenRectTransform;

		public IHierachicalAnnotation root
		{
			get
			{
				if (_root != null) return _root;
				var parentObj = transform.parent?.gameObject;
				if (parentObj is null) return this;
				_root = parentObj.TryGetComponent<IHierachicalAnnotation>(out var parent)
					? parent.root : this;

				return _root;
			}
			protected set => _root = value;
		}

		private RectTransform ResolveScreenRectTransform()
		{
			if (_cachedScreenRectTransform is not null) return _cachedScreenRectTransform;
			var parent = root.transform.parent;
			_cachedScreenRectTransform = parent as RectTransform;

			return _cachedScreenRectTransform;
		}

		public UnityEngine.Rect GetScreenRect() => ResolveScreenRectTransform()?.rect ?? default;

		// activeSelf only take accounts for the object itself is active or not
		// activeInHierarchy include whether the object's parent is also active or not
		public bool isActiveInHierarchy => gameObject.activeInHierarchy;

		public void SetActive(bool active)
		{
			if (gameObject.activeSelf != active)
			{
				gameObject.SetActive(active);
			}
		}

		/// <summary>
		///   Prepare to annotate <paramref name="target" />.
		///   If <paramref name="target" /> is not null, it activates itself.
		/// </summary>
		/// <return>
		///   If it is activated and <paramref name="target" /> can be drawn.
		///   In effect, it returns if <paramref name="target" /> is null or not.
		/// </return>
		/// <param name="target">Data to be annotated</param>
		protected bool ActivateFor<T>(T target)
		{
			if (target is null)
			{
				SetActive(false);
				return false;
			}

			SetActive(true);
			return true;
		}

		public virtual bool isMirrored { get; set; }
		public virtual RotationAngle rotationAngle { get; set; } = RotationAngle.Rotation0;

		protected TH InstantiateChild<TH>(GameObject prefab)
			where TH : HierarchicalAnnotation
		{
			Instantiate(prefab, transform).TryGetComponent<TH>(out var annotation);
			annotation.isMirrored = isMirrored;
			annotation.rotationAngle = rotationAngle;
			return annotation;
		}

		protected TH InstantiateChild<TH>(string obName = "Game Object")
			where TH : HierarchicalAnnotation
		{
			var gameOb = new GameObject(obName);
			gameOb.transform.SetParent(transform);

			return gameOb.AddComponent<TH>();
		}
	}
}