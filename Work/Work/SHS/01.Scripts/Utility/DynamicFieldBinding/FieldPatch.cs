using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Sirenix.Serialization;
using UnityEngine;

namespace Code.SHS.Utility.DynamicFieldBinding
{
    public interface IFieldPatchRuntime
    {
        UnityEngine.Object TargetObject { get; }
        IReadOnlyList<FieldInfo> GetMutableFields();
        void SyncInputs();
        void GenerateSetter();
        void ApplySetter();
    }

    public enum FieldPatchValueKind
    {
        Unsupported = 0,
        Int = 1,
        Float = 2,
        Double = 3,
        Long = 4,
        Bool = 5,
        String = 6,
        Vector2 = 7,
        Vector3 = 8,
        Vector4 = 9,
        Color = 10,
        Enum = 11,
        ObjectReference = 12,
        ManagedReference = 13,
        List = 14,
    }

    [Serializable]
    public sealed class FieldPatchValue
    {
        [SerializeField] private string _fieldKey;
        [SerializeField] private string _fieldName;
        [SerializeField] private FieldPatchValueKind _valueKind;
        [SerializeField] private bool _hasOverride;

        [SerializeField] private int _intValue;
        [SerializeField] private float _floatValue;
        [SerializeField] private double _doubleValue;
        [SerializeField] private long _longValue;
        [SerializeField] private bool _boolValue;
        [SerializeField] private string _stringValue;
        [SerializeField] private Vector2 _vector2Value;
        [SerializeField] private Vector3 _vector3Value;
        [SerializeField] private Vector4 _vector4Value;
        [SerializeField] private Color _colorValue = Color.white;
        [SerializeField] private int _enumValue;
        [SerializeField] private UnityEngine.Object _objectValue;
        [SerializeField] private byte[] _managedReferenceData;

        [NonSerialized] private object _managedReferenceCache;
        [NonSerialized] private bool _hasManagedReferenceCache;

        public string FieldKey => _fieldKey;
        public string FieldName => _fieldName;
        public FieldPatchValueKind ValueKind => _valueKind;
        public bool HasOverride => _hasOverride;

        public void SetMetadata(string fieldKey, string fieldName, FieldPatchValueKind valueKind)
        {
            _fieldKey = fieldKey;
            _fieldName = fieldName;
            _valueKind = valueKind;
        }

        public void SetOverride(bool hasOverride)
        {
            _hasOverride = hasOverride;
        }

        public void SetFromObject(object value, Type fieldType)
        {
            switch (_valueKind)
            {
                case FieldPatchValueKind.Int:
                    _intValue = value is int intValue ? intValue : default;
                    break;
                case FieldPatchValueKind.Float:
                    _floatValue = value is float floatValue ? floatValue : default;
                    break;
                case FieldPatchValueKind.Double:
                    _doubleValue = value is double doubleValue ? doubleValue : default;
                    break;
                case FieldPatchValueKind.Long:
                    _longValue = value is long longValue ? longValue : default;
                    break;
                case FieldPatchValueKind.Bool:
                    _boolValue = value is bool boolValue && boolValue;
                    break;
                case FieldPatchValueKind.String:
                    _stringValue = value as string;
                    break;
                case FieldPatchValueKind.Vector2:
                    _vector2Value = value is Vector2 vector2Value ? vector2Value : default;
                    break;
                case FieldPatchValueKind.Vector3:
                    _vector3Value = value is Vector3 vector3Value ? vector3Value : default;
                    break;
                case FieldPatchValueKind.Vector4:
                    _vector4Value = value is Vector4 vector4Value ? vector4Value : default;
                    break;
                case FieldPatchValueKind.Color:
                    _colorValue = value is Color colorValue ? colorValue : Color.white;
                    break;
                case FieldPatchValueKind.Enum:
                    _enumValue = value != null ? Convert.ToInt32(value) : 0;
                    break;
                case FieldPatchValueKind.ObjectReference:
                    _objectValue = value as UnityEngine.Object;
                    break;
                case FieldPatchValueKind.ManagedReference:
                    SetManagedReference(value);
                    break;
                case FieldPatchValueKind.List:
                    SetManagedReference(value);
                    break;
            }
        }

