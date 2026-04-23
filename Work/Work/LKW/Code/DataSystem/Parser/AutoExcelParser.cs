using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using UnityEngine;

namespace Code.DataSystem
{
    public static class AutoExcelParser
    {
        private static Dictionary<Type, List<FieldInfo>> fieldCache = new Dictionary<Type, List<FieldInfo>>();
        private static Dictionary<Type, List<PropertyInfo>> propertyCache = new Dictionary<Type, List<PropertyInfo>>();

        public static void ParseRow(DataRow dataRow, int rowIndex, object instance)
        {
            Type type = instance.GetType();

            foreach (var field in GetCacheFields(type))
            {
                var attribute = field.GetCustomAttribute<ExcelColumnAttribute>();
                if (attribute == null)
                    continue;
                Debug.Log(attribute.ColumnName);

                object value = GetColumnValue(dataRow, rowIndex, attribute.ColumnName, field.FieldType, false);
                field.SetValue(instance, value);
            }
        }

        private static object GetColumnValue(
            DataRow row,
            int rowIndex,
            string columnName,
            Type targetType,
            bool isRequired)
        {
            if (!row.Table.Columns.Contains(columnName))
            {
                if (isRequired)
                {
                    throw new ArgumentException(
                        $"{rowIndex + 1}번째 행, 컬럼 '{columnName}'이 존재하지 않습니다.");
                }
                return GetDefaultValue(targetType);
            }

            var cellValue = row[columnName];

            if (cellValue == null || cellValue == DBNull.Value)
            {
                if (isRequired)
                {
                    throw new ArgumentException(
                        $"{rowIndex + 1}번째 행, 컬럼 '{columnName}'에 데이터가 존재하지 않습니다.");
                }
                return GetDefaultValue(targetType);
            }

            try
            {
                return ConvertValue(cellValue, targetType);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                throw new InvalidCastException(
                    $"{rowIndex + 1}번째 행, 컬럼 '{columnName}', 값 '{cellValue}'를 {targetType.Name} 타입으로 변환할 수 없습니다.");
            }
        }
        private static object ConvertValue(object value, Type targetType)
        {
            if (value == null)
                GetDefaultValue(targetType);

            if (targetType.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type underlyingType = Nullable.GetUnderlyingType(targetType);
                return ConvertValue(value, underlyingType);
            }

            string stringValue = value.ToString();
            if (targetType == typeof(string))
                return stringValue;
            if (targetType == typeof(int))
                return int.Parse(stringValue);
            if (targetType == typeof(float))
                return float.Parse(stringValue);
            if (targetType == typeof(bool))
                return bool.Parse(stringValue);
            if (targetType == typeof(double))
                return double.Parse(stringValue);
            if (targetType == typeof(long))
                return long.Parse(stringValue);
            if (targetType.IsEnum)
            {
                Debug.Log($"Enum Parse Try: '{stringValue}' → {targetType.Name}");
                if (stringValue.Contains(','))
                {
                    //여기서 flagEnum변환
                    var parts = stringValue.Trim().Split(',');

                    int result = 0;

                    foreach (var part in parts)
                    {
                        if (Enum.TryParse(targetType, part, true, out var partEnum))
                        {
                            result |= (int)partEnum;
                        }
                    }
                    return Enum.ToObject(targetType, result);

                }
                
                return Enum.Parse(targetType, stringValue.Trim(), ignoreCase: true);
            }
            return JsonUtility.FromJson(stringValue, targetType);
        }

        private static object GetDefaultValue(Type targetType)
        {
            if (targetType.IsValueType)
                return Activator.CreateInstance(targetType);

            return null;
        }

        private static List<FieldInfo> GetCacheFields(Type type)
        {
            if (!fieldCache.ContainsKey(type))
            {
                var fields = new List<FieldInfo>();
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (field.GetCustomAttributes<ExcelColumnAttribute>() != null)
                        fields.Add(field);
                }
                fieldCache[type] = fields;
            }

            return fieldCache[type];
        }

        private static List<PropertyInfo> GetCacheProperties(Type type)
        {
            if (!propertyCache.ContainsKey(type))
            {
                var properties = new List<PropertyInfo>();
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (property.GetCustomAttributes<ExcelColumnAttribute>() != null)
                    {
                        properties.Add(property);
                    }
                }

                propertyCache[type] = properties;
            }
            return propertyCache[type];
        }
    }
}