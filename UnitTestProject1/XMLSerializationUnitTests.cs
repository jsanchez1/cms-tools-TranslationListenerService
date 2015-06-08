using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TranslationListenerService;
using XMLSerialization;
using LocalizationDatabaseAccess;


namespace LocalizationUnitTestProject
{
    public static class TestHelp
    {
        public static string testpath = "D:\\dataAdapter1.xml";
        public static string testconnectionstring = "Data Source=BELGF4ZKM1;Integrated Security=True;Initial Catalog=TranslationDB";
        public static string teststoredprocedurename = "Localization_GetQueue";
        public static string testtableName = "TranslationRequest";
        public static string testprimarykey = "TranslationRequestID";
        public static string testbatchcolumn = "AssignedBatchID";


        public static void PopulateXMLFile()
        {
            XMLSerializer serializer = new XMLSerializer(testconnectionstring, teststoredprocedurename, testtableName, testprimarykey, testbatchcolumn);

            serializer.DataBaseToXMLFile(testpath);
        }
    }

    [TestClass]
    public class DataBaseToXMLFileTests
    {
        [TestMethod]
        public void DataBaseToXMLFile_ValidParameters()
        {
            XMLSerializer serializer = new XMLSerializer(TestHelp.testconnectionstring, TestHelp.teststoredprocedurename, TestHelp.testtableName, TestHelp.testprimarykey, TestHelp.testbatchcolumn);

            serializer.DataBaseToXMLFile(
                path: TestHelp.testpath);
        }


        [TestMethod]
        public void DataBaseToXMLFile_NullParameters()
        {
            try
            {
                XMLSerializer serializer = new XMLSerializer(null, null, null, null, null);

                serializer.DataBaseToXMLFile(null);
            }
            catch (LocalizationDatabaseAccessor.LocalizationDatabaseAccessorException ex)
            {
                StringAssert.Contains(ex.Message, LocalizationDatabaseAccessor.LocalizationDatabaseAccessorException.SQLConnectionInvalid);

                return;
            }

            Assert.Fail("No exception was thrown.");
        }


        [TestMethod]
        public void DataBaseToXMLFile_NullXMLPath()
        {
            XMLSerializer serializer = new XMLSerializer(TestHelp.testconnectionstring, TestHelp.teststoredprocedurename, TestHelp.testtableName, TestHelp.testprimarykey, TestHelp.testbatchcolumn);

            try
            {
                serializer.DataBaseToXMLFile(null);
            }
            catch (XMLSerializer.XMLSerializerException ex)
            {
                StringAssert.Contains(ex.Message, XMLSerializer.XMLSerializerException.XMLPathInvalid);
                return;
            }

            Assert.Fail("No exception was thrown.");
        }

        [TestMethod]
        public void DataBaseToXMLFile_NullConnectionString()
        {
            XMLSerializer serializer = new XMLSerializer(null, TestHelp.teststoredprocedurename, TestHelp.testtableName, TestHelp.testprimarykey, TestHelp.testbatchcolumn);

            try
            {
                serializer.DataBaseToXMLFile(TestHelp.testpath);
            }
            catch (LocalizationDatabaseAccessor.LocalizationDatabaseAccessorException ex)
            {
                StringAssert.Contains(ex.Message, LocalizationDatabaseAccessor.LocalizationDatabaseAccessorException.SQLConnectionInvalid);
                return;
            }

            Assert.Fail("No exception was thrown.");
        }

        [TestMethod]
        public void DataBaseToXMLFile_NullStoredProcedureName()
        {
            try
            {
                XMLSerializer serializer = new XMLSerializer(TestHelp.testconnectionstring, null, TestHelp.testtableName, TestHelp.testprimarykey, TestHelp.testbatchcolumn);

                serializer.DataBaseToXMLFile(TestHelp.testpath);
            }
            catch (LocalizationDatabaseAccessor.LocalizationDatabaseAccessorException ex)
            {
                StringAssert.Contains(ex.Message, LocalizationDatabaseAccessor.LocalizationDatabaseAccessorException.SQLStoredProcedureInvalid);
                return;
            }

            Assert.Fail("No exception was thrown.");
        }



        [TestMethod]
        public void DataBaseToXMLFile_NullTableName()
        {
            XMLSerializer serializer = new XMLSerializer(TestHelp.testconnectionstring, TestHelp.teststoredprocedurename, null, TestHelp.testprimarykey, TestHelp.testbatchcolumn);

            try
            {
                serializer.DataBaseToXMLFile(TestHelp.testpath);
            }
            catch (XMLSerializer.XMLSerializerException ex)
            {
                StringAssert.Contains(ex.Message, XMLSerializer.XMLSerializerException.SQLTableInvalid);
                return;
            }

            Assert.Fail("No exception was thrown.");
        }


        [TestMethod]
        public void DataBaseToXMLFile_NullPrimaryKey()
        {
            XMLSerializer serializer = new XMLSerializer(TestHelp.testconnectionstring, TestHelp.teststoredprocedurename, TestHelp.testtableName, null, TestHelp.testbatchcolumn);

            try
            {
                serializer.DataBaseToXMLFile(TestHelp.testpath);
            }
            catch (XMLSerializer.XMLSerializerException ex)
            {
                StringAssert.Contains(ex.Message, XMLSerializer.XMLSerializerException.exceptionMessage);
                return;
            }

            Assert.Fail("No exception was thrown.");
        }


        [TestMethod]
        public void DataBaseToXMLFile_NullBatchIDColumn()
        {
            XMLSerializer serializer = new XMLSerializer(TestHelp.testconnectionstring, TestHelp.teststoredprocedurename, TestHelp.testtableName, TestHelp.testprimarykey, null);

            try
            {
                serializer.DataBaseToXMLFile(TestHelp.testpath);
            }
            catch (XMLSerializer.XMLSerializerException ex)
            {
                StringAssert.Contains(ex.Message, XMLSerializer.XMLSerializerException.exceptionMessage);
                return;
            }

            Assert.Fail("No exception was thrown.");
        }



    }

    [TestClass]
    public class XmlFileToDataBase
    {


        [TestMethod]
        public void XmlFileToDataBase_ValidParameters()
        {
            TestHelp.PopulateXMLFile();

            XMLSerializer serializer = new XMLSerializer(TestHelp.testconnectionstring, TestHelp.teststoredprocedurename, TestHelp.testtableName, TestHelp.testprimarykey, TestHelp.testbatchcolumn);

            serializer.XmlFileToDataBase(
                path: TestHelp.testpath);
        }


        [TestMethod]
        public void XmlFileToDataBase_NullParameters()
        {
            try
            {
                TestHelp.PopulateXMLFile();

                XMLSerializer serializer = new XMLSerializer(null, null, null, null, null);

                serializer.XmlFileToDataBase(
                    path: null);
            }
            catch (XMLSerializer.XMLSerializerException ex)
            {               
                return;
            }

            Assert.Fail("No exception was thrown.");          
        }

    }
}
