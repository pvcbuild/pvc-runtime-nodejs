using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcRuntime.NodeJs
{
    public class NodeJsInputData
    {
        public readonly Func<object, Task<object>> onStreamOutput = null;

        // Looks strange lowercase, I know.
        public readonly dynamic data = null;

        public readonly string scopeName = null;

        public NodeJsInputData(string scopeName, dynamic data)
        {
            this.data = data;
            this.scopeName = scopeName;
            this.onStreamOutput = (Func<object, Task<object>>)(async (message) =>
            {
                var trimmedMessage = message.ToString().Trim();
                if (trimmedMessage.Length > 0)
                    Console.Write(trimmedMessage);

                return message;
            });
        }
    }
}
