#if UNITY_5_5 || UNITY_5_6 || UNITY_2017
using UnityEngine.AI;
#endif

using System;
using System.Collections.Generic;
#if UNITY_EDITOR
	using UnityEditor;
#endif 
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace DarkTonic.PoolBoss {
	/// <summary>
	/// This class is used to spawn and despawn things using pooling (avoids Instantiate and Destroy calls).
	/// </summary>
	// ReSharper disable once CheckNamespace
	public class PoolBoss : MonoBehaviour {
		/*! \cond PRIVATE */
		public const string NoCategory = "[Uncategorized]";
		/*! \endcond */
		
		private const string SpawnedMessageName = "OnSpawned";
		private const string DespawnedMessageName = "OnDespawned";
		
		private const string NotInitError =
			"Pool Boss has not initialized (does so in Awake event) and is not ready to be used yet.";
		
		/*! \cond PRIVATE */
		// ReSharper disable InconsistentNaming
		public List<PoolBossItem> poolItems = new List<PoolBossItem>();
		public bool logMessages = false;
		public bool autoAddMissingPoolItems = false;
		public bool allowDespawningInactive = false;
		public string newCategoryName = "New Category";
		public string addToCategoryName = "New Category";
        public int framesForInit = 1;
        public int _changes;
		
		public List<PoolBossCategory> _categories = new List<PoolBossCategory> {
			new PoolBossCategory()
		};
		
		// ReSharper restore InconsistentNaming
		/*! \endcond */
		
		private static readonly Dictionary<string, PoolItemInstanceList> PoolItemsByName =
			new Dictionary<string, PoolItemInstanceList>(StringComparer.OrdinalIgnoreCase);
		
		private static Transform _trans;

	    // ReSharper disable InconsistentNaming
        private static readonly List<PoolableInfo> _deadList = new List<PoolableInfo>(16);
        private static readonly Dictionary<PoolableInfo, object> _potentialInSceneObjects = new Dictionary<PoolableInfo, object>(16);
        // ReSharper restore InconsistentNaming
        private static PoolBoss _instance;
        private static int _initFrameStart;
        private static float _itemsToInitPerFrame;
		private static int _lastFramInitContinued = -1;
		private int _itemsInited = 0;
        private static bool _isReady;
		
		/*! \cond PRIVATE */
		
		public class PoolItemInstanceList {
			public bool LogMessages;
			public bool AllowInstantiateMore;
			public int? ItemHardLimit;
			public bool EnableNavMeshAgent;
			public Transform SourceTrans;
			public List<Transform> SpawnedClones;
			public List<Transform> DespawnedClones;
			public bool AllowRecycle;
			public string CategoryName;
			public int Peak;
			public float PeakTime;

			public PoolItemInstanceList(List<Transform> clones) {
                DespawnedClones = clones;
                SpawnedClones = new List<Transform>(clones.Count);
			}
		}
		
		public static PoolBoss Instance {
			get {
				// ReSharper disable once ConvertIfStatementToNullCoalescingExpression
				if (_instance == null) {
					_instance = (PoolBoss)FindObjectOfType(typeof(PoolBoss));
				}
				
				return _instance;
			}
		}
		
		/*! \endcond */
		
		// ReSharper disable once UnusedMember.Local
		private void Awake() {
			Initialize();
		}

        void Update() {
            if (_isReady) {
                RegisterInSceneObjects();
                _changes++;
				return;
            }

            ContinueInit();
        }

	    private void RegisterInSceneObjects() {
	        if (_potentialInSceneObjects.Keys.Count == 0) {
	            return;
	        }

            _deadList.Clear();

	        foreach (var key in _potentialInSceneObjects.Keys) {
                var itemTrans = key.GetComponent<Transform>();
	            PoolItemInstanceList match = null;

                if (PoolItemsByName.ContainsKey(key.ItemName)) {
                    match = PoolItemsByName[key.ItemName];
                } else {
                    if (!autoAddMissingPoolItems) {
                        Debug.LogWarning("Could not create Pool Boss item for in-Scene game object '" + key.ItemName + "' because Auto-Add Missing Items is turned off.");
                    } else {
                        var itemName = CreateMissingPoolItem(itemTrans, key.ItemName, false);
                        match = PoolItemsByName[itemName];
                    }
                }

                if (match != null) {
                    match.SpawnedClones.Add(itemTrans);
                }

                _deadList.Add(key);
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _deadList.Count; i++) { 
                _potentialInSceneObjects.Remove(_deadList[i]);
	        }
	    }

        /*! \cond PRIVATE */
        public static void Initialize() {
            _isReady = false;
			_lastFramInitContinued = -1;

            _initFrameStart = Time.frameCount;
            _itemsToInitPerFrame = ((float)Instance.poolItems.Count) / Instance.framesForInit;
			PoolItemsByName.Clear();

            Instance.ContinueInit();
        }

        private void ContinueInit() {
            if (_isReady || Time.frameCount <= _lastFramInitContinued) {
                return;
            }

			_lastFramInitContinued = Time.frameCount;

            var itemCountToStopAt = Instance.poolItems.Count;
            if (Instance.framesForInit != 1) {
                var framesInitSoFar = Time.frameCount - _initFrameStart + 1;
                itemCountToStopAt = (int)Math.Max(framesInitSoFar * _itemsToInitPerFrame, 0);
            }

            if (logMessages) {
                Debug.Log("Pool Boss initializing: frame #: " + Time.frameCount + " - creating items: " + (_itemsInited + 1) + " - " + itemCountToStopAt);
            }

            PoolBossItem item = null;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var p = _itemsInited; p < itemCountToStopAt; p++) {
                item = Instance.poolItems[p];

                Instance.CreatePoolItemClones(item, true);
                _itemsInited++;
            }

            if (itemCountToStopAt != Instance.poolItems.Count) {
                return;
            }

            if (logMessages) {
                Debug.Log("Pool Boss done initializing in frame #: " + Time.frameCount);
            }

            _isReady = true;
        }

        /*! \endcond */

        /// <summary>
        /// This is called by PoolableInfo components on objects that begin in the Scene, so Pool Boss will know about them and put them in the "Spawned" list.
        /// </summary>
        /// <param name="poolable"></param>
	    public static void RegisterPotentialInScenePoolable(PoolableInfo poolable) {
            if (_potentialInSceneObjects.ContainsKey(poolable)) {
                return;
            }

            _potentialInSceneObjects.Add(poolable, null);
	    }

        /// <summary>
        /// This is called by PoolableInfo components that get spawned, so we know to remove them from the in scene objects list.
        /// </summary>
        /// <param name="poolable"></param>
	    public static void UnregisterNonStartInScenePoolable(PoolableInfo poolable) {
            _potentialInSceneObjects.Remove(poolable);
        }

        /// <summary>
        /// This method allows you to add a new Pool Item at runtime.
        /// </summary>
        /// <param name="itemTrans">The Transform of the item.</param>
        /// <param name="preloadInstances">The number of instances to preload.</param>
        /// <param name="canInstantiateMore">Can instantiate more or not</param>
        /// <param name="hardLimit">Item Hard Limit</param>
        /// <param name="logMsgs">Log messages during spawn and despawn.</param>
        /// <param name="catName">Category name</param>
        public static void CreateNewPoolItem(Transform itemTrans, int preloadInstances, bool canInstantiateMore,
		                                     int hardLimit, bool logMsgs, string catName) {
			var newItem = new PoolBossItem() {
				prefabTransform = itemTrans,
				instancesToPreload = preloadInstances,
				allowInstantiateMore = canInstantiateMore,
				itemHardLimit = hardLimit,
				isExpanded = true,
				logMessages = logMsgs,
				categoryName = catName
			};
			
			if (string.IsNullOrEmpty(catName)) {
				newItem.categoryName = Instance._categories[0].CatName;
			}
			
			Instance.CreatePoolItemClones(newItem, false);
		}
		
		// ReSharper disable once MemberCanBeMadeStatic.Local
		private void CreatePoolItemClones(PoolBossItem item, bool isDuringAwake) {
			if (!isDuringAwake) {
				Instance.poolItems.Add(item);
			}
			
			if (item.instancesToPreload <= 0) {
				return;
			}
			
			if (item.prefabTransform == null) {
				if (isDuringAwake) {
					Debug.LogWarning("You have an item in Pool Boss with no prefab assigned in category: " + item.categoryName);
				} else {
					Debug.LogWarning("You are attempting to add a Pool Boss Item with no prefab assigned.");
				}
				return;
			}
			
			var itemName = GetPrefabName(item.prefabTransform); // calling this here will add the PoolableInfo script to the prefab, so all clones will get it.
			if (PoolItemsByName.ContainsKey(itemName)) {
				Debug.LogWarning("You have more than one instance of '" + itemName + "' in Pool Boss. Skipping the second instance.");
				return;
			}
			
			var itemClones = new List<Transform>();
			
			var navAgent = item.prefabTransform.GetComponent<UnityEngine.AI.NavMeshAgent>();
			var hasAgent = navAgent != null;
			
			for (var i = 0; i < item.instancesToPreload; i++) {
				var createdObjTransform = InstantiateForPool(item.prefabTransform, i + 1);
				itemClones.Add(createdObjTransform);
			}
			
			var instanceList = new PoolItemInstanceList(itemClones) {
				LogMessages = item.logMessages,
				AllowInstantiateMore = item.allowInstantiateMore,
				SourceTrans = item.prefabTransform,
				ItemHardLimit = item.itemHardLimit,
				AllowRecycle = item.allowRecycle,
				EnableNavMeshAgent = hasAgent && item.enableNavMeshAgentOnSpawn,
				CategoryName = item.categoryName
			};
			
			if (Instance._categories.Find(delegate(PoolBossCategory x) { return x.CatName == item.categoryName; }) == null) {
				Instance._categories.Add(new PoolBossCategory() {
					CatName = item.categoryName,
					IsExpanded = true,
					IsEditing = false
				});
			}
			
			PoolItemsByName.Add(itemName, instanceList);
		}
		
		private static Transform InstantiateForPool(Transform prefabTrans, int cloneNumber) {
			// ReSharper disable once JoinDeclarationAndInitializer
			Transform createdObjTransform;
			
			createdObjTransform = Instantiate(prefabTrans, Trans.position, prefabTrans.rotation) as Transform;
			
			// ReSharper disable once PossibleNullReferenceException
			createdObjTransform.name = prefabTrans.name + " (Clone " + cloneNumber + ")";
			// don't want the "(Clone)" suffix.
			
			SetParent(createdObjTransform, Trans);
			
			SetActive(createdObjTransform.gameObject, false);
			
			return createdObjTransform;
		}
		
		private static string CreateMissingPoolItem(Transform missingTran, string itemName, bool isSpawn, bool enableNavMeshAgentOnSpawn = true) {
#if UNITY_EDITOR
			var prefab = PrefabUtility.GetPrefabParent(missingTran) as Transform;
            if (prefab == null) {
                prefab = missingTran; // there is no parent because it was already the one from Hierarchy (prefab, not instance)
            }
#else 
			var prefab = missingTran;
#endif

            var instances = new List<Transform>();
			
			if (isSpawn) {
				var createdObjTransform = InstantiateForPool(prefab, instances.Count + 1);
				instances.Add(createdObjTransform);
			}
			
			var navAgent = prefab.GetComponent<UnityEngine.AI.NavMeshAgent>();
			var hasNavAgent = navAgent != null;
			
			var catName = Instance._categories[0].CatName;
			
			var newItemSettings = new PoolItemInstanceList(instances) {
				LogMessages = false,
				AllowInstantiateMore = true,
				SourceTrans = prefab,
				EnableNavMeshAgent = hasNavAgent && enableNavMeshAgentOnSpawn,
				CategoryName = catName
			};
			
			PoolItemsByName.Add(itemName, newItemSettings);
			
			// for the Inspector only
			Instance.poolItems.Add(new PoolBossItem() {
				instancesToPreload = 1,
				isExpanded = true,
				allowInstantiateMore = true,
				logMessages = false,
				prefabTransform = prefab,
				categoryName = catName
			});
			
			if (Instance.logMessages) {
				Debug.LogWarning("PoolBoss created Pool Item for missing item '" + itemName + "' at " + Time.time);
			}

		    return itemName;
		}
		
		/// <summary>
		/// Call this method to spawn a prefab using Pool Boss, which will be spawned with no parent Transform (outside the pool)
		/// </summary>
		/// <param name="itemName">Name of Transform to spawn</param>
		/// <param name="position">The position to spawn it at</param>
		/// <param name="rotation">The rotation to use</param>
		/// <returns>The Transform of the spawned object. It can be null if spawning failed from limits you have set.</returns>
		public static Transform SpawnOutsidePool(string itemName, Vector3 position, Quaternion rotation) {
			return Spawn(itemName, position, rotation, null);
		}
		
		/// <summary>
		/// Call this method to spawn a prefab using Pool Boss, which will be spawned with no parent Transform (outside the pool)
		/// </summary>
		/// <param name="transToSpawn">Transform to spawn</param>
		/// <param name="position">The position to spawn it at</param>
		/// <param name="rotation">The rotation to use</param>
		/// <returns>The Transform of the spawned object. It can be null if spawning failed from limits you have set.</returns>
		public static Transform SpawnOutsidePool(Transform transToSpawn, Vector3 position, Quaternion rotation) {
			return Spawn(transToSpawn, position, rotation, null);
		}
		
		/// <summary>
		/// Call this method to spawn a prefab using Pool Boss, which will be a child of the Pool Boss prefab.
		/// </summary>
		/// <param name="itemName">Name of Transform to spawn</param>
		/// <param name="position">The position to spawn it at</param>
		/// <param name="rotation">The rotation to use</param>
		/// <returns>The Transform of the spawned object. It can be null if spawning failed from limits you have set.</returns>
		public static Transform SpawnInPool(string itemName, Vector3 position, Quaternion rotation) {
			return Spawn(itemName, position, rotation, Trans);
		}
		
		/// <summary>
		/// Call this method to spawn a prefab using Pool Boss, which will be a child of the Pool Boss prefab.
		/// </summary>
		/// <param name="transToSpawn">Transform to spawn</param>
		/// <param name="position">The position to spawn it at</param>
		/// <param name="rotation">The rotation to use</param>
		/// <returns>The Transform of the spawned object. It can be null if spawning failed from limits you have set.</returns>
		public static Transform SpawnInPool(Transform transToSpawn, Vector3 position, Quaternion rotation) {
			return Spawn(transToSpawn, position, rotation, Trans);
		}
		
		/// <summary>
		/// Call this method to spawn a prefab using Pool Boss.
		/// </summary>
		/// <param name="transToSpawn">Transform to spawn</param>
		/// <param name="position">The position to spawn it at</param>
		/// <param name="rotation">The rotation to use</param>
		/// <param name="parentTransform">The parent Transform to use</param>
		/// <returns>The Transform of the spawned object. It can be null if spawning failed from limits you have set.</returns>
		public static Transform Spawn(Transform transToSpawn, Vector3 position, Quaternion rotation, Transform parentTransform) {
			if (!_isReady) {
				Debug.LogWarning(NotInitError);
				return null;
			}
			
			if (transToSpawn == null) {
				Debug.LogWarning("No Transform passed to Spawn method.");
				return null;
			}
			
			if (Instance == null) {
				return null;
			}
			
			var itemName = GetPrefabName(transToSpawn);

			if (PoolItemsByName.ContainsKey(itemName)) {
				return Spawn(itemName, position, rotation, parentTransform);
			}
			
			if (Instance.autoAddMissingPoolItems) {
				CreateMissingPoolItem(transToSpawn, itemName, true);
			} else {
				Debug.LogWarning("The Transform '" + itemName +
				                 "' passed to Spawn method is not configured in Pool Boss.");
				return null;
			}
			
			return Spawn(itemName, position, rotation, parentTransform);
		}
		
		/// <summary>
		/// Call this method to spawn a prefab using Pool Boss. 
		/// </summary>
		/// <param name="itemName">Name of the transform to spawn</param>
		/// <param name="position">The position to spawn it at</param>
		/// <param name="rotation">The rotation to use</param>
		/// <param name="parentTransform">The parent Transform to use</param>
		/// <returns>The Transform of the spawned object. It can be null if spawning failed from limits you have set.</returns>
		public static Transform Spawn(string itemName, Vector3 position, Quaternion rotation, Transform parentTransform) {
			var itemSettings = PoolItemsByName[itemName];
			
			Transform cloneToSpawn = null;
			
			if (itemSettings.DespawnedClones.Count == 0) {
				if (!itemSettings.AllowInstantiateMore) {
					if (itemSettings.AllowRecycle) {
						cloneToSpawn = itemSettings.SpawnedClones[0];
						// keep the SpawnedClones and DespawnedClones arrays in line.
						Despawn(cloneToSpawn);
					} else {
						Debug.LogWarning(
							"The Transform '" + itemName +
							"' has no available clones left to Spawn in Pool Boss. Please increase your Preload Qty, turn on Allow Instantiate More or turn on Recycle Oldest (Recycle is only for non-essential things like decals).");
						return null;
					}
				} else {
					// Instantiate a new one
					var curCount = NumberOfClones(itemSettings);
					if (curCount >= itemSettings.ItemHardLimit) {
						Debug.LogWarning(
							"The Transform '" + itemName +
							"' has reached its item limit in Pool Boss. Please increase your Preload Qty or Item Limit.");
						return null;
					}
					
					var createdObjTransform = InstantiateForPool(itemSettings.SourceTrans, curCount + 1);
					itemSettings.DespawnedClones.Add(createdObjTransform);
					
					if (Instance.logMessages || itemSettings.LogMessages) {
						Debug.LogWarning("Pool Boss Instantiated an extra '" + itemName + "' at " + Time.time +
						                 " because there were none left in the Pool.");
					}
				}
			}
			
			if (cloneToSpawn == null) {
				cloneToSpawn = itemSettings.DespawnedClones[0];
			} else {
				// recycling
				cloneToSpawn.BroadcastMessage(DespawnedMessageName, SendMessageOptions.DontRequireReceiver);
			}
			
			if (cloneToSpawn == null) {
				Debug.LogWarning("One or more of the prefab '" + itemName +
				                 "' in Pool Boss has been destroyed. You should never destroy objects in the Pool. Despawn instead. Not spawning anything for this call.");
				return null;
			}
			
			cloneToSpawn.position = position;
			cloneToSpawn.rotation = rotation;
			SetActive(cloneToSpawn.gameObject, true);
			Instance._changes++;
			
			if (Instance.logMessages || itemSettings.LogMessages) {
				Debug.Log("Pool Boss spawned '" + itemName + "' at " + Time.time);
			}
			
			SetParent(cloneToSpawn, parentTransform);
			
			cloneToSpawn.BroadcastMessage(SpawnedMessageName, SendMessageOptions.DontRequireReceiver);
			
			if (itemSettings.EnableNavMeshAgent) {
				var agent = cloneToSpawn.GetComponent<UnityEngine.AI.NavMeshAgent>();
				if (agent != null) {
					agent.enabled = true;
				}
			}
			
			itemSettings.DespawnedClones.Remove(cloneToSpawn);
			itemSettings.SpawnedClones.Add(cloneToSpawn);

			if (itemSettings.Peak < itemSettings.SpawnedClones.Count) {
				itemSettings.Peak = itemSettings.SpawnedClones.Count;
				itemSettings.PeakTime = Time.realtimeSinceStartup;
				Instance._changes++;
			}

			return cloneToSpawn;
		}
		
		private static void SetParent(Transform trns, Transform parentTrans) {
			#if UNITY_4_6 || UNITY_4_7 || UNITY_5 || UNITY_2017 || UNITY_2018 || UNITY_2019
			var rectTrans = trns as RectTransform;
			if (rectTrans != null) {
				rectTrans.SetParent(parentTrans);
			} else {
				trns.SetParent(parentTrans);
			}
			#else
			trns.parent = parentTrans;
			#endif
		}
		
		/// <summary>
		/// This method returns the number of items in a category that are currently despawned.
		/// </summary>
		/// <param name="category">Category name</param>
		/// <returns>integer</returns>
		public static int CategoryItemsDespawned(string category) {
			if (Instance == null) {
				// Scene changing, do nothing.
				return 0;
			}
			
			var itemCount = 0;
			
			var items = PoolItemsByName.Values.GetEnumerator();
			while (items.MoveNext()) {
				// ReSharper disable once PossibleNullReferenceException
				if (items.Current.CategoryName != category) {
					continue;
				}
				
				if (items.Current.DespawnedClones.Count > 0) {
					itemCount += items.Current.DespawnedClones.Count;
				}
			}
			
			return itemCount;
		}
		
		/// <summary>
		/// This method returns the number of items in a category that are currently spawned.
		/// </summary>
		/// <param name="category">Category name</param>
		/// <returns>integer</returns>
		public static int CategoryItemsSpawned(string category) {
			if (Instance == null) {
				// Scene changing, do nothing.
				return 0;
			}
			
			var itemCount = 0;
			
			var items = PoolItemsByName.Values.GetEnumerator();
			while (items.MoveNext()) {
				// ReSharper disable once PossibleNullReferenceException
				if (items.Current.CategoryName != category) {
					continue;
				}
				
				if (items.Current.SpawnedClones.Count > 0) {
					itemCount += items.Current.SpawnedClones.Count;
				}
			}
			
			return itemCount;
		}

        /// <summary>
        /// Call this method to despawn a prefab using Pool Boss. 
        /// </summary>
        /// <param name="transToDespawn">Transform to despawn</param>
        /// <param name="keepParent">Specify true if you want the Game Object to stay where it is in the Hierarch (necessary for UI and come other components)</param>
        public static void Despawn(Transform transToDespawn, bool keepParent = false) {
			if (!_isReady) {
				Debug.LogWarning(NotInitError);
				return;
			}
			
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			if (transToDespawn == null) {
				Debug.LogWarning("No Transform passed to Despawn method.");
				return;
			}
			
			// ReSharper disable HeuristicUnreachableCode
			if (Instance == null) {
				// Scene changing, do nothing.
				return;
			}
			 
			if (!IsSpawned(transToDespawn.gameObject)) {
				if (Instance.logMessages) { 
					Debug.LogWarning("Game Object is already despawned. Will not despawn '" + transToDespawn.name + "'.");
				}
				return; // already sent to despawn
			}
			
			var itemName = GetPrefabName(transToDespawn);

			if (!PoolItemsByName.ContainsKey(itemName)) {
				if (Instance.autoAddMissingPoolItems) {
					CreateMissingPoolItem(transToDespawn, itemName, false);
				} else {
					// Debug.LogWarning("The Transform '" + itemName + "' passed to Despawn is not in Pool Boss. Not despawning. If you want this to be allowed, check the Auto-Add Missing Items checkbox.");
					
					// @blue7un: 2018-03-19
					Debug.LogWarning("The Transform '" + itemName + "' passed to Despawn is not in Pool Boss, will destroy it");
					Destroy(transToDespawn.gameObject);
					return;
				}
			}
			
			transToDespawn.BroadcastMessage(DespawnedMessageName, SendMessageOptions.DontRequireReceiver);
			
			var cloneList = PoolItemsByName[itemName];

            if (!keepParent) {
                SetParent(transToDespawn, Trans);
            }

            SetActive(transToDespawn.gameObject, false);
			Instance._changes++;
			
			if (Instance.logMessages || cloneList.LogMessages) {
				Debug.Log("Pool Boss despawned '" + itemName + "' at " + Time.time);
			}
			
			cloneList.SpawnedClones.Remove(transToDespawn);
			cloneList.DespawnedClones.Add(transToDespawn);
			// ReSharper restore HeuristicUnreachableCode
		}
		
		/// <summary>
		/// This method will despawn all spawned instances of prefabs.
		/// </summary>
		public static void DespawnAllPrefabs() {
			if (Instance == null) {
				// Scene changing, do nothing.
				return;
			}
			
			var items = PoolItemsByName.Values.GetEnumerator();
			while (items.MoveNext()) {
				// ReSharper disable once PossibleNullReferenceException
				DespawnAllOfPrefab(items.Current.SourceTrans);
			}
		}
		
		/// <summary>
		/// This method will Despawn all spawned instances of all prefabs in a single category.
		/// </summary>
		/// <param name="category">category name</param>
		public static void DespawnAllPrefabsInCategory(string category) {
			if (Instance == null) {
				// Scene changing, do nothing.
				return;
			}
			
			var items = PoolItemsByName.Values.GetEnumerator();
			while (items.MoveNext()) {
				// ReSharper disable once PossibleNullReferenceException
				if (items.Current.CategoryName != category) {
					continue;
				}
				
				DespawnAllOfPrefab(items.Current.SourceTrans);
			}
		}
		
		/// <summary>
		/// This method will despawn all spawned instances of the prefab you pass in.
		/// </summary>
		/// <param name="transToDespawn">Transform component of a prefab</param>
		public static void DespawnAllOfPrefab(Transform transToDespawn) {
			if (Instance == null) {
				// Scene changing, do nothing.
				return;
			}
			
			if (transToDespawn == null) {
				Debug.LogWarning("No Transform passed to DespawnAllOfPrefab method.");
				return;
			}
			
			var itemName = GetPrefabName(transToDespawn);

			if (!PoolItemsByName.ContainsKey(itemName)) {
				Debug.LogWarning("The Transform '" + itemName +
				                 "' passed to DespawnAllOfPrefab is not in Pool Boss. Not despawning.");
				return;
			}
			
			var spawned = PoolItemsByName[itemName].SpawnedClones;
			
			var max = spawned.Count;
			while (spawned.Count > 0 && max > 0) {
				Despawn(spawned[0]);
				max--;
			}
		}

		/// <summary>
		/// Destroys the pool item, and all prefabs from it that are already spawned. You should never call this except maybe during a new Scene load when you no longer need an item.
		/// </summary>
		/// <param name="transDeadItem">Trans dead item.</param>
		public static void DestroyPoolItem(Transform transDeadItem) {
			if (Instance == null) {
				// Scene changing, do nothing.
				return;
			}
			
			if (transDeadItem == null) {
				Debug.LogWarning("No Transform passed to DestroyPoolItem method.");
				return;
			}
			
			var itemName = GetPrefabName(transDeadItem);

			if (!PoolItemsByName.ContainsKey(itemName)) {
				Debug.LogWarning("The Transform '" + itemName +
				                 "' passed to DestroyPoolItem is not in Pool Boss. Not despawning.");
				return;
			}

			var item = PoolItemsByName[itemName];

			for (var i = 0; i< item.DespawnedClones.Count; i++) {
				GameObject.Destroy(item.DespawnedClones[i].gameObject);
			}

			for (var i = 0; i< item.SpawnedClones.Count; i++) {
				GameObject.Destroy(item.SpawnedClones[i].gameObject);
			}

			PoolItemsByName.Remove(itemName);

			var deadItem = Instance.poolItems.Find(delegate (PoolBossItem x) {
				return x.prefabTransform != null && x.prefabTransform.name == item.SourceTrans.name;
			});
			 
			if (deadItem != null) {
				Instance.poolItems.Remove(deadItem);
			}
		}

		/// <summary>
		/// Destroys all pool items in the category specified, and all prefabs from it that are already spawned. You should never call this except maybe during a new Scene load when you no longer need all items in a category.
		/// </summary>
		/// <param name="transDeadItem">Trans dead item.</param>
		public static void DestroyCategoryPoolItems(string categoryName) {
			if (Instance == null) {
				// Scene changing, do nothing.
				return;
			}
			
			if (string.IsNullOrEmpty(categoryName)) {
				Debug.LogWarning("No Category Name passed to DestroyCategoryPoolItems method.");
				return;
			}

			var matchingItems = new List<PoolItemInstanceList>();

			foreach (var key in PoolItemsByName.Keys) {
				var item = PoolItemsByName[key];

				if (item.CategoryName == categoryName) {
					matchingItems.Add(item);
				}
			}

			foreach (var item in matchingItems) {
				for (var i = 0; i< item.DespawnedClones.Count; i++) {
					GameObject.Destroy(item.DespawnedClones[i].gameObject);
				}
				
				for (var i = 0; i< item.SpawnedClones.Count; i++) {
					GameObject.Destroy(item.SpawnedClones[i].gameObject);
				}
				
				PoolItemsByName.Remove(item.SourceTrans.name);

				var deadItem = Instance.poolItems.Find(delegate (PoolBossItem x) {
					return x.prefabTransform != null && x.prefabTransform.name == item.SourceTrans.name;
				});
				
				if (deadItem != null) {
					Instance.poolItems.Remove(deadItem);
				}
			}
		}

		/// <summary>
		/// Call this method get info on a Pool Boss item (number of spawned and despawned copies, allow instantiate more, log etc).
		/// </summary>
		/// <param name="poolItemName">The name of the prefab you're asking about.</param>
		/// <returns>The list of pool items.</returns>
		public static PoolItemInstanceList PoolItemInfoByName(string poolItemName) {
			if (string.IsNullOrEmpty(poolItemName)) {
				return null;
			}
			
			if (!PoolItemsByName.ContainsKey(poolItemName)) {
				return null;
			}
			
			return PoolItemsByName[poolItemName];
		}
		
		/// <summary>
		/// Call this method determine if the item (Transform) you pass in is set up in Pool Boss.
		/// </summary>
		/// <param name="trans">Transform you want to know is in the Pool or not.</param>
		/// <returns>Boolean value.</returns>
		public static bool PrefabIsInPool(Transform trans) {
			if (_isReady) {
				return PrefabIsInPool(trans.name);
			}
			Debug.LogWarning(NotInitError);
			return false;
		}
		
		/// <summary>
		/// Call this method determine if the item name you pass in is set up in Pool Boss.
		/// </summary>
		/// <param name="transName">Item name you want to know is in the Pool or not.</param>
		/// <returns>Boolean value.</returns>
		public static bool PrefabIsInPool(string transName) {
			if (_isReady) {
				return PoolItemsByName.ContainsKey(GetPrefabShortName(transName));
			}
			Debug.LogWarning(NotInitError);
			return false;
		}

		/// <summary>
		/// This will tell you how many available clones of a prefab are despawned and ready to spawn. A value of -1 indicates an error
		/// </summary>
		/// <param name="transPrefab">The transform component of the prefab you want the despawned count of.</param>
		/// <returns>Integer value.</returns>
		public static int PrefabDespawnedCount(Transform transPrefab) {
			if (Instance == null) {
				// Scene changing, do nothing.
				return -1;
			}
			
			if (transPrefab == null) {
				Debug.LogWarning("No Transform passed to PrefabDespawnedCount method.");
				return -1;
			}
			
			var itemName = GetPrefabName(transPrefab);

			if (!PoolItemsByName.ContainsKey(itemName)) {
				Debug.LogWarning("The Transform '" + itemName +
				                 "' passed to PrefabDespawnedCount is not in Pool Boss. Not despawning.");
				return -1;
			}
			
			var despawned = PoolItemsByName[itemName].DespawnedClones.Count;
			return despawned;
		}
		
		/// <summary>
		/// This will tell you how many clones of a prefab are already spawned out of Pool Boss. A value of -1 indicates an error
		/// </summary>
		/// <param name="transPrefab">The transform component of the prefab you want the spawned count of.</param>
		/// <returns>Integer value.</returns>
		public static int PrefabSpawnedCount(Transform transPrefab) {
			if (Instance == null) {
				// Scene changing, do nothing.
				return -1;
			}
			
			if (transPrefab == null) {
				Debug.LogWarning("No Transform passed to PrefabSpawnedCount method.");
				return -1;
			}
			
			var itemName = GetPrefabName(transPrefab);

			if (!PoolItemsByName.ContainsKey(itemName)) {
				Debug.LogWarning("The Transform '" + itemName +
				                 "' passed to PrefabSpawnedCount is not in Pool Boss. Not despawning.");
				return -1;
			}
			
			var spawned = PoolItemsByName[itemName].SpawnedClones.Count;
			return spawned;
		}
		
		/// <summary>
		/// Call this method to find out if all are despawned
		/// </summary>
		/// <param name="transPrefab">The transform of the prefab you are asking about.</param>
		/// <returns>Boolean value.</returns>
		public static bool AllOfPrefabAreDespawned(Transform transPrefab) {
			return PrefabDespawnedCount(transPrefab) == 0;
		}
		
		
		/// <summary>
		/// This property will tell you how many different items are set up in Pool Boss.
		/// </summary>
		public static int PrefabCount {
			get {
				if (_isReady) {
					return PoolItemsByName.Count;
				}
				Debug.LogWarning(NotInitError);
				
				return -1;
			}
		}
		
		/// <summary>
		/// This will return the name of the game object's prefab without "(Clone X)" in the name. It is used internally by PoolBoss for a lot of things.
		/// </summary>
		/// <param name="trans">The Transform of the game object</param>
		/// <returns>string</returns>
		public static string GetPrefabName(Transform trans) {
			if (trans == null) {
				return null;
			}
			
			var poolable = trans.GetComponent<PoolableInfo>();
			if (poolable != null) {
				return poolable.ItemName;
			}
			
			poolable = trans.gameObject.AddComponent<PoolableInfo>();
			return poolable.ItemName;
		}

		/// <summary>
		/// This will return the name of the game object's prefab without "(Clone X)" in the name. It is used internally by PoolBoss for a lot of things.
		/// </summary>
		/// <param name="go">The Game Object of the game object</param>
		/// <returns>string</returns>
		public static string GetPrefabName(GameObject go) {
			if (go == null) {
				return null;
			}
			
			var poolable = go.GetComponent<PoolableInfo>();
			return poolable.poolItemName;
		}

		/// <summary>
		/// This will return the name of the game object's prefab without "(Clone X)" in the name. 
		/// </summary>
		/// <param name="prefabName">The name of the game object</param>
		/// <returns>string</returns>
		public static string GetPrefabShortName(string prefabName) {
			var iParen = prefabName.IndexOf(" (Clone", StringComparison.Ordinal);
			if (iParen > -1) {
				prefabName = prefabName.Substring(0, iParen);
			}
			
			return prefabName;
		}

		/// <summary>
		/// Call this get the next available item to spawn for a pool item.
		/// </summary>
		/// <param name="trans">Transform you want to get the next item to spawn for.</param>
		/// <returns>Transform</returns>
		public static Transform NextPoolItemToSpawn(Transform trans) {
			return NextPoolItemToSpawn(trans.name);
		}
		
		/// <summary>
		/// Call this get the next available item to spawn for a pool item.
		/// </summary>
		/// <param name="itemName">Name of item you want to get the next item to spawn for.</param>
		/// <returns>Transform</returns>
		public static Transform NextPoolItemToSpawn(string itemName) {
			if (!_isReady) {
				Debug.LogWarning(NotInitError);
			}
			
			if (!PoolItemsByName.ContainsKey(itemName)) {
				return null;
			}
			
			var itemSettings = PoolItemsByName[itemName];
			
			if (itemSettings.DespawnedClones.Count > 0) {
				return itemSettings.DespawnedClones[0];
			}
			
			if (!itemSettings.AllowInstantiateMore) {
				return null;
			}
			
			var totalItems = itemSettings.DespawnedClones.Count + itemSettings.SpawnedClones.Count;
			
			if (itemSettings.ItemHardLimit <= totalItems) {
				return null;
			}
			
			var createdObjTransform = InstantiateForPool(itemSettings.SourceTrans, totalItems + 1);
			itemSettings.DespawnedClones.Add(createdObjTransform);
			
			return createdObjTransform;
		}

		private static int NumberOfClones(PoolItemInstanceList instList) {
			if (_isReady) {
				return instList.DespawnedClones.Count + instList.SpawnedClones.Count;
			}
			Debug.LogWarning(NotInitError);
			return -1;
		}

		public static bool IsSpawned(GameObject go) {
			if (!Instance.allowDespawningInactive) {
				return IsActive(go);
			}
			
			var itemName = GetPrefabName(go);
			if (!PoolItemsByName.ContainsKey(itemName)) {
				return false;
			}
			
			var itemSettings = PoolItemsByName[itemName];

			return !itemSettings.DespawnedClones.Contains(go.GetComponent<Transform>());
		}

		/*! \cond PRIVATE */
		/// <summary>
		/// This is a cross-Unity-version method to tell you if a GameObject is active in the Scene.
		/// </summary>
		/// <param name="go">The GameObject you're asking about.</param>
		/// <returns>True or false</returns>
		public static bool IsActive(GameObject go) {
			return go.activeSelf;
		}
		
		/// <summary>
		/// This is a cross-Unity-version method to set a GameObject to active in the Scene.
		/// </summary>
		/// <param name="go">The GameObject you're setting to active or inactive</param>
		/// <param name="isActive">True to set the object to active, false to set it to inactive.</param>
		public static void SetActive(GameObject go, bool isActive) {
			go.SetActive(isActive);
		}

		public static Transform Trans {
			get {
				// ReSharper disable once ConvertIfStatementToNullCoalescingExpression
				if (_trans == null) {
					_trans = Instance.GetComponent<Transform>();
				}
				
				return _trans;
			}
		}
		/*! \endcond */
	}

	/// <summary>
	/// Extension methods of Pool Boss methods, that you can call with one less parameter from the Transform component.
	/// </summary>
	public static class PoolBossExtensions {
		/// <summary>
		/// Calls the same named Pool Boss method
		/// </summary>
		/// <param name="transPrefab"></param>
		/// <returns></returns>
		public static bool AllOfPrefabAreDespawned(this Transform transPrefab) {
			return PoolBoss.AllOfPrefabAreDespawned(transPrefab);
		}
		
		/// <summary>
		/// Calls the same named Pool Boss method
		/// </summary>
		/// <param name="itemTrans"></param>
		/// <param name="preloadInstances"></param>
		/// <param name="canInstantiateMore"></param>
		/// <param name="hardLimit"></param>
		/// <param name="logMsgs"></param>
		/// <param name="catName"></param>
		public static void CreateNewPoolItem(this Transform itemTrans, int preloadInstances, bool canInstantiateMore, int hardLimit, bool logMsgs, string catName) {
			PoolBoss.CreateNewPoolItem(itemTrans, preloadInstances, canInstantiateMore, hardLimit, logMsgs, catName);
		}
		
		/// <summary>
		/// Calls the same named Pool Boss method
		/// </summary>
		/// <param name="transToDespawn"></param>
		public static void Despawn(this Transform transToDespawn) {
			PoolBoss.Despawn(transToDespawn);
		}
		
		/// <summary>
		/// Calls the same named Pool Boss method
		/// </summary>
		/// <param name="transToDespawn"></param>
		public static void DespawnAllOfPrefab(this Transform transToDespawn) {
			PoolBoss.DespawnAllOfPrefab(transToDespawn);
		}
		
		/// <summary>
		/// Calls the same named Pool Boss method
		/// </summary>
		/// <param name="trans"></param>
		/// <returns></returns>
		public static string GetPrefabName(this Transform trans) {
			return PoolBoss.GetPrefabName(trans);
		}
		
		/// <summary>
		/// Calls the same named Pool Boss method
		/// </summary>
		/// <param name="trans"></param>
		/// <returns></returns>
		public static Transform NextPoolItemToSpawn(this Transform trans) {
			return PoolBoss.NextPoolItemToSpawn(trans);
		}
		
		/// <summary>
		/// Calls the same named Pool Boss method
		/// </summary>
		/// <param name="transPrefab"></param>
		/// <returns></returns>
		public static int PrefabDespawnedCount(this Transform transPrefab) {
			return PoolBoss.PrefabDespawnedCount(transPrefab);
		}
		
		/// <summary>
		/// Calls the same named Pool Boss method
		/// </summary>
		/// <param name="trans"></param>
		/// <returns></returns>
		public static bool PrefabIsInPool(this Transform trans) {
			return PoolBoss.PrefabIsInPool(trans);
		}
		
		/// <summary>
		/// Calls the same named Pool Boss method
		/// </summary>
		/// <param name="transPrefab"></param>
		/// <returns></returns>
		public static int PrefabSpawnedCount(this Transform transPrefab) {
			return PoolBoss.PrefabSpawnedCount(transPrefab);
		}
		
		/// <summary>
		/// Calls the same named Pool Boss method
		/// </summary>
		/// <param name="transToSpawn"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public static Transform SpawnInPool(this Transform transToSpawn, Vector3 position, Quaternion rotation) {
			return PoolBoss.SpawnInPool(transToSpawn, position, rotation);
		}
		
		/// <summary>
		/// Calls the same named Pool Boss method
		/// </summary>
		/// <param name="transToSpawn"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public static Transform SpawnOutsidePool(this Transform transToSpawn, Vector3 position, Quaternion rotation) {
			return PoolBoss.SpawnOutsidePool(transToSpawn, position, rotation);
		}
		
		/// <summary>
		/// Calls the same named Pool Boss method
		/// </summary>
		/// <param name="transToSpawn"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <param name="parentTransform"></param>
		/// <returns></returns>
		public static Transform Spawn(this Transform transToSpawn, Vector3 position, Quaternion rotation, Transform parentTransform) {
			return PoolBoss.Spawn(transToSpawn, position, rotation, parentTransform);
		}

		
		/// <summary>
		/// Change the layer of something you just spawned.
		/// </summary>
		/// <param name="spawned"></param>
		/// <param name="layer"></param>
		public static Transform OnLayer(this Transform spawned, int layer) {
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7
            spawned.gameObject.layer = layer;
#else
            spawned.GetComponent<GameObject>().layer = layer;
#endif
            return spawned;
		}
		
		/// <summary>
		/// Change the local scale of something you just spawned.
		/// </summary>
		/// <param name="spawned"></param>
		/// <param name="newScale"></param>
		/// <returns></returns>
		public static Transform WithScale(this Transform spawned, Vector3 newScale) {
			spawned.localScale = newScale;
			return spawned;
		}
	}
}