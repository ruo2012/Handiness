﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections.Concurrent;
namespace Handiness.Orm
{
    public class PropertyAccessor<T>
    {
        private static PropertyAccessor<T> _Instance = new PropertyAccessor<T>();
        public PropertyAccessor<T> Instance
        {
            get
            {
                return _Instance;
            }
        }

        /// <summary>
        /// 属性获取访问器名称格式
        /// </summary>
        protected const String ProperityGetterNameFormat = "get_{0}";
        /// <summary>
        /// 属性设置访问器名称格式
        /// </summary>
        protected const String ProperitySetterNameFormat = "set_{0}";

        /// <summary>
        /// 泛型无返回值方法附加头 ACT_GEN
        /// </summary>
        protected const Int32 GenericActionHead = 7;
        /// <summary>
        /// 普通无返回值方法附加头 ACT_
        /// </summary>
        protected const Int32 ActionHead = 13;
        /// <summary>
        /// 泛型有返回值方法附加头 RET_GEN
        /// </summary>
        protected const Int32 GenericFunctionHead = 19;
        /// <summary>
        /// 普通有返回值方法附加头 RET_
        /// </summary>
        protected const Int32 FunctionHead = 23;

        /***********************/
        private ConcurrentDictionary<Int64, Delegate> _deleagateCache = new ConcurrentDictionary<Int64, Delegate>();
        private Type _type = typeof(T);

        public void SetProperityValue(T obj, String propertyName, Object value)
        {
            Type type = this._type;
            Type inputType = value.GetType();
            dynamic dyncValue = value;
            dynamic action = null;

            Int64 key = this.GenerationKey(ActionHead, type.GetHashCode(), propertyName.GetHashCode(), inputType.GetHashCode());
            var getResult = this.GetDelegateCache(key);
            if (getResult.existed)
            {
                action = getResult.dlgtObj;
            }
            else
            {
                var getter = type.GetMethod(String.Format(ProperitySetterNameFormat, propertyName));
                ParameterExpression instanceExp = Expression.Parameter(type, "obj");
                ParameterExpression valueExp = Expression.Parameter(inputType, "value");
                MethodCallExpression callExp = Expression.Call(instanceExp, getter, valueExp);
                var lambda = Expression.Lambda(callExp, instanceExp, valueExp);
                action = lambda.Compile();
                this.AddDelegateCache(key, action);
            }
            action.Invoke(obj, dyncValue);
        }


        public void SetProperityValue<TINPUT>(T obj, String propertyName, TINPUT value)
        {
            Type type = this._type;
            Type inputType = value.GetType();
            Action<T, TINPUT> action = null;
            Int64 key = this.GenerationKey(GenericActionHead, type.GetHashCode(), propertyName.GetHashCode(), inputType.GetHashCode());
            var getResult = this.GetDelegateCache(key);
            if (getResult.existed)
            {
                action = getResult.dlgtObj as Action<T, TINPUT>;
            }
            else
            {
                var getter = type.GetMethod(String.Format(ProperitySetterNameFormat, propertyName));
                ParameterExpression instanceExp = Expression.Parameter(type, "obj");
                ParameterExpression valueExp = Expression.Parameter(inputType, "value");
                MethodCallExpression callExp = Expression.Call(instanceExp, getter, valueExp);
                var lambda = Expression.Lambda<Action<T, TINPUT>>(callExp, instanceExp, valueExp);
                action = lambda.Compile();
                this.AddDelegateCache(key, action);
            }
            action.Invoke(obj, value);
        }

        public Object GetProperityValue(T obj, String propertyName)
        {
            Type type = this._type;
            dynamic func = null;
            Int64 key = this.GenerationKey(FunctionHead, type.GetHashCode(), propertyName.GetHashCode());
            var getResult = this.GetDelegateCache(key);
            if (getResult.existed)
            {
                func = getResult.dlgtObj;
            }
            else
            {
                var getter = type.GetMethod(String.Format(ProperityGetterNameFormat, propertyName));
                ParameterExpression parameter = Expression.Parameter(type, "obj");
                MethodCallExpression callExp = Expression.Call(parameter, getter);
                var lambda = Expression.Lambda(callExp, parameter);
                func = lambda.Compile();
                this.AddDelegateCache(key, func);
            }
            return func.Invoke(obj);
        }

        public TResult GetProperityValue<TResult>(T obj, String propertyName)
        {
            Type type = this._type;
            Type retType = typeof(TResult);
            Func<T, TResult> func = null;
            Int64 key = this.GenerationKey(GenericFunctionHead, type.GetHashCode(), propertyName.GetHashCode(), retType.GetHashCode());
            var getResult = this.GetDelegateCache(key);
            if (getResult.existed)
            {
                func = getResult.dlgtObj as Func<T, TResult>;
            }
            else
            {
                var getter = type.GetMethod(String.Format(ProperityGetterNameFormat, propertyName));
                ParameterExpression parameter = Expression.Parameter(type, "obj");
                MethodCallExpression callExp = Expression.Call(parameter, getter);
                var lambda = Expression.Lambda<Func<T, TResult>>(callExp, parameter);
                func = lambda.Compile();
                this.AddDelegateCache(key, func);
            }
            return func.Invoke(obj);
        }

        protected Int64 GenerationKey(params Int32[] code)
        {
            Int64 key = 0;
            Int32 count = code.Length;
            for (Int32 i = 0; i < count; ++i)
            {
                key += code[i];
            }
            return key;
        }
        protected void AddDelegateCache(Int64 key, Delegate dlgtObj)
        {
            this._deleagateCache.TryAdd(key, dlgtObj);
        }
        protected (Boolean existed, Delegate dlgtObj) GetDelegateCache(Int64 key)
        {
            Boolean existed = false;
            existed = this._deleagateCache.TryGetValue(key, out Delegate dlgtObj);
            return (existed, dlgtObj);
        }
        protected Boolean RemoveDelegateCache(Int64 key)
        {
            return this._deleagateCache.TryRemove(key, out Delegate obj);
        }

        public void ClearDelegateCache()
        {
            this._deleagateCache.Clear();
        }
    }
}
