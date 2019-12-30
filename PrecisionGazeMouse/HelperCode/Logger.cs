/*
 * Copyright 2019 Rishi Kapadia
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at 
 * http://www.apache.org/licenses/LICENSE-2.0|http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using PrecisionGazeMouse;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

/*
 * Used for logging debug info, and then can create a .7z zip file to generate a report
 * so that it can be exported
 */

namespace PrecisionGazeMouse
{

    public static class Logger
    {
        public static int millisecondsToKeep = 45000;
        public static bool loggingEnabled = true;

        public static string logListXmlFilename = "logList.xml";
        public static string variablesDictXmlFilename = "variablesDict.xml";
        public static string funcCallCountDictXmlFilename = "FuncCallCountDict.xml";

        public static string autoErrorReportFolder = "AutoErrorReports";
        public static int autoErrorReportsCount = 0;
        public static long lastErrorReportCreationTimestamp = 0;
        public static string knownErrorsXmlFilename = "knownErrors.xml";

        private static object errorReportCond = new object();
        private static object threadStartCond = new object();
        public static Thread errorReportThread = new Thread(errorReportingThread);

        //This list keeps all log entries for the past ConfigManager.dataCaptureSecondsToKeep
        public static LogList logList = new LogList();

        //Keeps all the points related to gaze and cursor movement

        public static void WriteVar(string varname, object varval,
            [CallerFilePath] string filename = null,
            [CallerMemberName] string methodname = null,
            [CallerLineNumber] int linenumber = 0)
        {
            try
            {
                string msg = varname + "=" + varval.ToString();
                LogEntry logentry = new LogEntry(LOG_ENTRY_TYPE.VAR, filename, methodname, linenumber, msg);
                logList.AddEntryThreadSafe(logentry);
            }
            catch(Exception e)
            {
                Debug.WriteLine("WriteVar failed: " + e.ToString());
            }
            
        }

        public static void WriteMsg(string msg,
            [CallerFilePath] string filename = null,
            [CallerMemberName] string methodname = null,
            [CallerLineNumber] int linenumber = 0)
        {
            try
            {
                LogEntry logentry = new LogEntry(LOG_ENTRY_TYPE.MSG, filename, methodname, linenumber, msg);
                logList.AddEntryThreadSafe(logentry);
            }
            catch (Exception e)
            {
                Debug.WriteLine("WriteMsg failed: " + e.ToString());
            }
        }

        public static void WriteError(string msg,
            [CallerFilePath] string filename = null,
            [CallerMemberName] string methodname = null,
            [CallerLineNumber] int linenumber = 0)
        {
            LogEntry logentry = null;
            try
            {
                logentry = new LogEntry(LOG_ENTRY_TYPE.ERROR, filename, methodname, linenumber, msg);
                logList.AddEntryThreadSafe(logentry);
            }
            catch(Exception e)
            {
                Debug.WriteLine("WriteError failed: " + e.ToString());
            }

            if(ShouldCreateErrorReport(logentry))
            {
                knownErrorsList.Add(logentry);

                //Start the reporting in a thread so that it does it does not slow down rest of the program
                if(errorReportThread.ThreadState == System.Threading.ThreadState.Unstarted)
                {
                    errorReportThread.Start();
                    //ensure the thread gets started
                    lock(threadStartCond)
                    {
                        Monitor.Wait(threadStartCond);
                    }
                }

                //signal the error reporting thread that it should create a new report
                lock (errorReportCond)
                {
                    Monitor.PulseAll(errorReportCond);
                }

                
            }
        }
        
