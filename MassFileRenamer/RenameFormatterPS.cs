using System.IO;

namespace OCSS.MHMassFileRenamer {

   public class RenameFormatterPS: IRenameFormatter {

      public string FormatRename(OldAndNewNames rename) {
         return $"Rename-Item -Path \"{rename.CurrentName}\" -NewName \"{Path.GetFileName(rename.NewName)}\"";
      }

      public string FormatHeader() {
         return string.Empty;
      }

      public string FormatTrailer() {
         return string.Empty;
      }

   }

}
