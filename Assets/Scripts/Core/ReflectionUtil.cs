using System.Reflection;

namespace SurvivalGame.Core
{
    /// <summary>
    /// Small helper used by the runtime bootstrap to assign serialized fields on
    /// procedurally-created components (the same job the Editor scene builder does).
    /// </summary>
    public static class ReflectionUtil
    {
        private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

        public static void Set(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, Flags);
            if (field != null) field.SetValue(target, value);
        }

        public static T Get<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, Flags);
            return field == null ? default(T) : (T)field.GetValue(target);
        }
    }
}
