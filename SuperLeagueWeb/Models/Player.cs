using System;
using System.Collections.Generic;

namespace SuperLeagueWeb.Models
{
    public partial class Player
    {
        public Player()
        {
            Teams = new HashSet<Team>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Skill { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public int CountryId { get; set; }

        public virtual Category? Category { get; set; }
        public virtual Country? Country { get; set; }

        public virtual ICollection<Team> Teams { get; set; }
    }
}
