using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.ForTesting
{
    public class TestClass
    {
        public int Id { get; set; }
    }
    [TestFixture]
    public class FixtureForFixture
    {
        [Test]
        public void Transform_expression_to_id()
        {
            Assert.That(Helper.GetElementName<TestClass>(test => test.Id), 
                Is.EqualTo("test_Id"));
        }

        [Test]
        public void Get_id_of_element_in_array()
        {
            Assert.That(Helper.GetElementName<TestClass[]>(test => test[0].Id),
                Is.EqualTo("test_0_Id"));
        }
    }
}
