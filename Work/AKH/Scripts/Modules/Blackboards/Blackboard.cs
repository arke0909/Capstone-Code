using System.Collections.Generic;

namespace Scripts.Modules.Blackboards
{
    public class Blackboard
    {
        private readonly Dictionary<string, object> _datas = new();

        public void Set<T>(string key, T value)
        {
            _datas[key] = value;
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (_datas.TryGetValue(key, out object obj) && obj is T casted)
            {
                value = casted;
                return true;
            }

            value = default;
            return false;
        }

        public T GetOrDefault<T>(string key)
        {
            return TryGet<T>(key, out T value) ? value : default;
        }

        public bool Remove(string key)
        {
            return _datas.Remove(key);
        }

        public void Clear()
        {
            _datas.Clear();
        }
    }
}
