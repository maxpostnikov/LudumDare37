using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MaxPostnikov.Utils
{
    public interface IPooled<T> where T : MonoBehaviour, IPooled<T>
    {
        int PrefabIndex { get; }

        void OnInstantiate(int prefabIndex, PrefabsPool<T> pool);

        void OnSpawn();

        void OnRecycle();

        void Recycle();
    }

    public class PrefabsPool<T> where T : MonoBehaviour, IPooled<T>
    {
        public int SpawnedCount { get { return spawned.Count; } }

        readonly IList<T> prefabs;
        readonly Transform container;

        List<T> spawned;
        Stack<T>[] recycled;
        
        public PrefabsPool(IList<T> prefabs, Transform parent = null, int preload = 1)
        {
            if (preload < 0)
                throw new ArgumentException("Negative preload!", "preload");

            this.prefabs = prefabs;
            
            container = new GameObject(string.Format("{0} Pool", ComponentName)).transform;
            if (parent != null)
                container.SetParent(parent);

            Init(preload);
        }

        void Init(int count)
        {
            spawned = new List<T>(prefabs.Count * count);
            recycled = new Stack<T>[prefabs.Count];

            for (var i = 0; i < prefabs.Count; i++) {
                recycled[i] = new Stack<T>(count);

                for (var j = 0; j < count; j++)
                    Spawn(i);
            } 

            RecycleAll();
        }
        
        public T Spawn(int prefabIndex = 0)
        {
            var stack = recycled[prefabIndex];
            var instance = stack.Count > 0 ? stack.Pop() : Instantiate(prefabIndex);

            spawned.Add(instance);

            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;

            instance.gameObject.SetActive(true);
            instance.OnSpawn();
            
            return instance;
        }

        public T SpawnRandom()
        {
            return Spawn(Random.Range(0, prefabs.Count));
        }

        public void Recycle(T instance)
        {
            if (!spawned.Contains(instance)) {
                Debug.LogError(string.Format("Instance {0} not spawned!", instance.name));
                return;
            }
            
            var stack = recycled[instance.PrefabIndex];

            stack.Push(instance);
            spawned.Remove(instance);

            instance.gameObject.SetActive(false);
            instance.OnRecycle();
        }

        public void RecycleAll()
        {
            for (var i = spawned.Count - 1; i >= 0; i--)
                Recycle(spawned[i]);
        }

        T Instantiate(int prefabIndex)
        {
            var instance = Object.Instantiate(prefabs[prefabIndex]);

            instance.transform.SetParent(container);
            instance.OnInstantiate(prefabIndex, this);

            return instance;
        }

        string ComponentName {
            get {
                var nameSplit = typeof(T).ToString().Split('.');

                return nameSplit[nameSplit.Length - 1];
            }
        }
    }
}
