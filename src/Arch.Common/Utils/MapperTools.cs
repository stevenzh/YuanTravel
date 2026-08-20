using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using NBear.Mapping;

namespace Arch.Common.Utils
{
    /// <summary>
    /// 对象映射工具类
    /// </summary>
    public class MapperTools
    {

        /// <summary>
        ///  对象之间映射
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public TOutput ToMap<TInput, TOutput>(TInput input, TOutput output)
            where TInput : class
            where TOutput : class
        {
            return ObjectConvertor.ToObject(input, output);
        }

        /// <summary>
        /// 对象集合之间映射
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="inputs"></param>
        /// <param name="outputs"></param>
        /// <returns></returns>
        public IList<TOutput> ToMapList<TInput, TOutput>(IList<TInput> inputs, IList<TOutput> outputs)
            where TInput : class
            where TOutput : class
        {

            if (inputs.Count <= 0)
                return outputs;


            var temps = ObjectConvertor.ToArray<TInput, TOutput>(inputs);
            foreach (TOutput item in temps)
            {
                outputs.Add(item);
            }
            return outputs;
        }

    }
}
