using Microsoft.VisualStudio.TestTools.UnitTesting;
using OCSS.MHMassFileRenamer;

namespace MFR.Tests {

   [TestClass]
   public class UnitTest1 {
      public static string ASIS_CODE = "FileName";
      public static string CHANGE_TOLOWER_CODE = "FileName.ToLower()";
      public static string RENAME_EXT_CODE = @"Path.ChangeExtension(FileName, "".dat"")";
      public static string MHSTRING1_CODE = "FileName.StrRight(6)";
      public static string BAD_CODE = "crash = true";

      public static string TEST_FILES_FOLDER = @"E:\Data\Source\CS\ClassLib\MassFileRenamer\MFR.Tests\TestDataFiles\";

      [TestMethod]
      public void SubTraversalIsSuccessful() {
         MassFileRenamer mfr = new MassFileRenamer(TEST_FILES_FOLDER, null, true, CHANGE_TOLOWER_CODE);
         var result = mfr.ProcessFiles();
         Assert.IsTrue(result.FileCount > 0, "file count is 0");
         Assert.IsTrue(result.FolderCount > 0, "folder count is 0");
         Assert.AreEqual(0, result.ErrorCount);
         Assert.IsTrue(result.GetCurrentEntryCount > 0, "Output entry count is 0");
      }

      [TestMethod]
      public void OneFolderIsSuccessful() {
         MassFileRenamer mfr = new MassFileRenamer(TEST_FILES_FOLDER, "*.txt", false, CHANGE_TOLOWER_CODE);
         var result = mfr.ProcessFiles();
         Assert.IsTrue(result.FileCount > 0, "file count is 0");
         Assert.IsTrue(result.FolderCount > 0, "folder count is 0");
         Assert.AreEqual(0, result.ErrorCount);
         Assert.IsTrue(result.GetCurrentEntryCount > 0, "Output entry count is 0");
      }

      [TestMethod]
      public void RootFolderIsSuccessful() {
         MassFileRenamer mfr = new MassFileRenamer(@"E:\", null, false, CHANGE_TOLOWER_CODE);
         var result = mfr.ProcessFiles();
         Assert.IsTrue(result.FileCount > 0, "file count is 0");
         Assert.IsTrue(result.FolderCount > 0, "folder count is 0");
         Assert.AreEqual(0, result.ErrorCount);
         Assert.IsTrue(result.GetCurrentEntryCount > 0, "Output entry count is 0");
      }

      [TestMethod]
      public void ReplaceExtensionIsSuccessful() {
         MassFileRenamer mfr = new MassFileRenamer(TEST_FILES_FOLDER, "*.txt", false, RENAME_EXT_CODE);
         var result = mfr.ProcessFiles();
         Assert.IsTrue(result.FileCount > 0, "file count is 0");
         Assert.IsTrue(result.FolderCount > 0, "folder count is 0");
         Assert.AreEqual(0, result.ErrorCount);
         Assert.IsTrue(result.GetCurrentEntryCount > 0, "Output entry count is 0");
      }

      [TestMethod]
      public void BadCodeFails() {
         MassFileRenamer mfr = new MassFileRenamer(TEST_FILES_FOLDER, "*.txt", false, BAD_CODE);
         var result = mfr.ProcessFiles();
         Assert.AreEqual(0, result.FileCount, "File count is not 0");
         Assert.AreEqual(0, result.FolderCount, "Folder count is 0");
         Assert.AreEqual(1, result.ErrorCount, "Error count not 0");
         Assert.IsTrue(result.GetCurrentEntryCount == 0, "Output entry count is non-zero");
      }

      [TestMethod]
      public void MHString1IsSuccessful() {
         MassFileRenamer mfr = new MassFileRenamer(TEST_FILES_FOLDER, "*.txt", false, MHSTRING1_CODE);
         var result = mfr.ProcessFiles();
         Assert.IsTrue(result.FileCount > 0, "file count is 0");
         Assert.IsTrue(result.FolderCount > 0, "folder count is 0");
         Assert.AreEqual(0, result.ErrorCount);
         Assert.IsTrue(result.GetCurrentEntryCount > 0, "Output entry count is 0");
      }

      [TestMethod]
      public void FileToTSVIsSuccessful() {
         MassFileRenamer mfr = new MassFileRenamer(TEST_FILES_FOLDER, "*.txt", true, CHANGE_TOLOWER_CODE, new RenameFormatterTSV(), @"e:\data\RenamesTSV.dat");
         var result = mfr.ProcessFiles();
         Assert.IsTrue(result.FileCount > 0, "file count is 0");
         Assert.IsTrue(result.FolderCount > 0, "folder count is 0");
         Assert.AreEqual(0, result.ErrorCount);
         Assert.AreEqual(0, result.GetCurrentEntryCount, "Output entry count is non 0");
      }

      [TestMethod]
      public void FileToWinBatchIsSuccessful() {
         MassFileRenamer mfr = new MassFileRenamer(TEST_FILES_FOLDER, "*.txt", true, CHANGE_TOLOWER_CODE, new RenameFormatterWinBatch(), @"e:\data\RenamesWinBatch.txt");
         var result = mfr.ProcessFiles();
         Assert.IsTrue(result.FileCount > 0, "file count is 0");
         Assert.IsTrue(result.FolderCount > 0, "folder count is 0");
         Assert.AreEqual(0, result.ErrorCount);
         Assert.AreEqual(0, result.GetCurrentEntryCount, "Output entry count is non 0");
      }

      [TestMethod]
      public void FileToUnixBatchIsSuccessful() {
         MassFileRenamer mfr = new MassFileRenamer(TEST_FILES_FOLDER, "*.txt", true, CHANGE_TOLOWER_CODE, new RenameFormatterUnixBatch(), @"e:\data\RenamesUnixBatch.txt");
         var result = mfr.ProcessFiles();
         Assert.IsTrue(result.FileCount > 0, "file count is 0");
         Assert.IsTrue(result.FolderCount > 0, "folder count is 0");
         Assert.AreEqual(0, result.ErrorCount);
         Assert.AreEqual(0, result.GetCurrentEntryCount, "Output entry count is non 0");
      }

      [TestMethod]
      public void ExactMatchesExcludesProperly() {
         // file is there but already lower case so this would be excluded from processed files
         MassFileRenamer mfr = new MassFileRenamer(TEST_FILES_FOLDER, "*.sql", false, CHANGE_TOLOWER_CODE, true);
         var result = mfr.ProcessFiles();
         Assert.AreEqual(0, result.FileCount, "file count is non 0");
         Assert.IsTrue(result.FolderCount > 0, "folder count is 0");
         Assert.AreEqual(0, result.ErrorCount);
         Assert.AreEqual(0, result.GetCurrentEntryCount, "Output entry count is non 0");
      }

   }

}
