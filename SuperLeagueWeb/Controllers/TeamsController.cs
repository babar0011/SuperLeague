using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using SuperLeagueWeb.Hubs;
using SuperLeagueWeb.Models;

namespace SuperLeagueWeb.Controllers
{
    public class TeamsController : Controller
    {
        private readonly SuperContext _db;

        public TeamsController(SuperContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Team> objTeam = await _db.Teams.ToListAsync();
            return View(objTeam);
        }

        //Get
        public IActionResult Create()
        {
            return View();
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Team team)
        {
            if (ModelState.IsValid)
            {
                _db.Teams.Add(team);
                await _db.SaveChangesAsync();
                TempData["success"] = "Team Added Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        //Get
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var teamFromDb = await _db.Teams.FindAsync(id);
            if (teamFromDb == null)
            {
                return NotFound();
            }
            return View(teamFromDb);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Team team)
        {
            if (ModelState.IsValid)
            {
                _db.Teams.Update(team);
                await _db.SaveChangesAsync();
                TempData["success"] = "Team Updated Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        //Get
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var teamFromDb = await _db.Teams.FindAsync(id);
            if (teamFromDb == null)
            {
                return NotFound();
            }
            return View(teamFromDb);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int? id)
        {
            var obj = _db.Teams.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Teams.Remove(obj);
            await _db.SaveChangesAsync();
            TempData["success"] = "Team Deleted Successfully";
            return RedirectToAction(nameof(Index));
        }

        //Get
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var teamWithPlayers = await _db.Teams
                                           .Include(t => t.Players).ThenInclude(tp => tp.Country)
                                           .FirstOrDefaultAsync(t => t.Id == id);

            if (teamWithPlayers == null)
            {
                return NotFound();
            }

            return View(teamWithPlayers);
        }

        //Get
        public async Task<IActionResult> EditTeamPlayers(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var teamFromDb = await _db.Teams
                .Include(t => t.Players).ThenInclude(tp => tp.Country)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teamFromDb == null)
            {
                return NotFound();
            }

            var allPlayers = await _db.Players.OrderBy(p => p.Skill).Include(p => p.Category).Include(p => p.Country).ToListAsync();

            ViewBag.Team = teamFromDb;
            ViewBag.AllPlayers = allPlayers;

            return View(teamFromDb);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeamPlayers(int id, List<int> SelectedPlayerIds)
        {
            var teamFromDb = await _db.Teams
                .Include(t => t.Players)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teamFromDb == null)
            {
                return NotFound();
            }

            var selectedPlayers = await _db.Players.Where(p => SelectedPlayerIds.Contains(p.Id)).ToListAsync();
            if (selectedPlayers.Count != 5)
            {
                ModelState.AddModelError("", "You must select 5 Players in a team.");
            }

            var hasWicketKeeper = selectedPlayers.Any(p => p.Skill.ToLower() == "wicket-keeping");
            if (!hasWicketKeeper)
            {
                ModelState.AddModelError("", "You must select at least one Wicket-Keeper.");
            }

            var bowlerOrAllRounderCount = selectedPlayers.Count(p => p.Skill.ToLower() == "bowling" || p.Skill.ToLower() == "all-rounder");
            if (bowlerOrAllRounderCount < 2)
            {
                ModelState.AddModelError("", "You must select at least 2 bowlers or all-rounders (a mix of two is also allowed).");
            }

            if (ModelState.IsValid)
            {
                teamFromDb.Players.Clear();

                foreach (var player in selectedPlayers)
                {
                    if (player != null)
                    {
                        teamFromDb.Players.Add(player);
                    }
                }

                await _db.SaveChangesAsync();
                TempData["success"] = "Team Players Updated Successfully!";
                return RedirectToAction("Detail", new { id = teamFromDb.Id });
            }

            var allPlayers = await _db.Players.OrderBy(p => p.Skill).Include(p => p.Category).Include(p => p.Country).ToListAsync();
            ViewBag.Team = teamFromDb;
            ViewBag.AllPlayers = allPlayers;
            ViewBag.SelectedPlayers = selectedPlayers;
            return View(teamFromDb);
        }

