using System;
using System.Collections.Generic;

namespace SuperLeagueWeb.Models
{
    public partial class Country
    {
        public Country()
        {
            Players = new HashSet<Player>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? FlagUrl { get; set; }
        public string? KitUrl { get; set; }

        public virtual ICollection<Player> Players { get; set; }
    }
}
