using System;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using OCSS.StringUtil;

namespace OCSS.MHMassFileRenamer {

   //public class ScriptCompileResponse {
   //   public readonly string ReturnValue;
   //   public readonly string CompileErrors;

   //   public bool HasErrors { get { return CompileErrors != string.Empty; } }

   //   public ScriptCompileResponse(string retVal, string compErrs) {
   //      this.ReturnValue = retVal;
   //      this.CompileErrors = compErrs;
   //   }
   //}

   public class GlobalScriptVars {
      public readonly string FullName;
      public readonly string FileName;
      public readonly long FileNum;
      public readonly long FileLength;
      public readonly DateTime CreationTime;
      public readonly DateTime LastWriteTime;

      // OneFile.FullName, OneFile.Name
      public GlobalScriptVars(string fullName, FileInfo fileinfo, long fileNum) {
         this.FullName = fullName;
         this.FileName = fileinfo.Name;
         this.FileNum = fileNum;
         this.CreationTime = fileinfo.CreationTime;
         this.LastWriteTime = fileinfo.LastWriteTime;
         this.FileLength = fileinfo.Length;
      }

   }

   public static class ScriptRunner {

      public static string ExecuteCode(Script<string> script, GlobalScriptVars glob, out string err) {
         err = string.Empty;
         string ret = string.Empty;
         try {
            script.RunAsync(glob).ContinueWith(s => ret = s.Result.ReturnValue).Wait();
            return ret;
         }
         catch (CompilationErrorException e) {
            err = e.Message;
            return ret;
         }
      }

      public static Script<string> CompileScript(string code, out string err) {
         err = string.Empty;
         ScriptOptions opts = ScriptOptions.Default.AddReferences(typeof(MHString).Assembly).AddImports("System", "System.IO", "OCSS.StringUtil");
         try {
            var script = CSharpScript.Create<string>(code, opts, typeof(GlobalScriptVars));
            var diags = script.Compile();
            if (diags.Length > 0) {
               foreach (var diag in diags) {
                  err += diags[0].ToString() + "\r\n";
               }
               return null;
            }
            return script;
         }
         catch (CompilationErrorException e) {
            err = e.Message;
            return null;
         }
      }

   }

}
