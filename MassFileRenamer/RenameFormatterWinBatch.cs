using System.IO;

namespace OCSS.MHMassFileRenamer {

   public class RenameFormatterWinBatch: IRenameFormatter {

      public string FormatRename(OldAndNewNames rename) {
         return $"rename \"{rename.CurrentName}\" \"{Path.GetFileName(rename.NewName)}\"";
      }

      public string FormatHeader() {
         return "@echo off\r\nREM Change the next line to reflect where you want the log file\r\nSET LOGFILE=\"\\MyPreferredPath\\RenamesOutput.log\"\r\n@echo Processing renames on: %date% %time%  > %LOGFILE%\r\n";
      }

      public string FormatTrailer() {
         return "@echo Finished renames on: %date% %time%  >> %LOGFILE%\r\nPause";
      }

   }

}
