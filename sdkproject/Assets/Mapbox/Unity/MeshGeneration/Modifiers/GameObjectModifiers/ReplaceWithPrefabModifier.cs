namespace Mapbox.Unity.MeshGeneration.Modifiers
{
	using UnityEngine;
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Unity.MeshGeneration.Components;
	using Mapbox.Unity.MeshGeneration.Interfaces;
	using System.Collections.Generic;
	using System;
	using System.Linq;

	[Serializable]
	public struct IDPrefabPair
	{
		public string id;
		public GameObject prefab;
	}
	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Replace With Prefab Modifier")]
	public class ReplaceWithPrefabModifier : GameObjectModifier
	{
		[SerializeField]
		private List<IDPrefabPair> _prefabs;

		[SerializeField]
		private bool _scaleDownWithWorld = false;

		private Dictionary<GameObject, GameObject> _objects;
		private Dictionary<string, GameObject> _prefabDictionary;

		public override void Initialize()
		{
			if (_objects == null)
			{
				_objects = new Dictionary<GameObject, GameObject>();
			}
			if (_prefabDictionary == null)
			{
				_prefabDictionary = new Dictionary<string, GameObject>();
			}

			foreach (var prefabIdPair in _prefabs)
			{
				_prefabDictionary.Add(prefabIdPair.id, prefabIdPair.prefab);
			}

		}

		public override void Run(VectorEntity ve, UnityTile tile)
		{
			if (_prefabDictionary.ContainsKey(ve.Feature.Data.Id.ToString()))
			{
				int selpos = ve.Feature.Points[0].Count / 2;
				var met = ve.Feature.Points[0][selpos];

				IFeaturePropertySettable settable = null;
				GameObject go;

				if (_objects.ContainsKey(ve.GameObject))
				{
					go = _objects[ve.GameObject];
				}
				else
				{
					go = Instantiate(_prefabDictionary[ve.Feature.Data.Id.ToString()]);
					_objects.Add(ve.GameObject, go);
				}

				go.name = ve.Feature.Data.Id.ToString();
				go.transform.position = met;
				go.transform.SetParent(ve.GameObject.transform, false);
				go.transform.localScale = Constants.Math.Vector3One;

				settable = go.GetComponent<IFeaturePropertySettable>();
				if (settable != null)
				{
					go = (settable as MonoBehaviour).gameObject;
					settable.Set(ve.Feature.Properties);
				}

				if (!_scaleDownWithWorld)
				{
					go.transform.localScale = Vector3.one / tile.TileScale;
				}
			}

		}
	}
}