        //Get
        public async Task<IActionResult> Match()
        {
            IEnumerable<Team> teamsFromDb = await _db.Teams
                .Include(t => t.Players).ThenInclude(tp => tp.Country)
                .ToListAsync();

            return View(teamsFromDb);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Match(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            IEnumerable<Team> teamsFromDb = await _db.Teams
                .Include(t => t.Players).ThenInclude(tp => tp.Country)
                .ToListAsync();

            var selectedTeam = await _db.Teams.FirstOrDefaultAsync(t => t.Id == id);
            if (selectedTeam == null)
            {
                return NotFound();
            }

            if (selectedTeam.Players.Count != 5)
            {
                ModelState.AddModelError("", "Selected Team is Incomplete, Add Players.");
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction("Start", new { id = selectedTeam.Id });
            }

            return View(teamsFromDb);
        }

        //Get
        public async Task<IActionResult> Start(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var teamFromDb = await _db.Teams
                .Include(t => t.Players)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teamFromDb == null)
            {
                return NotFound();
            }

            var allOpponentTeams = await _db.Teams
                .Include(t => t.Players)
                .Where(t => t.Id != id && t.Players.Count == 5)
                .ToListAsync();

            var opponentTeam = allOpponentTeams.OrderBy(r => Guid.NewGuid()).FirstOrDefault();

            if (opponentTeam == null)
            {
                TempData["error"] = "No suitable opponent team found. Please try again.";
                return RedirectToAction("Match");
            }

            ViewBag.OpponentTeam = opponentTeam;
            ViewBag.AllOpponentTeams = allOpponentTeams.Where(t => t.Id != opponentTeam.Id);

            return View(teamFromDb);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int? id, int? overs, int? tid)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var teamFromDb = await _db.Teams
                .Include(t => t.Players)
                .FirstOrDefaultAsync(t => t.Id == id);

            var opponentTeam = await _db.Teams
                .Include(t => t.Players)
                .FirstOrDefaultAsync(t => t.Id == tid);

            if (teamFromDb == null || opponentTeam == null)
            {
                return NotFound();
            }

            List<List<int>> oversData = new List<List<int>>();
            List<int> currentTeamPlayers = teamFromDb.Players.OrderBy(p =>
                p.Skill == "batting" ? 1 :
                p.Skill == "wicket-keeping" ? 2 :
                p.Skill == "all-rounder" ? 3 : 4)
                .Select(p => p.Id).ToList();

            List<int> opponentBowlers = opponentTeam
                .Players.Where(p => p.Skill == "bowling" || p.Skill == "all-rounder").Select(p => p.Id).ToList();

            int currentPlayerIndex = 0;
            int wicketCount = 0;
            int finalScore = 0;
            Random random = new Random();

            for (int i = 0; i < overs; i++)
            {
                List<int> balls = new List<int>();
                for (int j = 0; j < 6; j++)
                {
                    int run = random.Next(-1, 7);
                    balls.Add(run);
                    if (run == -1)
                    {
                        wicketCount++;
                        currentPlayerIndex++;
                        if (currentPlayerIndex >= currentTeamPlayers.Count)
                        {
                            balls.Add(-2);
                            break;
                        }
                        balls.Add(currentTeamPlayers[currentPlayerIndex]);
                    }
                    else
                    {
                        finalScore += run;
                    }
                    if (j == 5)
                    {
                        balls.Add(opponentBowlers[random.Next(opponentBowlers.Count)]);
                    }
                }
                oversData.Add(balls);
                if (wicketCount >= currentTeamPlayers.Count)
                {
                    break;
                }
            }

            ViewBag.OpponentTeam = opponentTeam;
            ViewBag.FinalScore = finalScore;
            ViewBag.OversData = oversData;

            return View(teamFromDb);
        }

        //Get
        public async Task<IActionResult> Challenge(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var teamFromDb = await _db.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (teamFromDb == null)
            {
                return NotFound();
            }

            var allOpponentTeams = await _db.Teams.Where(t => t.Id != id && t.Players.Count == 5).ToListAsync();

            ViewBag.AllOpponentTeams = allOpponentTeams;

            return View(teamFromDb);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartChallenge(int? id, int? tid, int? target)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var teamFromDb = await _db.Teams.FirstOrDefaultAsync(t => t.Id == id);

            var opponentTeam = await _db.Teams.FirstOrDefaultAsync(t => t.Id == tid);

            if (teamFromDb == null || opponentTeam == null)
            {
                return NotFound();
            }
            ViewBag.OpponentTeam = opponentTeam;
            ViewBag.Target = target + 1;

            return View(teamFromDb);
        }
    }
}
