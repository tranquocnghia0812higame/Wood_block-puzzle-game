using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBehaviour : MonoBehaviour
{
    protected virtual void OnDestroy()
    {
        foreach (System.Reflection.FieldInfo field in GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
        {
            System.Type fieldType = field.FieldType;
 
            if (typeof(IList).IsAssignableFrom(fieldType))
            {
                IList list = field.GetValue(this) as IList;
                if (list != null)
                {
                    list.Clear();
                }
            }
 
            if (typeof(IDictionary).IsAssignableFrom(fieldType))
            {
                IDictionary dictionary = field.GetValue(this) as IDictionary;
                if (dictionary != null)
                {
                    dictionary.Clear();
                }
            }
 
            if (!fieldType.IsPrimitive)
            {
                field.SetValue(this, null);
            }
        }
    }
}
