// ***********************************************************************
// Assembly         : Infrastructure
// Author           : Yubao Li
// Created          : 11-23-2015
//
// Last Modified By : Yubao Li
// Last Modified On : 11-23-2015
// ***********************************************************************
// <copyright file="ObjectHelper.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary>
//对象COPY/初始化帮助，通常是防止从视图中传过来的对象属性为空，这其赋初始值
//</summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Infrastructure.Helpers
{
    public static  class ObjectHelper<T> where T : new()
    {
        //public static T CopyTo<T>(this object source) where T:class, new()
        //{
        //    var result = new T();
        //    source.CopyTo(result);
        //    return result;
        //}

        //public static void CopyTo<T>(this object source, T target)
        //    where T : class,new()
        //{
        //    if (source == null)
        //        return;

        //    if (target == null)
        //    {
        //        target = new T();
        //    }

        //    foreach (var property in target.GetType().GetProperties())
        //    {
        //        var propertyValue = source.GetType().GetProperty(property.Name).GetValue(source, null);
        //        if (propertyValue != null)
        //        {
        //            if (propertyValue.GetType().IsClass)
        //            {

        //            }
        //            target.GetType().InvokeMember(property.Name, BindingFlags.SetProperty, null, target, new object[] { propertyValue });
        //        }

        //    }

        //    foreach (var field in target.GetType().GetFields())
        //    {
        //        var fieldValue = source.GetType().GetField(field.Name).GetValue(source);
        //        if (fieldValue != null)
        //        {
        //            target.GetType().InvokeMember(field.Name, BindingFlags.SetField, null, target, new object[] { fieldValue });
        //        }
        //    }
        //}

        public static T ConvertToEntity(DataTable dt)
        {
            // 定义集合    
            T t = new T();

            if (dt == null)
            {
                return t;
            }

            // 获得此模型的类型   
            Type type = typeof(T);
            string tempName = "";

            DataRow dr = dt.Rows[0];

            // 获得此模型的公共属性      
            PropertyInfo[] propertys = t.GetType().GetProperties();
            foreach (PropertyInfo pi in propertys)
            {
                tempName = pi.Name;  // 检查DataTable是否包含此列    

                if (dt.Columns.Contains(tempName))
                {
                    // 判断此属性是否有Setter      
                    if (!pi.CanWrite) continue;

                    object value = dr[tempName];
                    if (value != DBNull.Value)
                        pi.SetValue(t, value.ToString(), null);
                }
            }

            return t;
        }

        public static IList<T> ConvertToModel(DataTable dt)
        {
            // 定义集合    
            IList<T> ts = new List<T>();

            if (dt == null)
            {
                return ts;
            }

            // 获得此模型的类型   
            Type type = typeof(T);
            string tempName = "";

            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                // 获得此模型的公共属性      
                PropertyInfo[] propertys = t.GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;  // 检查DataTable是否包含此列    

                    if (dt.Columns.Contains(tempName))
                    {
                        // 判断此属性是否有Setter      
                        if (!pi.CanWrite) continue;

                        object value = dr[tempName];
                        if (value != DBNull.Value)
                            pi.SetValue(t, value.ToString(), null);
                    }
                }
                ts.Add(t);
            }
            return ts;
        }
    }
}