        public object ReadOrThrow(Type fieldType)
        {
            switch (_valueKind)
            {
                case FieldPatchValueKind.Int:
                    return _intValue;
                case FieldPatchValueKind.Float:
                    return _floatValue;
                case FieldPatchValueKind.Double:
                    return _doubleValue;
                case FieldPatchValueKind.Long:
                    return _longValue;
                case FieldPatchValueKind.Bool:
                    return _boolValue;
                case FieldPatchValueKind.String:
                    return _stringValue;
                case FieldPatchValueKind.Vector2:
                    return _vector2Value;
                case FieldPatchValueKind.Vector3:
                    return _vector3Value;
                case FieldPatchValueKind.Vector4:
                    return _vector4Value;
                case FieldPatchValueKind.Color:
                    return _colorValue;
                case FieldPatchValueKind.Enum:
                {
                    Type enumUnderlyingType = Enum.GetUnderlyingType(fieldType);
                    object enumRawValue = Convert.ChangeType(_enumValue, enumUnderlyingType);
                    return Enum.ToObject(fieldType, enumRawValue);
                }
                case FieldPatchValueKind.ObjectReference:
                {
                    if (_objectValue == null)
                    {
                        return null;
                    }

                    if (fieldType.IsInstanceOfType(_objectValue))
                    {
                        return _objectValue;
                    }

                    throw new InvalidOperationException(
                        $"'{_fieldName}' expects '{fieldType.Name}', but input is '{_objectValue.GetType().Name}'.");
                }
                case FieldPatchValueKind.ManagedReference:
                {
                    object managedReferenceValue = GetManagedReferenceValue();
                    if (managedReferenceValue == null)
                    {
                        return null;
                    }

                    if (fieldType.IsInstanceOfType(managedReferenceValue))
                    {
                        return managedReferenceValue;
                    }

                    throw new InvalidOperationException(
                        $"'{_fieldName}' expects '{fieldType.Name}', but input is '{managedReferenceValue.GetType().Name}'.");
                }
                case FieldPatchValueKind.List:
                {
                    object listValue = GetManagedReferenceValue();
                    if (listValue == null)
                    {
                        return null;
                    }

                    if (fieldType.IsInstanceOfType(listValue))
                    {
                        return listValue;
                    }

                    throw new InvalidOperationException(
                        $"'{_fieldName}' expects '{fieldType.Name}', but input is '{listValue.GetType().Name}'.");
                }
                default:
                    throw new NotSupportedException($"'{_fieldName}' type is unsupported.");
            }
        }

        public object ReadClonedOrThrow(Type fieldType)
        {
            switch (_valueKind)
            {
                case FieldPatchValueKind.ManagedReference:
                case FieldPatchValueKind.List:
                {
                    object clonedValue = DeserializeManagedReferenceValue();
                    if (clonedValue == null)
                    {
                        return null;
                    }

                    if (fieldType.IsInstanceOfType(clonedValue))
                    {
                        return clonedValue;
                    }

                    throw new InvalidOperationException(
                        $"'{_fieldName}' expects '{fieldType.Name}', but input is '{clonedValue.GetType().Name}'.");
                }
                default:
                    return ReadOrThrow(fieldType);
            }
        }

        public object GetManagedReferenceValue()
        {
            if (_hasManagedReferenceCache)
            {
                return _managedReferenceCache;
            }

            if (_managedReferenceData == null || _managedReferenceData.Length == 0)
            {
                _managedReferenceCache = null;
                _hasManagedReferenceCache = true;
                return null;
            }

            _managedReferenceCache = DeserializeManagedReferenceValue();
            _hasManagedReferenceCache = true;
            return _managedReferenceCache;
        }

        public void SetManagedReference(object value)
        {
            _managedReferenceCache = value;
            _hasManagedReferenceCache = true;
            _managedReferenceData = value == null
                ? null
                : SerializationUtility.SerializeValueWeak(value, DataFormat.Binary, (SerializationContext)null);
        }

        private object DeserializeManagedReferenceValue()
        {
            if (_managedReferenceData == null || _managedReferenceData.Length == 0)
            {
                return null;
            }

            return SerializationUtility.DeserializeValueWeak(
                _managedReferenceData,
                DataFormat.Binary,
                (DeserializationContext)null);
        }
    }

    [Serializable]
    public sealed class FieldPatch<T> : IFieldPatchRuntime where T : UnityEngine.Object
    {
        [SerializeField] private T _target;
        [SerializeField] private List<FieldPatchValue> _inputs = new List<FieldPatchValue>();

        [NonSerialized] private Action<T> _generatedSetter;
        [NonSerialized] private Type _generatedRuntimeType;

        public T Value => _target;
        public T Target => _target;
        UnityEngine.Object IFieldPatchRuntime.TargetObject => _target;

