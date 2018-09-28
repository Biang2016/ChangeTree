using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
#endif

[AttributeUsage(AttributeTargets.Field)]
public class RenameAttribute : PropertyAttribute
{

    public string name;

    public RenameAttribute(string name)
    {
        this.name = name;
    }

}

#if UNITY_EDITOR
//重命名属性绘制器
[CustomPropertyDrawer(typeof(RenameAttribute))]
public class RenameDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //替换属性名称
        RenameAttribute rename = (RenameAttribute)attribute;
        label.text = rename.name;

        //重绘GUI
        SerializedPropertyType type = property.propertyType;
        if (type == SerializedPropertyType.Enum)
        {
            redrawEnums(position, property, label);
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }

    //枚举类型键值
    Dictionary<string, string> map = new Dictionary<string, string>();

    //重绘枚举类型
    private void redrawEnums(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        string[] names = property.enumNames;
        //获取所有标记rename的属性
        object[] attrs = fieldInfo.GetCustomAttributes(typeof(RenameAttribute), false);
        for (int i = 0; i < attrs.Length; i++)
        {
            Type field = fieldInfo.FieldType;
            //循环所有枚举名称
            for (int j = 0; j < names.Length; j++)
            {
                //不重复记录键值
                string name = names[j];
                if (map.ContainsKey(name)) continue;

                //属性为null 继续
                FieldInfo info = field.GetField(name);
                if (info == null) continue;

                //记录枚举键值
                RenameAttribute[] enumValues = (RenameAttribute[])info.GetCustomAttributes(typeof(RenameAttribute), false);
                string value = enumValues[0].name;
                map.Add(name, value);
            }
        }

        //重绘GUI
        List<string> list = new List<string>(map.Values);
        string[] values = list.ToArray();
        int index = EditorGUI.Popup(position, label.text, property.enumValueIndex, values);
        if (EditorGUI.EndChangeCheck() && index != -1) property.enumValueIndex = index;
    }

}
#endif