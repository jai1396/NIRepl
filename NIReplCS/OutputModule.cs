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
            StartExecution();
            RunningCommandNow = false;
        }

        private async void StartExecution()
        {
            //Initializes Execution state
            state = await CSharpScript.RunAsync("",
                ScriptOptions.Default.WithImports("System.IO"));
        }


        internal string GetLastOutput()
        {
            while (RunningCommandNow) ;
            return LastCommandOutput;

        }

        public async void RunCommand(string dispText)
        {            
            RunningCommandNow = true;

            if(RunningCommandNow)
            {
                //Gets state after execution of new code
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
        
    }
}


