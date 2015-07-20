using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using LocalizationDatabaseAccess;
using XMLSerialization;
using TranslationListenerService;

namespace LocalizationDevTesting
{
    class Program
    {
        private static LocalizationDatabaseAccessor access;
        private static XMLSerializer serializer;
        private static FileSystemWatcher watcher;
        static void Main(string[] args)
        {
            access = new LocalizationDatabaseAccessor();
            testDatabaseAccess();
        }

        private static void testDatabaseAccess()
        {

            ProcessQueue();

        }

        static void ProcessQueue()
        {
            DataTable UnAssignedRequests = access.DataBaseToDataTable();

            if (UnAssignedRequests.Rows.Count > 0)
            {
                DataTable AssignedRequests = access.AssignBatches(UnAssignedRequests);
                //access.UpdateDataBase(AssignedRequests);

                foreach (LocalizationDatabaseAccessor.BatchInfo batch in access.GetBatches(AssignedRequests))
                {
                    //serializer.DataBaseToXMLFile(GetXMLOutputPath(batch), batch.BatchID);
                    //access.UpdateBatch(batch.BatchID, batch.BatchSize, GetXMLOutputPath(batch), "Submitted");
                    Console.WriteLine("ID: {0} Size: {1} In: {2} Out: {3}", batch.BatchID, batch.BatchSize, batch.InputLCID, batch.OutputLCID);

                }
                Console.ReadLine();
            }
        }
    }
}
