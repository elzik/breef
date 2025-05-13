using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elzik.Breef.Infrastructure
{
    public class AiServiceOptions
    {
        public required AiServiceProviders Provider { get; set; }
        public required string ModelId { get; set; }
        public required string EndpointUrl { get; set; }
        public required string ApiKey { get; set; }
    }
}
