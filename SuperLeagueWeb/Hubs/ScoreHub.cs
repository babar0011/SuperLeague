using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SuperLeagueWeb.Models;

namespace SuperLeagueWeb.Hubs
{
    public class ScoreHub : Hub
    {
        private readonly SuperContext _db;

        public ScoreHub(SuperContext db)
        {
            _db = db;
        }

        public static int TotalRuns { get; set; } = 0;
        public static int Wickets { get; set; } = 0;
        public static int Balls { get; set; } = 0;
        public static int BatsmanIndex { get; set; } = 0;
        public static Player? Bowler { get; set; }

        private static readonly Random random = new();
        private static readonly int[] runsArray = { 0, 1, 2, 3, 4, 5, 6, -1 };

        public async Task StartInnings(int teamId, int opponentTeamId, int target)
        {
            Team? team = await _db.Teams.Include(t => t.Players).FirstOrDefaultAsync(t => t.Id == teamId);
            List<Player> players = team.Players.OrderBy(p => p.Skill == "batting" ? 1 : p.Skill == "wicket-keeping" ? 2 : p.Skill == "all-rounder" ? 3 : 4).ToList();

            Team? opponentTeam = await _db.Teams.Include(t => t.Players).FirstOrDefaultAsync(t => t.Id == opponentTeamId);
            List<Player> bowlers = opponentTeam.Players.Where(p => p.Skill == "bowling" || p.Skill == "all-rounder").ToList();
            Bowler = bowlers[random.Next(bowlers.Count)];

            while (Wickets < 5 && Balls != 30 && TotalRuns < target)
            {
                int run = runsArray[random.Next(runsArray.Length)];
                await UpdateScore(run, players, bowlers, team.Name, opponentTeam.Name);
                await Task.Delay(1000);
            }
        }

        public async Task UpdateScore(int run, List<Player> players, List<Player> bowlers, string teamA, string teamB)
        {
            if (run == -1)
            {
                Wickets++;
                if (BatsmanIndex != 4)
                {
                    BatsmanIndex++;
                }
            }
            else
            {
                TotalRuns += run;
            }
            Balls++;
            if (Balls % 6 == 0)
            {
                Bowler = bowlers[random.Next(bowlers.Count)];
            }
            string batsmanName = players[BatsmanIndex].Name;
            string batsmanImage = players[BatsmanIndex].ImageUrl;
            string bowlerName = Bowler.Name;
            string bowlerImage = Bowler.ImageUrl;
            await Clients.All.SendAsync("UpdateScore", TotalRuns, Wickets, Balls, run, batsmanName, batsmanImage, bowlerName, bowlerImage, teamA, teamB);
        }

        public void ResetScore()
        {
            TotalRuns = 0;
            Wickets = 0;
            Balls = 0;
            BatsmanIndex = 0;
        }
    }
}