        public IReadOnlyList<FieldInfo> GetMutableFields()
        {
            return _target == null
                ? Array.Empty<FieldInfo>()
                : FieldPatchUtility.GetMutableFields(_target.GetType());
        }

        public void SyncInputs()
        {
            _generatedSetter = null;
            _generatedRuntimeType = null;

            if (_target == null)
            {
                _inputs.Clear();
                return;
            }

            IReadOnlyList<FieldInfo> fields = FieldPatchUtility.GetMutableFields(_target.GetType());
            Dictionary<string, FieldPatchValue> existingByKey = new Dictionary<string, FieldPatchValue>(_inputs.Count);

            for (int i = 0; i < _inputs.Count; i++)
            {
                FieldPatchValue input = _inputs[i];
                if (input == null || string.IsNullOrEmpty(input.FieldKey))
                {
                    continue;
                }

                existingByKey[input.FieldKey] = input;
            }

            List<FieldPatchValue> nextInputs = new List<FieldPatchValue>(fields.Count);

            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo field = fields[i];
                string fieldKey = FieldPatchUtility.GetFieldKey(field);
                string fieldName = FieldPatchUtility.GetDisplayName(field);
                FieldPatchValueKind valueKind = FieldPatchUtility.GetValueKind(field);

                if (!existingByKey.TryGetValue(fieldKey, out FieldPatchValue input) || input == null)
                {
                    input = new FieldPatchValue();
                    input.SetMetadata(fieldKey, fieldName, valueKind);
                    input.SetOverride(false);
                }
                else
                {
                    bool hasSameKind = input.ValueKind == valueKind;
                    input.SetMetadata(fieldKey, fieldName, valueKind);

                    if (!hasSameKind)
                    {
                        input.SetOverride(false);
                    }
                }

                if (!input.HasOverride)
                {
                    input.SetFromObject(field.GetValue(_target), field.FieldType);
                }

                nextInputs.Add(input);
            }

            _inputs = nextInputs;
        }

        public void GenerateSetter()
        {
            _generatedSetter = null;
            _generatedRuntimeType = null;

            if (_target == null)
            {
                throw new InvalidOperationException("Target is null. Drag and drop an object first.");
            }

            SyncInputs();

            IReadOnlyList<FieldInfo> fields = FieldPatchUtility.GetMutableFields(_target.GetType());
            Dictionary<string, FieldPatchValue> inputByKey = new Dictionary<string, FieldPatchValue>(_inputs.Count);

            for (int i = 0; i < _inputs.Count; i++)
            {
                FieldPatchValue input = _inputs[i];
                if (input == null)
                {
                    continue;
                }

                inputByKey[input.FieldKey] = input;
            }

            Type runtimeTargetType = _target.GetType();
            ParameterExpression targetParam = Expression.Parameter(typeof(T), "target");
            Expression runtimeTarget = runtimeTargetType == typeof(T)
                ? targetParam
                : Expression.Convert(targetParam, runtimeTargetType);

            List<Expression> assignments = new List<Expression>();
            int skippedCount = 0;

            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo field = fields[i];
                string fieldKey = FieldPatchUtility.GetFieldKey(field);

                if (!inputByKey.TryGetValue(fieldKey, out FieldPatchValue input) || input == null)
                {
                    continue;
                }

                if (!input.HasOverride)
                {
                    continue;
                }

                if (input.ValueKind == FieldPatchValueKind.Unsupported)
                {
                    skippedCount++;
                    continue;
                }

                object nextValue = input.ReadOrThrow(field.FieldType);

                Expression fieldOwner = field.DeclaringType == runtimeTarget.Type
                    ? runtimeTarget
                    : Expression.Convert(runtimeTarget, field.DeclaringType);

                MemberExpression fieldAccess = Expression.Field(fieldOwner, field);
                Expression valueExpression = Expression.Constant(nextValue, field.FieldType);
                assignments.Add(Expression.Assign(fieldAccess, valueExpression));
            }

            if (assignments.Count == 0)
            {
                if (skippedCount > 0)
                {
                    throw new NotSupportedException(
                        "No supported fields to generate. All detected fields are unsupported by FieldPatch.");
                }

                _generatedSetter = _ => { };
                _generatedRuntimeType = runtimeTargetType;
                return;
            }

