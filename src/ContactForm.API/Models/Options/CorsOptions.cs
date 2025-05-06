using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactForm.API.Models
{
    public class CorsOptions
    {
        public string[] Origins { get; set; } = Array.Empty<string>();
        public string[] Methods { get; set; } = Array.Empty<string>();
        public string[] Headers { get; set; } = Array.Empty<string>();
        public string[] ExposedHeaders { get; set; } = Array.Empty<string>();
        public bool AllowCredentials { get; set; } = false;
    }
}
