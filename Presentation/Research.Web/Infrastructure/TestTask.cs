using System.Diagnostics;
using Research.Core.Infrastructure;

namespace Research.Web.Infrastructure
{
    public class TestTask : ITask
    {
        public void Execute()
        {
            Debug.WriteLine("Task dang chay ha ha ha ha");
        }
    }
}
