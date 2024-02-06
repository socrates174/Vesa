using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vesa.Kafka.Infrastructure
{
    public class IgnoreSerializer : ISerializer<Ignore>
    {
        public byte[] Serialize(Ignore data, SerializationContext context)
        {
            return null;
        }
    }
}
