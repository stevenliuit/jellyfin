using System;

namespace MediaBrowser.Model.Dto
{
    public class RecommendationDto
    {
        public BaseItemDto[] Items { get; set; }

        public RecommendationType RecommendationType { get; set; }

        public string BaselineItemName { get; set; }

        public Guid CategoryId { get; set; }
    }
}
