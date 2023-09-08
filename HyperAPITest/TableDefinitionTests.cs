using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace Tableau.HyperAPI.Test
{
    [TestFixture]
    class TableDefinitionTests : HyperTest
    {
        [OneTimeSetUp]
        public void SetUp()
        {
        }

        [OneTimeTearDown]
        public void TearDown()
        {
        }

        [Test]
        public void TestGetColumnByName()
        {
            TableDefinition td = new TableDefinition("foo");
            td.AddColumn("a", SqlType.Int());

            Assert.AreEqual(td.GetColumnByName("a").Name.Unescaped, "a");
            Assert.AreEqual(td.GetColumnByName(new Name("a")).Name.Unescaped, "a");

            Assert.AreEqual(td.GetColumnByName("b"), null);
            Assert.AreEqual(td.GetColumnByName(new Name("b")), null);
        }

        [Test]
        public void TestGetColumnPosByName()
        {
            TableDefinition td = new TableDefinition("foo");
            td.AddColumn("a", SqlType.Int());


            int colPosByString = td.GetColumnPosByName("a");
            int colPosByName = td.GetColumnPosByName(new Name("a"));

            Assert.AreEqual(colPosByString, 0);
            Assert.AreEqual(colPosByString, colPosByName);
            Assert.AreEqual(td.GetColumnPosByName("b"), -1);
            Assert.AreEqual(td.GetColumnPosByName(new Name("b")), -1);
        }
    }
}
