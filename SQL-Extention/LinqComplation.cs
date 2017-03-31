using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace SQL_Extention
{
    public static class LinqComplation
    {
        public static string ExpretionToString<T>(Expression<Func<T, bool>> exp) =>
            CompileExpr<T>(exp.Body, new List<string>(exp.Parameters.Select(s => s.Name)));
        public static string ExpretionToString<T, T2>(Expression<Func<T, T2, bool>> exp) =>
            CompileExpr<T>(exp.Body, new List<string>(exp.Parameters.Select(s => s.Name)));
        public static string ExpretionToString<T, T2, T3>(Expression<Func<T, T2, T3, bool>> exp) =>
            CompileExpr<T>(exp.Body, new List<string>(exp.Parameters.Select(s => s.Name)));
        public static string ExpretionToString<T, T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> exp) =>
            CompileExpr<T>(exp.Body, new List<string>(exp.Parameters.Select(s => s.Name)));
        public static string ExpretionToString<T, T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, bool>> exp) =>
            CompileExpr<T>(exp.Body, new List<string>(exp.Parameters.Select(s => s.Name)));
        public static string ExpretionToString<T, T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> exp) =>
            CompileExpr<T>(exp.Body, new List<string>(exp.Parameters.Select(s => s.Name)));
        public static string ExpretionToString<T, T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> exp) =>
            CompileExpr<T>(exp.Body, new List<string>(exp.Parameters.Select(s => s.Name)));
        public static string ExpretionToString<T, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> exp) =>
            CompileExpr<T>(exp.Body, new List<string>(exp.Parameters.Select(s => s.Name)));
        public static string ExpretionToString<T, T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp) =>
            CompileExpr<T>(exp.Body, new List<string>(exp.Parameters.Select(s => s.Name)));
        public static string ExpretionToString<T, T2, T3, T4, T5, T6, T7, T8, T9,T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9,T10, bool>> exp) =>
            CompileExpr<T>(exp.Body, new List<string>(exp.Parameters.Select(s => s.Name)));

        private static string CompileExpr<T>(Expression expr, List<string> names)
        {
            if (expr == null)
                throw new Exception("cant calc, there is null");
            if (expr is BinaryExpression bin)
            {
                var left = CompileExpr<T>(bin.Left, names);
                var right = CompileExpr<T>(bin.Right, names);
                return $"({left} {GetSqlName(bin)} {right})";
            }

            else if (expr is MethodCallExpression met)
            {
                if (met.Method.DeclaringType == typeof(string))
                {
                    if (met.Method.Name.Equals("Contains"))
                        return $"({CompileExpr<T>(met.Object, names)} LIKE '%{CompileExpr<T>(met.Arguments[0], names)}%')";
                    else if (met.Method.Name.Equals("StartsWith"))
                        return $"({CompileExpr<T>(met.Object, names)} LIKE '{CompileExpr<T>(met.Arguments[0], names)}%')";
                    else if (met.Method.Name.Equals("EndsWith"))
                        return $"({CompileExpr<T>(met.Object, names)} LIKE '%{CompileExpr<T>(met.Arguments[0], names)}')";
                    else if (met.Method.Name.Equals("ToLower"))
                        return $"(lower({CompileExpr<T>(met.Object, names)}))";
                    else if (met.Method.Name.Equals("ToUpper"))
                        return $"(upper({CompileExpr<T>(met.Object, names)}))";
                }
            }
            else if(expr is UnaryExpression u)
            {
                //var ty = u.Type;
                //var valr = CompileExpr<T>(u.Operand, names);
                return CompileExpr<T>(u.Operand, names);
            }
            else if (expr is ConstantExpression constant)
            {
                if (constant.Value is string)
                    return $"'{constant.Value.ToString()}'";
                return constant.Value.ToString();
            }
            else if (expr is MemberExpression mem)
            {
                if (mem.Member.DeclaringType == typeof(T))
                {
                    return $"({typeof(T).Name}.{mem.Member.Name})";
                }
                throw new NotSupportedException("cant soport that");
            }
            else if (expr is ParameterExpression par)
            {
                return $"(@{names.FindIndex(s => s == par.Name)})";
            }
            throw new NotImplementedException();
        }

        private static string GetSqlName(Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.And:
                    return "&";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "!=";
                default:
                    throw new NotSupportedException($"Cannot get SQL for: {expr.NodeType}");

            }
        }
    }
}
