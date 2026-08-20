using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Common;

namespace Arch.Common.Linq
{
	/// <summary>
	/// Table扩展
	/// </summary>
	public static class TableExtensions
	{
		/// <summary>
		/// 批量删除
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="table">表</param>
		/// <param name="predicate">查询条件表达式</param>
		/// <returns>影响的行数</returns>
		public static int Delete<T>(this Table<T> table, Expression<Func<T, bool>> predicate) where T : class
		{ 
			//获取表名
			string tableName = table.Context.Mapping.GetTable(typeof(T)).TableName;

			//查询条件表达式转换成SQL的条件语句
			ConditionBuilder builder = new ConditionBuilder();
			builder.Build(predicate.Body);
			string sqlCondition = builder.Condition;

			//SQL命令
			string commandText = string.Format("DELETE FROM {0} WHERE {1}", tableName, sqlCondition);

			//获取SQL参数数组 
			var args = builder.Arguments;

			//执行
			return table.Context.ExecuteCommand(commandText, args);
		}

		/// <summary>
		/// 批量更新
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="table">表</param>
		/// <param name="predicate">查询条件表达式</param>
		/// <param name="updater">更新表达式</param>
		/// <returns>影响的行数</returns>
		public static int Update2<T>(this Table<T> table, Expression<Func<T, bool>> predicate, Expression<Func<T,T>> updater) where T : class
		{
			//获取表名
			string tableName = table.Context.Mapping.GetTable(typeof(T)).TableName;
			DbCommand command = table.Context.GetCommand(table.Where(predicate));
			string sqlCondition = command.CommandText;
			sqlCondition = sqlCondition.Substring(sqlCondition.LastIndexOf("WHERE ", StringComparison.InvariantCultureIgnoreCase) + 6);

			//获取Update的赋值语句
			var updateMemberExpr = (MemberInitExpression)updater.Body;
			var updateMemberCollection = updateMemberExpr.Bindings.Cast<MemberAssignment>().Select(c =>
			{
				var p = command.CreateParameter();
				p.ParameterName = c.Member.Name;
				p.Value = ((ConstantExpression)c.Expression).Value;
				return p;
			})
			.ToArray();

			string sqlUpdateBlock = string.Join(", ", updateMemberCollection.Select(c => string.Format("[{0}]=@{0}", c.ParameterName)).ToArray());

			//SQL命令
			string commandText = string.Format("UPDATE {0} SET {1} FROM {0} AS t0 WHERE {2}", tableName, sqlUpdateBlock, sqlCondition);

			//获取SQL参数数组 (包括查询参数和赋值参数)
			command.Parameters.AddRange(updateMemberCollection);
			command.CommandText = commandText; 

			//执行 
			try
			{
				if (command.Connection.State != ConnectionState.Open)
				{
					command.Connection.Open();
				}
				return command.ExecuteNonQuery();
			}
			finally
			{
				command.Connection.Close();
				command.Dispose();
			}
		}

		/// <summary>
		/// 批量更新
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="table">表</param>
		/// <param name="predicate">查询条件表达式</param>
		/// <param name="updater">更新表达式</param>
		/// <returns>影响的行数</returns>
		public static int Update<T>(this Table<T> table, Expression<Func<T, bool>> predicate, Expression<Func<T,T>> updater) where T : class
		{
			//获取表名
			string tableName = table.Context.Mapping.GetTable(typeof(T)).TableName;

			//查询条件表达式转换成SQL的条件语句
			ConditionBuilder builder = new ConditionBuilder();
			builder.Build(predicate.Body);
			string sqlCondition = builder.Condition;

			//获取Update的赋值语句
			var updateMemberExpr = (MemberInitExpression)updater.Body;
			var updateMemberCollection = updateMemberExpr.Bindings.Cast<MemberAssignment>().Select(c => new
			{
				Name = c.Member.Name,
    
				Value = ((ConstantExpression)c.Expression).Value
			});

			int i = builder.Arguments.Length;
			string sqlUpdateBlock = string.Join(", ", updateMemberCollection.Select(c => string.Format("[{0}]={1}", c.Name, "{" + (i++) + "}")).ToArray());

			//SQL命令
			string commandText = string.Format("UPDATE {0} SET {1} WHERE {2}", tableName, sqlUpdateBlock, sqlCondition);

			//获取SQL参数数组 (包括查询参数和赋值参数)
			var args = builder.Arguments.Union(updateMemberCollection.Select(c => c.Value)).ToArray();

			//执行
			return table.Context.ExecuteCommand(commandText, args);
		}

		/* Old Method
		public static int Update<T>(this Table<T> table, Expression<Func<T, bool>> predicate, object updater) where T : class
		{
			//获取表名
			string tableName = table.Context.Mapping.GetTable(typeof(T)).TableName;

			//查询条件表达式转换成SQL的条件语句
			ConditionBuilder builder = new ConditionBuilder();
			builder.Build(predicate.Body);
			string sqlCondition = builder.Condition;

			//获取Update的赋值语句
			var properties = updater.GetType().GetProperties();
			int i = builder.Arguments.Length;
			string sqlUpdateBlock = string.Join(", ", properties.Select(c => string.Format("[{0}]={1}", c.Name, "{" + (i++) + "}")).ToArray());

			//SQL命令
			string commandText = string.Format("UPDATE {0} SET {2} WHERE {1}", tableName, sqlCondition, sqlUpdateBlock);

			//获取SQL参数数组 (包括查询参数和赋值参数)
			var args = builder.Arguments.Union(properties.Select(c => c.GetValue(updater, null))).ToArray();

			//执行
			return table.Context.ExecuteCommand(commandText, args);
		}
		 * */
	}
}