        public static void errorReportingThread()
        {
            //The first time the thread starts, let it know it has started to execute
            lock(threadStartCond)
            {
                Monitor.PulseAll(threadStartCond);
            }

            while (true)
            {
                lock (errorReportCond)
                {
                    Monitor.Wait(errorReportCond);
                }

                try
                {
                    //1. create the folder for automatic errors
                    if (!Directory.Exists(autoErrorReportFolder))
                    {
                        Directory.CreateDirectory(autoErrorReportFolder);
                    }

                    long currentTimestamp = LogEntry.GetCurrentTimeStamp();
                    //2. create subfolder that stores all the files for this particular error report
                    string reportFolderName = autoErrorReportFolder + "/autoreport_" + currentTimestamp;
                    if (!Directory.Exists(reportFolderName))
                    {
                        Directory.CreateDirectory(reportFolderName);
                    }

                    string msg = knownErrorsList.Last().msg;

                    //3. Write the msg (error text) as a .txt file so that it can be read as the report description
                    File.WriteAllLines(reportFolderName + "/reportDescription.txt", msg.Split('\n'));

                    //4. Save the current loglist, variablesDict, FunctioncallsCountDict to the error reporting folder, csv gaze data
                    Logger.SaveToDisk(reportFolderName + "/");

                    //dataLogList.WriteToCsv(reportFolderName + "/gazedata.csv");

                    //Copy Config settings file over as well
                    ConfigManager.SaveToDisk();
                    File.Copy(ConfigManager.xmlExportFileName, reportFolderName + "/" + ConfigManager.xmlExportFileName);


                    //5. Zip up the folder and remove the original folder
                    if (Directory.Exists(reportFolderName))
                    {
                        Directory.Delete(reportFolderName, true);
                    }
                    //6. Notify precision gaze mouse form of created error report (increment)
                    autoErrorReportsCount++;

                    SaveKnownErrorsToDisk("");
                }
                catch(Exception ex)
                {
                    Debug.Write(ex);
                }
            }
        }

        private static bool ShouldCreateErrorReport(LogEntry logentry)
        {
            if(logentry == null) { return false; }
            //We don't want auto errors reported in a time shorter than 30 seconds
            if (!ConfigManager.automaticErrorNotification || LogEntry.GetCurrentTimeStamp() - lastErrorReportCreationTimestamp < 30000)
            {
                return false;
            }
            
            foreach(LogEntry known in knownErrorsList)
            {
                //if a known entry is the same as logentry, then return false
                if(known.SimilarTo(logentry))
                {
                    return false;
                }
            }

            return true;
        }

        //The same as writeFunc but with a stack depth of 0.  No actual arguments should be passed by caller.
        public static void WriteEvent(
            [CallerFilePath] string filename = null,
            [CallerMemberName] string methodname = null,
            [CallerLineNumber] int linenumber = 0)
        {
            try
            {
                string msg = "";
                LogEntry logentry = new LogEntry(LOG_ENTRY_TYPE.EVENT, filename, methodname, linenumber, msg);
                logList.AddEntryThreadSafe(logentry);
            }
            catch (Exception e)
            {
                Debug.WriteLine("WriteEvent failed: " + e.ToString());
            }
        }
        //stackDepth saids how far up the stack call we should print
        //  0 means none
        public static void WriteFunc(int stackDepth,
             [CallerFilePath] string filename = null,
            [CallerMemberName] string methodname = null,
            [CallerLineNumber] int linenumber = 0)
        {

            try
            {
                string msg = "";
                int stackIndex = 1;
                while (stackIndex <= stackDepth)
                {
                    msg += "\n" + new StackFrame(stackIndex).GetMethod().ToString();
                    stackIndex++;
                }

                LogEntry logentry = new LogEntry(LOG_ENTRY_TYPE.FUNC, filename, methodname, linenumber, msg);
                logList.AddEntryThreadSafe(logentry);
            }
            catch (Exception e)
            {
                Debug.WriteLine("WriteFunc failed: " + e.ToString());
            }
            
        }

