using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
public abstract class UITemplate
{
    public GameObject mainGo { get; set; }
    private const string UINameSpace = "UnityEngine.UI";
    public UITemplate()
    {
    }
    public virtual void Destroy()
    {
        GameObject.Destroy(mainGo);
    }
    public UITemplate(GameObject go)
    {
        mainGo = go;
        Type t = GetType();

        FieldInfo[] fields = Logic.Utility.GetFieldInfos(t);
        List<Transform> list = Logic.Utility.GetChildren(go.transform);
        string g_name = "";
        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo fieldInfo = fields[i];
            var attribtues = fieldInfo.GetCustomAttributes(typeof(Logic.AssignValueAttribute), false);
            if (attribtues == null || attribtues.Length==0) continue;
            if (typeof(Component).IsAssignableFrom(fieldInfo.FieldType) && fieldInfo.FieldType.Namespace.Contains(UINameSpace))
            {
                g_name = fieldInfo.Name.Replace("_" + fieldInfo.FieldType.Name, "");
                Component component = GetUIComponent(g_name, list, fieldInfo.FieldType);
                if (component)
                {
                    fieldInfo.SetValue(this, component);
                }
            }
            else if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType) && fieldInfo.FieldType.IsGenericType)
            {
                Type type = fieldInfo.FieldType.GetGenericArguments().First();
                object value = Activator.CreateInstance(fieldInfo.FieldType);
                fieldInfo.SetValue(this,value);
                if (typeof(Component).IsAssignableFrom(type) && fieldInfo.FieldType.Namespace.Contains(UINameSpace))
                {
                    g_name = fieldInfo.Name.Replace("_" + fieldInfo.FieldType.Name, "");
                    List<Component> components = GetUIComponents(g_name, list, type);
                    for (int j = 0; j < components.Count; j++)
                    {
                        (value as IList).Add(components[j]);
                    }
                }
                else if(type == typeof(GameObject))
                {
                    List<GameObject> gos = GetGameObjects(fieldInfo.Name, list);
                    for (int j = 0; j < gos.Count; j++)
                    {
                        (value as IList).Add(gos[j]);
                    }
                }
            }
        }
    }

    List<GameObject> GetGameObjects(string name, List<Transform> list)
    {
        List<GameObject> gos = new List<GameObject>(10);
        for (int j = 0; j < list.Count; j++)
        {
            if (list[j].transform.name == name)
                gos.Add(list[j].gameObject);
        }
        return gos;
    }
    List<Component> GetUIComponents(string name, List<Transform> list, Type type)
    {
        List<Component> components = new List<Component>(10);
        for (int j = 0; j < list.Count; j++)
        {
            if (list[j].transform.name == name)
            {
                Component component = list[j].GetComponent(type);
                if (component)
                {
                    components.Add(component);
                }
            }
        }
        return components;
    }
    Component GetUIComponent(string name, List<Transform> list, Type type)
    {
        for (int j = 0; j < list.Count; j++)
        {
            if (list[j].transform.name == name)
            {
                Component component = list[j].GetComponent(type);
                return component;
            }
        }
        return null;
    }
}
