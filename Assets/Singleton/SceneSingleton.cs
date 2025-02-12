using System.Reflection;
using UnityEngine;

namespace SingletonUtil {
    public class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour {
        private static T m_Instance;

        public static T Instance {
            get {
                if (m_Instance != null) return m_Instance;

                m_Instance ??= FindFirstObjectByType<T>();
                if (m_Instance == null) return m_Instance;

                typeof(T)
                    .GetMethod("OnTouched", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(Instance, null);

                return m_Instance;
            }
        }

        protected virtual void OnTouched() { }

        protected virtual void Awake() { }

        protected virtual void OnDestroy() { m_Instance = null; }
    }
}
