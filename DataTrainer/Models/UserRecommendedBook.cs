using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommendationMLDataService.Models
{
    public class UserRecommendedBook
    {
        public string BookId { get; set; }
        public float Score { get; set; }
    }
}
