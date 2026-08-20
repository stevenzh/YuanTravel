using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;

namespace Arch.Common.Utils
{
    /*
     为确保两者能够正确的转换，请在使用之前，确定转换的两个对象中的属性名称以及属性的数据类型完全一致，
     属性名称或者数据类型不一致的，将不会进行转换，特别需要注意的是值类型间的转换，一般情况下，LinQ To
     SQL在生成Entity时，会将值类型声明为可以容纳null值的对象形式(如“int?”)，在建立Model时，请务必
     检查Model中的对应的属性的数据类型和Entity中的保持一致。
    */
    public static class EntityToModel
    {
        /// <summary>
        /// 将Entity的List转换成Model的List
        /// </summary>
        /// <typeparam name="TInput">Entity的实例集合</typeparam>
        /// <typeparam name="TOutput">Model的实例</typeparam>
        /// <param name="inputList">Entity的实例集合，如果传入Null，则返回Null</param>
        /// <param name="output">Model的实例，如果传入Null，则返回Null</param>
        /// <returns>Model的实例集合</returns>
        public static IList<TOutput> ToModelList<TInput, TOutput>(this IList<TInput> inputList, TOutput output)
            where TInput : class
            where TOutput : class
        {
            if (inputList == null || output == null)//检查参数是否为null
                return null;
            IList<TOutput> list = new List<TOutput>();//实例化返回值
            foreach (var temp in inputList)
            {
                Type inputType = temp.GetType();
                if (output != null)
                {
                    Type outputType = output.GetType();
                    ConstructorInfo info = outputType.GetConstructor(new Type[0]);//根据数据类型获取数据类型的构造访问权限
                    output = info.Invoke(null) as TOutput;//通过构造访问权限访问构造函数实例化该数据类型
                    foreach (PropertyInfo inputPro in inputType.GetProperties())
                    {
                        foreach (PropertyInfo outputPro in outputType.GetProperties())
                        {
                            if (inputPro.Name == outputPro.Name && outputPro.PropertyType == inputPro.PropertyType)//匹配属性名以及属性类型是否一致
                            {
                                outputPro.SetValue(output, inputPro.GetValue(temp, null), null);//属性赋值
                                break;
                            }
                        }
                    }
                }
                list.Add(output);
            }
            return list;
        }

        /// <summary>
        /// 将通过LinQ查询出来的结果集，转换成对应的ModelList
        /// </summary>
        /// <typeparam name="TInput">查询结果集</typeparam>
        /// <typeparam name="TOutput">Model的实例</typeparam>
        /// <param name="inputList">查询结果集</param>
        /// <param name="output">Model的实例</param>
        /// <returns>Model的泛型集合</returns>
        public static IList<TOutput> ToModelList<TInput, TOutput>(this IQueryable<TInput> inputList, TOutput output)
            where TInput : class
            where TOutput : class
        {
            return ToModelList(inputList.ToList(), output);
        }

        /// <summary>
        /// Entity和Model之间相互转换
        /// </summary>
        /// <typeparam name="TInput">需要转换的对象，可以是Entity也可以是Model</typeparam>
        /// <typeparam name="TOutput">转换后的对象，如第一个参数是Entity，则这里必须是Model，
        /// 如果第一个参数是Model，则这里必须是Entity</typeparam>
        /// <param name="input">需要转换的对象，可以是Entity也可以是Model，如果传入Null，则返回Null</param>
        /// <param name="output">转换后的对象，如第一个参数是Entity，则这里必须是Model，否则反之，如果传入Null，则返回Null</param>
        /// <returns>转换后的实例对象</returns>
        public static TOutput ToMutual<TInput, TOutput>(this TInput input, TOutput output)
            where TInput : class
            where TOutput : class
        {
            if (input == null || output == null)
                return null;
            Type inputType = input.GetType();
            Type outputType = output.GetType();
            foreach (PropertyInfo inputPro in inputType.GetProperties())
            {
                foreach (PropertyInfo outputPro in outputType.GetProperties())
                {
                    if (inputPro.Name == outputPro.Name && outputPro.PropertyType == inputPro.PropertyType)
                    {
                        outputPro.SetValue(output, inputPro.GetValue(inputType, null), null);
                        break;
                    }
                }
            }
            return output;
        }
    }
}
