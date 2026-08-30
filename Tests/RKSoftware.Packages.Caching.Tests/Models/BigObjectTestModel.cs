using System;
using System.Collections.Generic;

namespace RKSoftware.Packages.Caching.Tests.Models
{
    public class BigObjectTestModel
    {
        public required List<BigObjectTestItemModel> Data { get; set; }
    }

    public class BigObjectTestItemModel
    {
        public int? ComingMatchesID { get; set; }

        public required string MatchTitle { get; set; }

        public required string Description { get; set; }

        public DateTime? MatchStartDate { get; set; }

        public DateTime? MatchEndDate { get; set; }

        public bool? IsActive { get; set; }

        public bool? Followup { get; set; }

        public bool? Highlights { get; set; }

        public required string CustomImage { get; set; }

        public bool? FireFan { get; set; }

        public required string MatchTags { get; set; }

        public required string PromoLinkLandscape { get; set; }

        public required string PromoLinkPortrait { get; set; }

        public bool? MatchBG { get; set; }

        public required string TournamentId { get; set; }

        public required string TournamentName { get; set; }

        public required string FirstTeamID { get; set; }

        public required string FirstTeamName { get; set; }

        public required string SecondTeamID { get; set; }

        public required string SecondTeamName { get; set; }

        public required string MatchSFU { get; set; }

        public required string Id { get; set; }

        public DateTime? CreatedOnUTC { get; set; }

        public DateTime? UpdateOnUTC { get; set; }
    }
}
