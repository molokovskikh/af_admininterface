﻿using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using WatiN.Core;

namespace AdminInterface.Test.ForTesting
{
	public static class Helper
	{
		public static string ExtractName(Expression expression)
		{
			var unary = expression as UnaryExpression;
			if (unary != null)
				return ExtractName(unary.Operand);

			var parameter = expression as ParameterExpression;
			if (parameter != null)
				return parameter.Name;

			var member = expression as MemberExpression;
			if (member != null)
				return ExtractName(member.Expression) + "_" + member.Member.Name;

			var constant = expression as ConstantExpression;
			if (constant != null)
				return constant.Value.ToString();

			var array = expression as BinaryExpression;
			if (expression.NodeType == ExpressionType.ArrayIndex)
				return ExtractName(array.Left) + "_" + ExtractName(array.Right);

			throw new Exception(String.Format("Не знаю как разбирать выражение {0}", expression));
		}

		public static string GetElementName<T>(Expression<Func<T, object>> input)
		{
			return ExtractName(input.Body);
		}

		public static Table GetDataTable(IE ie)
		{
			return ie.Table(Find.By("ClassName", "DataTable"));
		}

		public static void AssertEquality<T>(this TableRow row, T record)
		{
			foreach (var header in FixtureMapping.Headers<T>())
			{
				var cell = GetCellByHeader(row, header);
				cell.AssertEquality(record, header);
			}
		}

		public static TableCell GetCellByHeader(TableRow row, string headerText)
		{
			var headerRow = row.ParentTable.TableRows.First();
			var index = 0;
			foreach (Element header in headerRow.Elements)
			{
				try
				{
					if (header.TagName != "TD" && header.TagName != "TH")
						continue;

					if (header.Text == headerText)
						return row.TableCells[index];
					index++;
				}
				catch (Exception e)
				{
					throw new Exception(headerText, e);
				}
			}
			throw new Exception("Не нашли ячейку по заголовку " + headerText);
		}

		public static string GetHeader(TableCell cell)
		{
			return cell
				.ParentTableRow
				.ParentTable
				.TableRows[0]
				.Elements
				.Where(e => e.TagName == "TH")
				.Skip(cell.Index)
				.First()
				.Text;
		}

		public static void AssertEquality<T>(this TableCell cell, T record, string alias)
		{
			var value = FixtureMapping.GetValue(record, alias);
			var compare = value as Func<Element, bool>;
			if (compare != null)
				Assert.That(compare(cell.Elements.First()), Is.True, "Для ячейки {0} не нашлось значения {1} по алиасу {2}", cell, record, alias);
			else
				Assert.That(cell.Text != null ? cell.Text.Trim() : cell.Text, Is.EqualTo(value));
		}

		public static bool IsEqual<T>(this TableCell cell, T record, string alias)
		{
			var value = FixtureMapping.GetValue(record, alias);
			var compare = value as Func<Element, bool>;
			if (compare != null)
				return compare(cell.Elements.First());
			if (value == null)
				return cell.Text == null;

			return (cell.Text != null ? cell.Text.Trim() : cell.Text) == value.ToString();
		}

	}
}