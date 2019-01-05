namespace OCSS.MHMassFileRenamer {

   public interface IRenameFormatter {
      string FormatHeader();     // for any file header, if needed
      string FormatRename(OldAndNewNames rename);     // for detail lines
      string FormatTrailer();    // for file trailer, if needed
   }

}
