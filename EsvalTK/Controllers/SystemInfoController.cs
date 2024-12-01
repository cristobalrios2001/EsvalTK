using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EsvalTK.Controllers
{
    public class SystemInfoController : ControllerBase
    {
        [HttpGet("cpu-and-threads")]
        public IActionResult GetCpuAndThreadInfo()
        {
            int totalLogicalProcessors = Environment.ProcessorCount; // Total de núcleos lógicos
            var process = Process.GetCurrentProcess();
            int threadCount = process.Threads.Count;

            return Ok(new
            {
                TotalLogicalProcessors = totalLogicalProcessors,
                ThreadCount = threadCount
            });
        }
    }
}
