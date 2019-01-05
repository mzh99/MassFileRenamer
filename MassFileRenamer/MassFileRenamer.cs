using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using OCSS.Util.DirSearch;
using Microsoft.CodeAnalysis.Scripting;

namespace OCSS.MHMassFileRenamer {

   public class OldAndNewNames {
      public readonly string CurrentName;
      public readonly string NewName;

      public OldAndNewNames(string currName, string newName) {
         this.CurrentName = currName;
         this.NewName = newName;
      }
   }

   public class RenameResults {
      public long FileCount { get; set; }
      public long FolderCount { get; set; }
      public long ErrorCount { get; set; }
      public ReadOnlyCollection<OldAndNewNames> NameChanges {
         get { return nameChanges.AsReadOnly(); }
      }
      public ReadOnlyCollection<string> ErrorMessages {
         get { return errorList.AsReadOnly(); }
      }
      public void AddRename(string currName, string newName) {
         nameChanges.Add(new OldAndNewNames(currName, newName));
      }
      public void AddError(string err) {
         errorList.Add(err);
      }
      public int GetCurrentEntryCount {
         get {
            return nameChanges.Count;
         }
      }
      public void ClearOutputQueue() {
         nameChanges.Clear();
      }

      private List<OldAndNewNames> nameChanges;
      private List<string> errorList;

      public RenameResults() {
         this.FileCount = 0;
         this.FolderCount = 0;
         this.ErrorCount = 0;
         this.nameChanges = new List<OldAndNewNames>();
         this.errorList = new List<string>();
      }
   }

   public class MassFileRenamer {

      const long MAXERRORS_DEFAULT = 1000;
      const int MAX_QUEUE_OUTPUT_ENTRIES = 10000;

      private readonly string StartingPath;
      private readonly string FileMask;
      private readonly bool ProcessSubFolders;
      private readonly string RenameCode;
      private readonly IRenameFormatter RenameFormatter;
      private readonly string OutputFilename;
      private readonly long MaxErrorsToLog;
      private readonly bool SkipResultExactMatches;
      private readonly bool SkipHidden;
      private readonly bool SkipSystem;

      public RenameResults Results { get; private set; }

      public delegate void ProcessingFolder(string folder, ref bool CancelFlag);
      public event ProcessingFolder OnProcessingFolder;

      private DirSearch dirSearch = null;
      private int batchNum;
      private Script<string> script;

      public MassFileRenamer(string startPath, string mask, bool processSubs, string renameCode, bool skipResultExactMatches = false, bool excludeSys = true, bool excludeHidden = true, long maxErrsToLog = MAXERRORS_DEFAULT):
                        this(startPath, mask, processSubs, renameCode, null, null, skipResultExactMatches, excludeSys, excludeHidden, maxErrsToLog) { }

      public MassFileRenamer(string startPath, string mask, bool processSubs, string renameCode, IRenameFormatter renameFormatter, string outFileName, bool skipResultExactMatches = false, bool excludeSys = true, bool excludeHidden = true, long maxErrsToLog = MAXERRORS_DEFAULT) {
         if (string.IsNullOrWhiteSpace(renameCode))
            throw new ArgumentException("Rename code must be specified.");
         this.StartingPath = startPath;
         this.FileMask = string.IsNullOrEmpty(mask) ? DirSearch.MASK_ALL_FILES_AND_FOLDERS : mask;
         this.ProcessSubFolders = processSubs;
         this.RenameCode = renameCode;
         this.RenameFormatter = renameFormatter;
         this.OutputFilename = outFileName;
         this.MaxErrorsToLog = maxErrsToLog;
         this.SkipResultExactMatches = skipResultExactMatches;
         this.SkipHidden = excludeHidden;
         this.SkipSystem = excludeSys;
      }

