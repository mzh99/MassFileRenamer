namespace OCSS.MHMassFileRenamer {

   public class RenameFormatterTSV: IRenameFormatter {

      public string FormatRename(OldAndNewNames rename) {
         return ($"{rename.CurrentName}\t{rename.NewName}");
      }

      public string FormatHeader() {
         return ("Current File Name\tNew Name\r\n");
      }

      public string FormatTrailer() {
         return string.Empty;
      }

      //public string FormatRenameBatch(IEnumerable<OldAndNewNames> renames) {
      //   StringBuilder sb = new StringBuilder();
      //   foreach (var item in renames) {
      //      sb.AppendLine(FormatRename(item));
      //   }
      //   return sb.ToString();
      //}

   }

}
