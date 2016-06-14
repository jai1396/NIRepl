using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace NIReplCS
{
    public class OutputModule
    {
        //public Dictionary<string, ValueType> NameValueDict  { get; set; }
        //public Dictionary<string, SyntaxToken> NameTokenDict { get; set; }
        private Microsoft.CodeAnalysis.Scripting.ScriptState state { get; set; }
        private bool RunningCommandNow { get; set; }
        private string LastCommandOutput { get; set; }

        private MetadataReference mscorlib;

        private MetadataReference Mscorlib
        {
            get
            {
                if (mscorlib == null)
                {
                    mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                }

                return mscorlib;
            }
        }

        public OutputModule()
        {
            //NameValueDict = new Dictionary<string, ValueType>();
            //NameTokenDict = new Dictionary<string, SyntaxToken>();
            StartExecution();
            RunningCommandNow = false;
        }

        private async void StartExecution()
        {
            state = await CSharpScript.RunAsync("",
                ScriptOptions.Default.WithImports("System.IO"));
        }

//        private string GenSourceCode(string input)
//        {
//            return @"using System;
//class Program
//        {
//            static void Main()
//            {
//                " + input + @"
//            }
//        }";
//        }

        internal string GetLastOutput()
        {
            while (RunningCommandNow) ;
            return LastCommandOutput;

        }

        public async void RunCommand(string dispText)
        {
            //var source = GenSourceCode(dispText);
            //var tree = SyntaxFactory.ParseSyntaxTree(source);
            //var compilation = CSharpCompilation.Create("MyCommand", syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            //var model = compilation.GetSemanticModel(tree);

            RunningCommandNow = true;

            if(RunningCommandNow)
            {
                state = await state.ContinueWithAsync(dispText);
                StoreOutput(state.ReturnValue);
            }

            
        }

        private void StoreOutput(object returnValue)
        {
            if (returnValue!=null)
            {
                LastCommandOutput = returnValue.ToString();
            }
            else
            {
                LastCommandOutput = "";
            }
            
            RunningCommandNow = false;
        }

        //private object GetAllScopes(SyntaxTree tree)
        //{

        //}
    }
}

/*
 *  MemoryStream ms = new MemoryStream();
> string path = @"C:\Users\Janthony\Desktop\asd.txt";
> using (FileStream fs = new File.OpenRead(path))
. {
.     fs.CopyTo(ms);
. }
(1,33): error CS0426: The type name 'OpenRead' does not exist in the type 'File'
> using (FileStream fs = File.OpenRead(path))
. {
.     fs.CopyTo(ms);
. }
> ms.ToString()
"System.IO.MemoryStream"
> var sr = new StreamReader(ms);
> var mstr = sr.ReadToEnd();
> mstr
""
> Console.WriteLine(mstr);

> FileStream fs = new File.OpenRead(path);
(1,26): error CS0426: The type name 'OpenRead' does not exist in the type 'File'
> FileStream fs = File.OpenRead(path);
> var sr = new StreamReader(fs);
> var mstr = sr.ReadToEnd();
> mstr
"asdasdaddsadasdasd"

string path = @"C:\Users\Janthony\Desktop\asd.txt";
FileStream fs = File.OpenRead(path);
var sr = new StreamReader(fs);
var mstr = sr.ReadToEnd();
mstr
*/
