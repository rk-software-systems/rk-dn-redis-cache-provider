using System;
using System.Collections.Generic;

namespace RKSoftware.Packages.Caching.Tests.Models
{
    public class BigObjectTestModel
    {
        public List<BigObjectTestItemModel> Data { get; set; }
    }

    public class BigObjectTestItemModel
    {
        public int? ComingMatchesID { get; set; }

        public string MatchTitle { get; set; }

        public string Description { get; set; }

        public DateTime? MatchStartDate { get; set; }

        public DateTime? MatchEndDate { get; set; }

        public bool? IsActive { get; set; }

        public bool? Followup { get; set; }

        public bool? Highlights { get; set; }

        public string CustomImage { get; set; }

        public bool? FireFan { get; set; }

        public string MatchTags { get; set; }

        public string PromoLinkLandscape { get; set; }

        public string PromoLinkPortrait { get; set; }

        public bool? MatchBG { get; set; }

        public string TournamentId { get; set; }

        public string TournamentName { get; set; }

        public string FirstTeamID { get; set; }

        public string FirstTeamName { get; set; }

        public string SecondTeamID { get; set; }

        public string SecondTeamName { get; set; }

        public string MatchSFU { get; set; }

        public string Id { get; set; }

        public DateTime? CreatedOnUTC { get; set; }

        public DateTime? UpdateOnUTC { get; set; }
    }
}
