using System;
using System.Collections.Generic;

namespace SuperLeagueWeb.Models
{
    public partial class Category
    {
        public Category()
        {
            Players = new HashSet<Player>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public DateTime CreatedDateTime { get; set; }

        public virtual ICollection<Player> Players { get; set; }
    }
}
