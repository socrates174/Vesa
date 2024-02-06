using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vesa.Core.Abstractions
{
    public interface IEventMapping
    {
        string SourceType { get; set; }
        string TargetType { get; set; }
    }
}
