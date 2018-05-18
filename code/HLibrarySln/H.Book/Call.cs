using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class Call
    {
        public Call(string file, string func, TimeSpan timeLimit)
        {
            File = file;
            Function = func;
            TimeLimit = timeLimit;
        }
        
        public string File { get; set; }
        public string Function { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public DateTime WaitTime { get; set; }
        public DateTime ProcessTime { get; set; }
        public DateTime LeaveTime { get; set; }

        public override string ToString()
        {
            return $"File={File}, Function={Function}, TimeLimit={TimeLimit}, WaitTime={WaitTime.ToLongTimeString()}, ProcessTime={ProcessTime.ToLongTimeString()}, LeaveTime={LeaveTime.ToLongTimeString()}";
        }
    }
}