        //parentFolderPath should already contain a slash at the end
        public static void SaveToDisk(string folderPath)
        {

            //https://stackoverflow.com/questions/1507010/how-to-implement-xml-serialization-for-custom-types
            try
            {
                DataContractSerializer format = new DataContractSerializer(logList.GetType());
                var settings = new XmlWriterSettings { Indent = true, NewLineOnAttributes = true };
                using (var w = XmlWriter.Create(folderPath + logListXmlFilename, settings))
                {
                    format.WriteObject(w, logList);
                }

                format = new DataContractSerializer(logList.VariablesDict.GetType());
                using (var w = XmlWriter.Create(folderPath + variablesDictXmlFilename, settings))
                {
                    format.WriteObject(w, logList.VariablesDict);
                }

                format = new DataContractSerializer(logList.FuncCallCountDict.GetType());
                using (var w = XmlWriter.Create(folderPath + funcCallCountDictXmlFilename, settings))
                {
                    format.WriteObject(w, logList.FuncCallCountDict);
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine("SaveToDisk failed: \n" + e.ToString());
            }
        }

        //folderpath includes the slash already
        public static void ReadFromDisk(string folderPath)
        {
            try
            {
                DataContractSerializer format = new DataContractSerializer(logList.GetType());
                if (File.Exists(folderPath + logListXmlFilename))
                {
                    using (var w = XmlReader.Create(folderPath + logListXmlFilename))
                    {
                        logList = (LogList)format.ReadObject(w);
                    }
                }

                format = new DataContractSerializer(logList.VariablesDict.GetType());
                if (File.Exists(folderPath + variablesDictXmlFilename))
                {
                    using (var w = XmlReader.Create(folderPath + variablesDictXmlFilename))
                    {
                        logList.VariablesDict = (Dictionary<string, string>)format.ReadObject(w);
                    }
                }

                format = new DataContractSerializer(logList.FuncCallCountDict.GetType());
                if (File.Exists(folderPath + funcCallCountDictXmlFilename))
                {
                    using (var w = XmlReader.Create(folderPath + funcCallCountDictXmlFilename))
                    {
                        logList.FuncCallCountDict = (Dictionary<string, int>)format.ReadObject(w);
                    }
                }
                
            }
            catch(Exception e)
            {
                Debug.WriteLine("ReadFromDisk failed: \n" + e.ToString());
            }
        }

       
        public static List<LogEntry> knownErrorsList = new List<LogEntry>();

        public static void ReadKnownErrorsFromDisk(string folderPath)
        {
            try
            {
                DataContractSerializer format = new DataContractSerializer(knownErrorsList.GetType());
                if (File.Exists(folderPath + knownErrorsXmlFilename))
                {
                    using (var w = XmlReader.Create(folderPath + knownErrorsXmlFilename))
                    {
                        knownErrorsList = (List<LogEntry>)format.ReadObject(w);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("ReadKnownErrorsFromDisk failed: \n" + e.ToString());
            }
        }

        public static void SaveKnownErrorsToDisk(string folderPath)
        {
            try
            {
                DataContractSerializer format = new DataContractSerializer(knownErrorsList.GetType());
                var settings = new XmlWriterSettings { Indent = true, NewLineOnAttributes = true };
                using (var w = XmlWriter.Create(folderPath + knownErrorsXmlFilename, settings))
                {
                    format.WriteObject(w, knownErrorsList);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("SaveKnownErrorsToDisk failed: \n" + e.ToString());
            }
        }

        public enum LOG_ENTRY_TYPE
        {
            VAR,
            MSG,
            ERROR,
            EVENT,
            FUNC,
            COUNT
        };

        [DataContract]
        public class LogEntry
        {
            [DataMember, DisplayName("timestamp"), Browsable(true)]
            public long timestamp { get; set; }
            [DataMember, DisplayName("Type"), Browsable(true)]
            public LOG_ENTRY_TYPE entryType { get; set; }
            //These provide information on the calling function and line number
            [DataMember, DisplayName("filename"), Browsable(true)]
            public string filename { get; set; }
            [DataMember, DisplayName("method"), Browsable(true)]
            public string methodname { get; set; }
            [DataMember, DisplayName("line"), Browsable(true)]
            public int linenumber { get; set; }
            [DataMember, DisplayName("msg"), Browsable(true)]
            public string msg { get; set; } //this is a well formatted string with contents

            public LogEntry(LOG_ENTRY_TYPE entryType,
                string filename, string methodname, int linenumber, string msg)
            {
                this.timestamp = GetCurrentTimeStamp();
                this.entryType = entryType;
                this.filename = Path.GetFileName(filename); //shortens to only the filename and not the full path
                this.methodname = methodname;
                this.linenumber = linenumber;
                this.msg = msg;
            }

            public static long GetCurrentTimeStamp()
            {
                return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            }

            public bool SimilarTo(LogEntry entry)
            {
                if(this.entryType == entry.entryType
                    && this.filename == entry.filename
                    && this.methodname == entry.methodname
                    && this.linenumber == entry.linenumber)
                {
                    //It's okay if the message is slightly different (such as used by a different variable)
                    return true;
                }
                return false;
            }
        }

        public class LogList : SortedList<long, LogEntry>
        {
            private static readonly object locker = new object();

            //Once ConfigManager.dataCaptureSecondsToKeep elapses, theis dict gets populated with the latest value for a given variable
            //Therefore, this dict will represent the state of variables before ConfigManager.dataCaptureSecondsToKeep, and thus
            //during data playback, the value of a variable at a given timestamp can be recovered
            //The key is of format: filename,Methodname,Variablename  (concatenated with comma separation)
            //The value is also a string which is the variable's value
            public Dictionary<string, string> VariablesDict = new Dictionary<string, string>();

            //Keeps track of how many times a given function that is being logged is called
            //The key is of format: filename,Methodname (concatenated with comma separation)
            //The value is the count
            public Dictionary<string, int> FuncCallCountDict = new Dictionary<string, int>();



            //If there are duplicate keys, the duplicate is marked as greater to avoid conflict
            public LogList() : base(new DuplicateKeyComparer<long>()) { }

            public void AddEntryThreadSafe(LogEntry logentry)
            {
                if(!loggingEnabled) { return; } //we don't add the entry to the list

                lock (locker)
                {
                    this.Add(logentry.timestamp, logentry);

                    //Remove all the "older" entries, and keep 5 seconds before that as sometimes those entries are helpful
                    long keepAsOfMs = LogEntry.GetCurrentTimeStamp() - (long)millisecondsToKeep - 5;
                    try
                    {
                        if (this.Count > 0)
                        {
                            while (this.ElementAt(0).Key < keepAsOfMs)
                            {
                                //This entry is older than we want, so remove it
                                RemoveEntryAndUpdateStatistics(0);
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine("AddEntryThreadSafe error: \n" + e.ToString());
                    }
                }
            }

            private void RemoveEntryAndUpdateStatistics(int index)
            {
                if (index >= this.Count) { return; }

                try
                {
                    LogEntry item = this.ElementAt(index).Value;

                    if (item.entryType == LOG_ENTRY_TYPE.VAR)
                    {
                        string[] msgsplit = item.msg.Split('=');
                        string varname = msgsplit[0];
                        string varval = msgsplit[1];
                        string varkey = item.filename + "," + item.methodname + "," + varname;
                        if (VariablesDict.ContainsKey(varkey))
                        {
                            //Update the value since this is more recent
                            VariablesDict[varkey] = varval;
                        }
                        else //key does not exist, therefore create
                        {
                            VariablesDict.Add(varkey, varval);
                        }
                    }
                    else if (item.entryType == LOG_ENTRY_TYPE.FUNC)
                    {
                        //We keep track of how many times this function has been called
                        string varkey = item.filename + "," + item.methodname;
                        if (FuncCallCountDict.ContainsKey(varkey))
                        {
                            //Another call has occurred
                            FuncCallCountDict[varkey] += 1;
                        }
                        else //key does not exist, therefore create
                        {
                            FuncCallCountDict.Add(varkey, 1);
                        }
                    }

                    //Remove the item from the list
                    this.RemoveAt(index);
                }
                catch(Exception e)
                {
                    Debug.WriteLine("RemoveEntryAndUpdateStatistics error: \n" + e.ToString());
                }
                
            }
        }

        /// <summary>
        /// Comparer for comparing two keys, handling equality as beeing greater
        /// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        public class DuplicateKeyComparer<TKey>
                        :
                     IComparer<TKey> where TKey : IComparable
        {
            #region IComparer<TKey> Members

            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1;   // Handle equality as beeing greater
                else
                    return result;
            }

            #endregion
        }
    }
}
