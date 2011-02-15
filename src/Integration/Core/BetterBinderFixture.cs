using System;
using System.Reflection;
using Castle.ActiveRecord.Framework.Internal;
using Castle.Components.Binder;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework.Test;
using NUnit.Framework;

namespace Integration.Core
{
	public class BetterBinder : ARDataBinder
	{
		private bool IsValidKey(object id)
		{
			if (id != null)
			{
				if (id.GetType() == typeof(String))
				{
					return id.ToString() != String.Empty;
				}
				else if (id.GetType() == typeof(Guid))
				{
					//if (this.TreatEmptyGuidAsNull)
						return Guid.Empty != ((Guid)id);
//					else
//						return true;
				}
				else
				{
					return Convert.ToInt64(id) != 0;
				}
			}

			return false;
		}


		protected override object CreateInstance(Type instanceType, String paramPrefix, Node node)
		{
			var model = ActiveRecordModel.GetModel(instanceType);

			if (node == null && model != null)
			{
				throw new BindingException(
					"Nothing found for the given prefix. Are you sure the form fields are using the prefix " +
					paramPrefix + "?");
			}

			if (node != null && node.NodeType != NodeType.Composite)
			{
				throw new BindingException("Unexpected node type. Expecting Composite, found " + node.NodeType);
			}

			var cNode = (CompositeNode) node;

			object instance;

			var shouldLoad = AutoLoad != AutoLoadBehavior.Never;

			if (AutoLoad == AutoLoadBehavior.OnlyNested)
			{
				shouldLoad = StackDepth != 0;
			}

			if (shouldLoad && model == null) // Nested type or unregistered type
			{
				shouldLoad = false;
			}

			if (shouldLoad)
			{
				if (instanceType.IsArray)
				{
					throw new BindingException("ARDataBinder AutoLoad does not support arrays");
				}

				PrimaryKeyModel pkModel;

				var id = ObtainPrimaryKeyValue(model, cNode, paramPrefix, out pkModel);

				if (IsValidKey(id))
				{
					instance = FindByPrimaryKey(instanceType, id);
				}
				else
				{
					if (AutoLoad == AutoLoadBehavior.NewInstanceIfInvalidKey ||
						(AutoLoad == AutoLoadBehavior.NewRootInstanceIfInvalidKey && StackDepth == 0))
					{
						instance = base.CreateInstance(instanceType, paramPrefix, node);
					}
					else if (AutoLoad == AutoLoadBehavior.NullIfInvalidKey ||
							 AutoLoad == AutoLoadBehavior.OnlyNested ||
							 (AutoLoad == AutoLoadBehavior.NewRootInstanceIfInvalidKey && StackDepth != 0))
					{
						instance = null;
					}
					else
					{
						throw new BindingException(string.Format(
													   "Could not find primary key '{0}' for '{1}'",
													   pkModel.Property.Name, instanceType.FullName));
					}
				}
			}
			else
			{
				const BindingFlags creationFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

				instance = Activator.CreateInstance(instanceType, creationFlags, null, null, null);
			}

			return instance;
		}

		private static PrimaryKeyModel ObtainPrimaryKey(ActiveRecordModel model)
		{
			if (model.IsJoinedSubClass || model.IsDiscriminatorSubClass)
			{
				return ObtainPrimaryKey(model.Parent);
			}
			return model.PrimaryKey;
		}

		private object ObtainPrimaryKeyValue(ActiveRecordModel model, CompositeNode node, String prefix,
											 out PrimaryKeyModel pkModel)
		{
			pkModel = ObtainPrimaryKey(model);

			var pkPropName = pkModel.Property.Name;

			var idNode = node.GetChildNode(pkPropName);

			if (idNode == null) return null;

			if (idNode != null && idNode.NodeType != NodeType.Leaf)
			{
				throw new BindingException("Expecting leaf node to contain id for ActiveRecord class. " +
										   "Prefix: {0} PK Property Name: {1}", prefix, pkPropName);
			}

			var lNode = (LeafNode) idNode;

			if (lNode == null)
			{
				throw new BindingException("ARDataBinder autoload failed as element {0} " +
										   "doesn't have a primary key {1} value", prefix, pkPropName);
			}

			bool conversionSuc;

			return Converter.Convert(pkModel.Property.PropertyType, lNode.ValueType, lNode.Value, out conversionSuc);
		}
	}

	[TestFixture]
	public class BetterBinderFixture
	{
		public class TestBetterBinder
		{}

		[Test]
		public void If_object_not_active_record_do_not_need_check_node()
		{
			var request = new StubRequest();
			var binder = new BetterBinder();
			
			var record = (TestBetterBinder)binder.BindObject(typeof(TestBetterBinder), "item", request.ParamsNode);
	
			Assert.That(record, Is.Not.Null);
		}
	}
}