            BlockExpression body = Expression.Block(assignments);
            _generatedSetter = Expression.Lambda<Action<T>>(body, targetParam).Compile();
            _generatedRuntimeType = runtimeTargetType;
        }

        public void ApplySetter()
        {
            ApplySetter(_target);
        }

        public void ApplySetter(T target)
        {
            if (target == null)
            {
                throw new InvalidOperationException("Target is null.");
            }

            if (_generatedSetter == null || _generatedRuntimeType == null)
            {
                throw new InvalidOperationException("Setter is not generated. Run Generate Setter first.");
            }

            Type targetRuntimeType = target.GetType();
            if (targetRuntimeType != _generatedRuntimeType)
            {
                throw new InvalidOperationException(
                    $"Generated setter type is '{_generatedRuntimeType.Name}', but target type is '{targetRuntimeType.Name}'. Generate again with matching type.");
            }

            _generatedSetter.Invoke(target);
            ApplyReferenceOverrides(target);
        }

        private void ApplyReferenceOverrides(T target)
        {
            if (target == null || _inputs == null || _inputs.Count == 0)
            {
                return;
            }

            IReadOnlyList<FieldInfo> fields = FieldPatchUtility.GetMutableFields(target.GetType());
            if (fields.Count == 0)
            {
                return;
            }

            Dictionary<string, FieldPatchValue> inputByKey = new Dictionary<string, FieldPatchValue>(_inputs.Count);
            for (int i = 0; i < _inputs.Count; i++)
            {
                FieldPatchValue input = _inputs[i];
                if (input == null)
                {
                    continue;
                }

                inputByKey[input.FieldKey] = input;
            }

            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo field = fields[i];
                string fieldKey = FieldPatchUtility.GetFieldKey(field);
                if (!inputByKey.TryGetValue(fieldKey, out FieldPatchValue input)
                    || input == null
                    || !input.HasOverride)
                {
                    continue;
                }

                if (input.ValueKind != FieldPatchValueKind.ManagedReference
                    && input.ValueKind != FieldPatchValueKind.List)
                {
                    continue;
                }

                object clonedValue = input.ReadClonedOrThrow(field.FieldType);
                field.SetValue(target, clonedValue);
            }
        }

        public static bool operator ==(FieldPatch<T> left, FieldPatch<T> right)
        {
            bool leftIsNull = IsNullOrEmpty(left);
            bool rightIsNull = IsNullOrEmpty(right);

            if (leftIsNull || rightIsNull)
            {
                return leftIsNull == rightIsNull;
            }

            return left._target == right._target;
        }

        public static bool operator !=(FieldPatch<T> left, FieldPatch<T> right)
        {
            return !(left == right);
        }

        public static bool operator ==(FieldPatch<T> left, T right)
        {
            if (IsNullOrEmpty(left))
            {
                return right == null;
            }

            return left._target == right;
        }

        public static bool operator !=(FieldPatch<T> left, T right)
        {
            return !(left == right);
        }

        public static bool operator ==(T left, FieldPatch<T> right)
        {
            return right == left;
        }

        public static bool operator !=(T left, FieldPatch<T> right)
        {
            return !(right == left);
        }

        public override bool Equals(object obj)
        {
            if (obj is FieldPatch<T> otherPatch)
            {
                return this == otherPatch;
            }

            if (obj is T otherTarget)
            {
                return this == otherTarget;
            }

            return obj == null && this == null;
        }

        public override int GetHashCode()
        {
            return _target == null ? 0 : _target.GetHashCode();
        }

        public static implicit operator T(FieldPatch<T> fieldPatch)
        {
            return fieldPatch is null ? null : fieldPatch._target;
        }

        private static bool IsNullOrEmpty(FieldPatch<T> fieldPatch)
        {
            return ReferenceEquals(fieldPatch, null) || fieldPatch._target == null;
        }
    }

    public static class FieldPatchUtility
    {
        private const BindingFlags ReflectionFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private static readonly Dictionary<Type, IReadOnlyList<FieldInfo>> MutableFieldCache =
            new Dictionary<Type, IReadOnlyList<FieldInfo>>();

        public static IReadOnlyList<FieldInfo> GetMutableFields(Type targetType)
        {
            if (targetType == null)
            {
                return Array.Empty<FieldInfo>();
            }

            if (MutableFieldCache.TryGetValue(targetType, out IReadOnlyList<FieldInfo> cached))
            {
                return cached;
            }

            List<FieldInfo> fields = new List<FieldInfo>();
            Type current = targetType;

            while (current != null && current != typeof(object))
            {
                FieldInfo[] declared = current.GetFields(ReflectionFlags);
                for (int i = 0; i < declared.Length; i++)
                {
                    FieldInfo field = declared[i];
                    if (!IsMutableField(field))
                    {
                        continue;
                    }

                    fields.Add(field);
                }

                current = current.BaseType;
            }

            MutableFieldCache[targetType] = fields;
            return fields;
        }

        public static string GetFieldKey(FieldInfo field)
        {
            return $"{field.DeclaringType?.FullName}.{field.Name}";
        }

        public static string GetDisplayName(FieldInfo field)
        {
            const string backingFieldSuffix = ">k__BackingField";
            string rawName = field.Name;

            if (rawName.StartsWith("<", StringComparison.Ordinal) &&
                rawName.EndsWith(backingFieldSuffix, StringComparison.Ordinal))
            {
                int endIndex = rawName.IndexOf('>');
                if (endIndex > 1)
                {
                    return rawName.Substring(1, endIndex - 1);
                }
            }

            return rawName;
        }

        public static FieldPatchValueKind GetValueKind(Type fieldType)
        {
            if (fieldType == typeof(int)) return FieldPatchValueKind.Int;
            if (fieldType == typeof(float)) return FieldPatchValueKind.Float;
            if (fieldType == typeof(double)) return FieldPatchValueKind.Double;
            if (fieldType == typeof(long)) return FieldPatchValueKind.Long;
            if (fieldType == typeof(bool)) return FieldPatchValueKind.Bool;
            if (fieldType == typeof(string)) return FieldPatchValueKind.String;
            if (fieldType == typeof(Vector2)) return FieldPatchValueKind.Vector2;
            if (fieldType == typeof(Vector3)) return FieldPatchValueKind.Vector3;
            if (fieldType == typeof(Vector4)) return FieldPatchValueKind.Vector4;
            if (fieldType == typeof(Color)) return FieldPatchValueKind.Color;
            if (fieldType.IsEnum) return FieldPatchValueKind.Enum;
            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType)) return FieldPatchValueKind.ObjectReference;

            return FieldPatchValueKind.Unsupported;
        }

        public static FieldPatchValueKind GetValueKind(FieldInfo field)
        {
            if (field == null)
            {
                return FieldPatchValueKind.Unsupported;
            }

            FieldPatchValueKind valueKind = GetValueKind(field.FieldType);
            if (valueKind != FieldPatchValueKind.Unsupported)
            {
                return valueKind;
            }

            if (TryGetSupportedListElementType(field.FieldType, out _))
            {
                return FieldPatchValueKind.List;
            }

            if (field.IsDefined(typeof(SerializeReference), true) && IsManagedReferenceType(field.FieldType))
            {
                return FieldPatchValueKind.ManagedReference;
            }

            return FieldPatchValueKind.Unsupported;
        }

        private static bool IsMutableField(FieldInfo field)
        {
            if (field == null)
            {
                return false;
            }

            if (field.IsStatic || field.IsLiteral || field.IsInitOnly)
            {
                return false;
            }

            if (field.IsDefined(typeof(NonSerializedAttribute), true))
            {
                return false;
            }

            if (field.IsDefined(typeof(HideInInspector), true))
            {
                return false;
            }

            bool hasSerializeField = field.IsDefined(typeof(SerializeField), true);
            bool hasSerializeReference = field.IsDefined(typeof(SerializeReference), true);
            bool isCompilerGenerated =
                field.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true);

            bool isInspectorVisible = field.IsPublic || hasSerializeField || hasSerializeReference;
            if (!isInspectorVisible)
            {
                return false;
            }

            if (isCompilerGenerated && !hasSerializeField && !hasSerializeReference)
            {
                return false;
            }

            return true;
        }

        private static bool IsManagedReferenceType(Type fieldType)
        {
            if (fieldType == null)
            {
                return false;
            }

            return !fieldType.IsValueType
                   && fieldType != typeof(string)
                   && !typeof(UnityEngine.Object).IsAssignableFrom(fieldType);
        }

        public static bool TryGetSupportedListElementType(Type fieldType, out Type elementType)
        {
            elementType = null;
            if (fieldType == null
                || !fieldType.IsGenericType
                || fieldType.GetGenericTypeDefinition() != typeof(List<>))
            {
                return false;
            }

            Type candidateElementType = fieldType.GetGenericArguments()[0];
            if (candidateElementType == null)
            {
                return false;
            }

            if (candidateElementType == typeof(LayerMask))
            {
                elementType = candidateElementType;
                return true;
            }

            if (GetValueKind(candidateElementType) == FieldPatchValueKind.Unsupported)
            {
                return false;
            }

            elementType = candidateElementType;
            return true;
        }
    }
}
