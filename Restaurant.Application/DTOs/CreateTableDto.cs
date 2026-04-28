using System;
using System.Collections.Generic;
using System.Text;

namespace Restaurant.Application.DTOs
{
    public class CreateTableDto
    {
        public int TableNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