      public RenameResults ProcessFiles() {
         string err = string.Empty;
         Results = new RenameResults();
         batchNum = 0;
         script = ScriptRunner.CompileScript(RenameCode, out err);
         if (script == null) {      // could not compile
            LogErr("Script code did not compile. Error: " + err);
         }
         else {
            dirSearch = new DirSearch(FileMask, StartingPath, AttrSearchType.stAny, DirSearch.ALLFILEATTRIB_MINUS_SYS_AND_HIDDEN, ProcessSubFolders);
            dirSearch.OnFileMatch += DirSearch_OnFileMatch;
            dirSearch.OnFolderMatch += DirSearch_OnFolderMatch;
            dirSearch.OnFileExcept += DirSearch_OnFileExcept;
            dirSearch.OnFolderExcept += DirSearch_OnFolderExcept;
            dirSearch.Execute();
            dirSearch.OnFileMatch -= DirSearch_OnFileMatch;
            dirSearch.OnFolderMatch -= DirSearch_OnFolderMatch;
            dirSearch.OnFileExcept -= DirSearch_OnFileExcept;
            dirSearch.OnFolderExcept -= DirSearch_OnFolderExcept;
         }
         OutputData(true);     // write any residual data collected for specific output types

         return Results;
      }

      private void LogErr(string err) {
         if (Results.ErrorCount >= MaxErrorsToLog)
            return;
         Results.AddError(err);
         Results.ErrorCount++;
      }

      private void DirSearch_OnFolderExcept(string errorMsg) {
         LogErr(errorMsg);
      }

      private void DirSearch_OnFileExcept(string errorMsg) {
         LogErr(errorMsg);
      }

      private void DirSearch_OnFolderMatch(DirectoryInfo OneFolder, ref bool CancelFlag) {
         if (CancelFlag)
            return;
         if (SkipBecauseOfAttribs(OneFolder.Attributes))
            return;
         Results.FolderCount++;
         OnProcessingFolder?.Invoke(OneFolder.FullName, ref CancelFlag);
      }

      private bool SkipBecauseOfAttribs(FileAttributes attributes) {
         if (SkipHidden && ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)) {
            return true;
         }
         if (SkipSystem && ((attributes & FileAttributes.System) == FileAttributes.System)) {
            return true;
         }
         return false;
      }

      private void DirSearch_OnFileMatch(FileInfo OneFile, ref bool CancelFlag) {
         if (SkipBecauseOfAttribs(OneFile.Attributes))
            return;
         if (CancelFlag)
            return;
         string err = string.Empty;
         string newName = ScriptRunner.ExecuteCode(script, new GlobalScriptVars(OneFile.FullName, OneFile, Results.FileCount + 1), out err);
         if (err == string.Empty) {    // code executed successfully
            string optTrailer = (OneFile.DirectoryName.EndsWith(Path.DirectorySeparatorChar.ToString())) ? "" : Path.DirectorySeparatorChar.ToString();
            string revisedName = OneFile.DirectoryName + optTrailer + newName;   // rebuild full file path
            if ((SkipResultExactMatches == false) || (OneFile.FullName.Equals(revisedName, StringComparison.CurrentCulture) == false)) {
               Results.FileCount++;
               Results.AddRename(OneFile.FullName, revisedName);
               OutputData(false);
            }
         }
         else {
            LogErr("Script did not execute. Error: " + err);
            CancelFlag = true;
         }
      }

      private void OutputData(bool forceWrite) {
         if (RenameFormatter == null)    // do not write data to disk if no RenameFormatter
            return;
         if (forceWrite == false && Results.GetCurrentEntryCount < MAX_QUEUE_OUTPUT_ENTRIES)
            return;
         StringBuilder sb = new StringBuilder();
         sb.AppendLine(RenameFormatter.FormatHeader());
         var resultList = Results.NameChanges;
         foreach (var item in resultList) {
            sb.AppendLine(RenameFormatter.FormatRename(item));
         }
         sb.AppendLine(RenameFormatter.FormatTrailer());
         if (batchNum == 0) {
            File.WriteAllText(OutputFilename, sb.ToString());
         }
         else {
            File.AppendAllText(OutputFilename, sb.ToString());
         }
         batchNum++;
         Debug.WriteLine($"Ouput file chunk {batchNum}");
         Results.ClearOutputQueue();
      }

   }

}
