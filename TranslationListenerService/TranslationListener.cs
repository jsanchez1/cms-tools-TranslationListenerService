using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using LocalizationDatabaseAccess;
using XMLSerialization;
using System.Data.SqlClient;
using System.IO;
using System.Threading;

namespace TranslationListenerService
{
    public partial class TranslationListener : ServiceBase
    {
        private LocalizationDatabaseAccessor access;
        private XMLSerializer serializer;
        private FileSystemWatcher watcher;

        private struct BatchInfo
        {
            public long BatchID;
            public int InputLCID;
            public int OutputLCID;
        }


        public TranslationListener()
        {
            InitializeComponent();

            //Create new database access object
            access = new LocalizationDatabaseAccessor();

            //Create new XML Serialization object
            serializer = new XMLSerializer(access);


            // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher();
            watcher.Path = Properties.Settings.Default.HandbackFolderPath;
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime;
            watcher.Filter = "*.xml";

            // Add event handlers.
            //watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            //watcher.Deleted += new FileSystemEventHandler(OnChanged);
            //watcher.Renamed += new RenamedEventHandler(OnRenamed);            
        }

        protected override void OnStart(string[] args)
        {
            ServiceTimer.Interval = Properties.Settings.Default.ServiceIntervalInMinutes * (1000 * 60);
            ServiceTimer.Start();

            watcher.EnableRaisingEvents = true;
        }

        protected override void OnStop()
        {
            ServiceTimer.Stop();

            watcher.EnableRaisingEvents = false;
        }

        private void ServiceTimer_Tick(object sender, EventArgs e)
        {
            ProcessQueue();
        }
        

        #region "Queue processing"


        public void ProcessQueue()
        {
            DataTable UnAssignedRequests = access.DataBaseToDataTable();

            if (UnAssignedRequests.Rows.Count > 0)
            {
                DataTable AssignedRequests = AssignBatches(UnAssignedRequests);
                access.UpdateDataBase(AssignedRequests);

                FindBatches(AssignedRequests);

                foreach (BatchInfo batch in FindBatches(AssignedRequests))
                {
                    serializer.DataBaseToXMLFile(GetXMLOutputPath(batch), batch.BatchID);
                }
            }
        }


        private string GetXMLOutputPath(BatchInfo batch)
        {
            //"<TargetLangId>_<SourceLangID>_<BatchID>_<DateStamp>.xml"
            string filename = String.Format("{0}_{1}_{2}_{3:MM-dd-yyyy}", batch.OutputLCID, batch.InputLCID, batch.BatchID, DateTime.Now);

            string path = Path.ChangeExtension(Path.Combine(Properties.Settings.Default.HandoffFolderPath, filename), "xml");

            return path;
        }

        
        private List<BatchInfo> FindBatches(DataTable table)
        {

            List<BatchInfo> newBatchIDs = (from DataRow row in table.Rows
                                           select new BatchInfo
                                           {
                                               BatchID = (long)row[access.BatchKey],
                                               InputLCID = (int)row[access.InputLCIDKey],
                                               OutputLCID = (int)row[access.OuputLCIDKey]
                                           }).Distinct().ToList();

            return newBatchIDs;
        }
        
        private DataTable AssignBatches(DataTable table)
        {
            var simplifiedList = from DataRow row in table.Rows
                                 select new
                                 {
                                     RequestID = row[access.RequestKey],
                                     InputLCID = row[access.InputLCIDKey],
                                     OutputLCID = row[access.OuputLCIDKey]
                                 };

            var groupedByLCID = from row in simplifiedList
                                group row.RequestID by new
                                {
                                    row.InputLCID,
                                    row.OutputLCID
                                } into newList
                                select new
                                {
                                    InputLCID = newList.Key.InputLCID,
                                    OutputLCID = newList.Key.OutputLCID,
                                    Requests = newList.ToList()
                                };

            foreach (var row in groupedByLCID)
            {
                long newBatchID = access.GenerateNewBatchID();

                foreach (long request in row.Requests)
                {
                    table.Rows.Find(request)[access.BatchKey] = newBatchID;
                }

            }

            return table;
        }

        #endregion



        #region "Handback processing"
        
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (IsFileReady(e.FullPath))
                serializer.XmlFileToDataBase(e.FullPath, serializer.GetBatchID(e.FullPath, access.BatchKey));
        }
        
        private bool IsFileReady(String sFilename, int waitSeconds = 10)
        {
            DateTime start = DateTime.Now;
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            while (DateTime.Now < start.AddSeconds(waitSeconds))
            {
                try
                {
                    using (FileStream inputStream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        if (inputStream.Length > 0)
                        {
                            return true;
                        }
                        else
                        {
                            //return false;
                        }

                    }
                }
                catch (Exception)
                {
                    //return false;
                }

                Thread.Sleep(1000);
            }

            return false;
        }



        #endregion

    }
}
