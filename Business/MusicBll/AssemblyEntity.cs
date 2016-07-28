using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace MusicBll
{
    /// <summary>
    /// 反射工具类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AssemblyEntity<T> where T : class,new()
    {
        public static T SetEntity(object data)
        {
            T temp = new T();
            Type dataType = data.GetType();
            foreach (PropertyInfo p in temp.GetType().GetProperties())
            {
                try
                {
                    p.SetValue(temp, data.GetType().GetProperty(p.Name).GetValue(data));
                    //temp.GetType().GetProperty(p.Name).SetValue(temp, dataType.GetProperty(p.Name).GetValue(data, null), null);
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                }

            }

            return temp;
        }
        public static T SetEntity(DataRow row)
        {
            T temp = new T();
            foreach (PropertyInfo p in temp.GetType().GetProperties())
            {
                try
                {
                    if (p.PropertyType!=typeof(string))
                    {
                        if (p.PropertyType == typeof(Single))
                        {
                            p.SetValue(temp, Convert.ToSingle(row[p.Name]));
                        }
                        else if (p.PropertyType == typeof(DateTime))
                        {
                            p.SetValue(temp, Convert.ToDateTime(row[p.Name]));
                        }
                        else if (p.PropertyType == typeof(Int32))
                        {
                            p.SetValue(temp, Convert.ToInt32(row[p.Name]));
                        }
                        else
                        {
                            p.SetValue(temp, row[p.Name]);
                        }
                    }                    
                    else
                    {
                        p.SetValue(temp, row[p.Name].ToString());
                    }
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                }
            }
            return temp;
        }

        public static List<T> SetEntitys(DataTable dt)
        {
            List<T> temp = new List<T>();
            foreach (DataRow dr in dt.Rows)
            {
                temp.Add(SetEntity(dr));
            }
            return temp;
        }
        public static List<T> SetEntitys(DataRow[] rows)
        {
            List<T> temp = new List<T>();
            foreach (DataRow dr in rows)
            {
                temp.Add(SetEntity(dr));
            }
            return temp;
        }
    }
}
