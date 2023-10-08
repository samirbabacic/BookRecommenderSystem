using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace RecommendationMLDataService.Models
{
    public class BookRating
    {
        [LoadColumn(0)]
        public float UserId;
        [LoadColumn(1)]
        public string BookId;
        [LoadColumn(2)]
        public float Rating;
    }

    public class BookRatingPrediction
    {
        public float Score;
    }
}
