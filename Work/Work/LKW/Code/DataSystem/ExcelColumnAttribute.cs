using System;

namespace Code.DataSystem
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ExcelColumnAttribute : Attribute
    {
        public string ColumnName { get;}
        public  bool IsRequired { get;}

        public ExcelColumnAttribute(string columnName, bool isRequired = false)
        {
            ColumnName = columnName;
            IsRequired = isRequired;
        }
    }
}