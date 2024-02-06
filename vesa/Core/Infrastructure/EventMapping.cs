using vesa.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vesa.Core.Infrastructure
{
    public class EventMapping : IEventMapping
    {
        public string SourceType { get; set; }
        public string TargetType { get; set; }
    }
}
