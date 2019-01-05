namespace OCSS.MHMassFileRenamer {

   public class RenameFormatterUnixBatch: IRenameFormatter {

      public string FormatRename(OldAndNewNames rename) {
         return $"mv \"{rename.CurrentName}\" \"{rename.NewName}\"";
      }

      public string FormatHeader() {
         return string.Empty;
      }

      public string FormatTrailer() {
         return string.Empty;
      }


   }

}
