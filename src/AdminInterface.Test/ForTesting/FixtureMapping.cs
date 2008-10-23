using System;
using System.Collections.Generic;
using WatiN.Core;

namespace AdminInterface.Test.ForTesting
{
    public class As
    {
        public static Func<Element, bool> Checkbox(object value)
        {
            return e => ((CheckBox)e).Checked == Convert.ToBoolean(value);
        }
    }
    public class FixtureMapping
    {
        private static readonly Dictionary<Type, Mapping> mappings = new Dictionary<Type, Mapping>();

        public abstract class Mapping
        {
            public abstract object GetValue(object value, string alias);
            public abstract IEnumerable<string> Headers();
        }

        public class Mapping<T> : Mapping
        {
            protected Dictionary<string, Func<T, object>> _map = new Dictionary<string, Func<T, object>>();

            public Mapping<T> To(Func<T, object> map, string value)
            {
                _map.Add(value, map);
                return this;
            }

            public override object GetValue(object value, string alias)
            {
                if (!_map.ContainsKey(alias))
                    throw new Exception("Нет мапинга для алиаса: " + alias);

                return _map[alias]((T) value);
            }

            public override IEnumerable<string> Headers()
            {
                return _map.Keys;
            }
        }

        public static Mapping<T> For<T>()
        {
            var mapping = new Mapping<T>();
            mappings.Add(typeof(T), mapping);
            return mapping; 
        }

        public static IEnumerable<string> Headers<T>()
        {
            if (!mappings.ContainsKey(typeof(T)))
                throw new Exception("Нет меппинга для " + typeof(T));

            return mappings[typeof (T)].Headers();
        }

        public static object GetValue<T>(T value, string alias)
        {
            if (!mappings.ContainsKey(typeof(T)))
                throw new Exception("Нет меппинга для " + typeof(T));
            return mappings[typeof(T)].GetValue(value, alias);
        }
    }
}
