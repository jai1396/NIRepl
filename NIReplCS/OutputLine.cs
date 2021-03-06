﻿namespace NIReplCS
{
    public class OutputLine
    {
        //Class is a wrapper for output lines
        private string execResult;
        public string ExecResult
        {
            get
            {
                return "> " + this.execResult;
            }
            set
            {
                this.execResult = value;
            }
        }

        public OutputLine(string s)
        {
            if (s!=null)
            {
                ExecResult = s;
            }
            else
            {
                ExecResult = "";
            }
        }
    }
}