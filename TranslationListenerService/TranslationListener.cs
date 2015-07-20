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
            watcher.Created += new FileSystemEventHandler(OnChanged);

            //watcher.Changed += new FileSystemEventHandler(OnChanged);
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

        //FIX
        public void ProcessQueue()
        {
            DataTable UnAssignedRequests = access.DataBaseToDataTable();

            if (UnAssignedRequests.Rows.Count > 0)
            {
                DataTable AssignedRequests = access.AssignBatches(UnAssignedRequests);
                access.UpdateDataBase(AssignedRequests);

                foreach (LocalizationDatabaseAccessor.BatchInfo batch in access.GetBatches(AssignedRequests))
                {
                    serializer.DataBaseToXMLFile(GetXMLOutputPath(batch), batch.BatchID);
                    access.UpdateBatch(batch.BatchID, batch.BatchSize, GetXMLOutputPath(batch), "Submitted");
                }
            }
        }


        private string GetXMLOutputPath(LocalizationDatabaseAccessor.BatchInfo batch)
        {
            //"<TargetLangId>_<SourceLangID>_<BatchID>_<DateStamp>.xml"
            string filename = String.Format("{0}_{1}_{2}_{3:MM-dd-yyyy}", batch.OutputLCID, batch.InputLCID, batch.BatchID, DateTime.Now);

            string path = Path.ChangeExtension(Path.Combine(Properties.Settings.Default.HandoffFolderPath, filename), "xml");

            return path;
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
