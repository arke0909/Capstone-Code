using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Scripts.SkillSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.SkillSystem.Upgrade
{
    public enum UpgradeType
    {
        FieldUpdate = 0,
        MethodCall = 1,
    }

    public enum FieldType
    {
        Float = 0,
        Int32 = 1,
        Boolean = 2
    }

    [CreateAssetMenu(fileName = "Skill Upgrade", menuName = "SO/Skill/Upgrade", order = 0)]
    public class SkillUpgradeSO : ScriptableObject
    {
        public Sprite upgradeIcon;
        public string UpgradeTitle;

        [TextArea] public string upgradeDescription;
        public int maxUpgradeCnt = 1;

        public BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        [HideInInspector] public string targetSkillName;
        [HideInInspector] public UpgradeType UpgradeType;

        [HideInInspector] public string fieldName;
        [HideInInspector] public FieldType fieldType;
        [HideInInspector] public float floatValue;
        [HideInInspector] public int intValue;

        [HideInInspector] public string upgradeMethodName;
        [HideInInspector] public string upgradeParams;
        [HideInInspector] public string rollbackMethodName;

        [FormerlySerializedAs("rollbackPrams")] [HideInInspector]
        public string rollbackParams;

        private Skill _skillInstance;
        [NonSerialized] private Action<Skill> _fieldUpgradeAction;
        [NonSerialized] private Action<Skill> _fieldRollbackAction;
        [NonSerialized] private Action<Skill> _methodUpgradeAction;
        [NonSerialized] private Action<Skill> _methodRollbackAction;

        private void OnEnable()
        {
            InitializeUpgrade();
        }

        public string InitializeUpgrade()
        {
            ClearCache();

            if (string.IsNullOrEmpty(targetSkillName)) return "fail: Target skill name is empty";

            try
            {
                if (UpgradeType == UpgradeType.FieldUpdate)
                    return FieldUpdaterFactory();
                if (UpgradeType == UpgradeType.MethodCall)
                    return MethodUpdaterFactory();
            }
            catch (Exception e)
            {
                return $"fail: {e.Message}";
            }

            return "Unknown upgrade type";
        }

        private void ClearCache()
        {
            _fieldUpgradeAction = null;
            _fieldRollbackAction = null;
            _methodUpgradeAction = null;
            _methodRollbackAction = null;
        }


        private string FieldUpdaterFactory()
        {
            Type skillParentType = typeof(Skill);
            Type skillType = GetTargetType(targetSkillName);

            if (skillType == null) return $"fail : Type [{targetSkillName}] not found";
            if (string.IsNullOrEmpty(fieldName)) return "fail : field name is null";

            FieldInfo targetField = skillType.GetField(fieldName, bindingFlags);
            if (targetField == null) return $"fail : field [{fieldName}] not found in {skillType.Name}";

            ParameterExpression skillUpgradeParam = Expression.Parameter(skillParentType, "skill");
            UnaryExpression casterParam = Expression.Convert(skillUpgradeParam, skillType);
            MemberExpression fieldAccess = Expression.Field(casterParam, targetField);

            Expression upgradeExpression = null;
            Expression rollbackExpression = null;

            switch (fieldType)
            {
                case FieldType.Float:
                    upgradeExpression = Expression.Add(fieldAccess, Expression.Constant(floatValue));
                    rollbackExpression = Expression.Subtract(fieldAccess, Expression.Constant(floatValue));
                    break;
                case FieldType.Int32:
                    upgradeExpression = Expression.Add(fieldAccess, Expression.Constant(intValue));
                    rollbackExpression = Expression.Subtract(fieldAccess, Expression.Constant(intValue));
                    break;
                case FieldType.Boolean:
                    upgradeExpression = Expression.Constant(true);
                    rollbackExpression = Expression.Constant(false);
                    break;
            }

            _fieldUpgradeAction = Expression
                .Lambda<Action<Skill>>(Expression.Assign(fieldAccess, upgradeExpression), skillUpgradeParam).Compile();
            _fieldRollbackAction = Expression
                .Lambda<Action<Skill>>(Expression.Assign(fieldAccess, rollbackExpression), skillUpgradeParam).Compile();

            return "Success";
        }

        private Type GetTargetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;
            var type = Type.GetType(typeName);
            if (type != null) return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null) return type;

                string simpleName = typeName.Split(',')[0];
                type = assembly.GetType(simpleName);
                if (type != null) return type;
            }

            return null;
        }

        private string MethodUpdaterFactory()
        {
            Type skillParentType = typeof(Skill);
            Type skillType = Type.GetType(targetSkillName);

            if (string.IsNullOrEmpty(upgradeMethodName) || string.IsNullOrEmpty(rollbackMethodName))
            {
                return "fail : string is null";
            }

            MethodInfo upgradeMethod = skillType.GetMethod(upgradeMethodName, bindingFlags);
            MethodInfo rollbackMethod = skillType.GetMethod(rollbackMethodName, bindingFlags);

            if (upgradeMethod == null || rollbackMethod == null)
                return "fail : method is null";

            ParameterExpression skillParam = Expression.Parameter(skillParentType, "skill");
            UnaryExpression casterParam = Expression.Convert(skillParam, skillType);

            try
            {
                Expression[] upgradeParamExpressions = GetMethodParameters(upgradeMethod, upgradeParams);
                Expression[] rollbackParamExpressions = GetMethodParameters(rollbackMethod, rollbackParams);

                MethodCallExpression upgradeCall = Expression.Call(casterParam, upgradeMethod, upgradeParamExpressions);
                MethodCallExpression rollbackCall =
                    Expression.Call(casterParam, rollbackMethod, rollbackParamExpressions);

                _methodUpgradeAction = Expression.Lambda<Action<Skill>>(upgradeCall, skillParam).Compile();
                _methodRollbackAction = Expression.Lambda<Action<Skill>>(rollbackCall, skillParam).Compile();
            }
            catch (Exception e)
            {
                return $"{e} Error in method call factory";
            }

            return "success";
        }

        private Expression[] GetMethodParameters(MethodInfo method, string inputParam)
        {
            string[] paramValues = inputParam.Split(',').Select(param => param.Trim()).ToArray();
            ParameterInfo[] requiredParams = method.GetParameters();

            Debug.Assert(requiredParams.Length == 0 || requiredParams.Length == paramValues.Length,
                " Parameter count miss match");

            Expression[] args = new Expression[requiredParams.Length];

            for (int i = 0; i < requiredParams.Length; ++i)
            {
                Type paramType = requiredParams[i].ParameterType;
                object convertValue = Convert.ChangeType(paramValues[i], paramType);
                args[i] = Expression.Constant(convertValue, paramType);
            }

            return args;
        }


        #region Runtime section

        public void UpgradeSkill(Skill skill)
        {
            if (UpgradeType == UpgradeType.FieldUpdate && _fieldUpgradeAction == null) InitializeUpgrade();
            if (UpgradeType == UpgradeType.MethodCall && _methodUpgradeAction == null) InitializeUpgrade();

            if (UpgradeType == UpgradeType.FieldUpdate)
                _fieldUpgradeAction?.Invoke(skill);
            else if (UpgradeType == UpgradeType.MethodCall)
                _methodUpgradeAction?.Invoke(skill);
        }

        public void RollbackSkill(Skill skill)
        {
            if (UpgradeType == UpgradeType.FieldUpdate && _fieldRollbackAction == null) InitializeUpgrade();
            if (UpgradeType == UpgradeType.MethodCall && _methodRollbackAction == null) InitializeUpgrade();

            if (UpgradeType == UpgradeType.FieldUpdate)
                _fieldRollbackAction?.Invoke(skill);
            else if (UpgradeType == UpgradeType.MethodCall)
                _methodRollbackAction?.Invoke(skill);
        }

        #endregion
    }
}