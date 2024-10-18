using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperLeagueWeb.Models;

namespace SuperLeagueWeb.Controllers
{
    public class PlayersController : Controller
    {
        private readonly SuperContext _db;

        public PlayersController(SuperContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Player> objPlayers = await _db.Players.Include(p => p.Category).Include(p => p.Country)
                .ToListAsync();
            return View(objPlayers);
        }

        //Get
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _db.Categories.ToListAsync();
            ViewBag.Countries = await _db.Countries.ToListAsync();
            return View(new Player());
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Player player)
        {
            if (ModelState.IsValid)
            {
                _db.Players.Add(player);
                await _db.SaveChangesAsync();
                TempData["success"] = "Player Added Successfully";
                return RedirectToAction(nameof(Index));
            }
            if (player.Skill == "Select Skill")
            {
                ModelState.AddModelError("skill", "The Skill field is required");
            }
            ViewBag.Categories = await _db.Categories.ToListAsync();
            ViewBag.Countries = await _db.Countries.ToListAsync();
            return View(player);
        }

        //Get
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var playerFromDb = await _db.Players.FindAsync(id);
            if (playerFromDb == null)
            {
                return NotFound();
            }
            ViewBag.Categories = await _db.Categories.ToListAsync();
            ViewBag.Countries = await _db.Countries.ToListAsync();
            return View(playerFromDb);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Player player)
        {
            if (ModelState.IsValid)
            {
                _db.Players.Update(player);
                await _db.SaveChangesAsync();
                TempData["success"] = "Player Updated Successfully";
                return RedirectToAction(nameof(Index));
            }
            if (player.Skill == "Select Skill")
            {
                ModelState.AddModelError("skill", "The Skill field is required");
            }
            ViewBag.Categories = await _db.Categories.ToListAsync();
            ViewBag.Countries = await _db.Countries.ToListAsync();
            return View(player);
        }

        //Get
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var playerFromDb = await _db.Players.FindAsync(id);
            if (playerFromDb == null)
            {
                return NotFound();
            }
            ViewBag.Categories = await _db.Categories.ToListAsync();
            ViewBag.Countries = await _db.Countries.ToListAsync();
            return View(playerFromDb);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int? id)
        {
            var obj = _db.Players.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Players.Remove(obj);
            await _db.SaveChangesAsync();
            TempData["success"] = "Player Deleted Successfully";
            return RedirectToAction(nameof(Index));
        }

        //Get
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var playerFromDb = await _db.Players
                .Include(p => p.Category)
                .Include(p => p.Country)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (playerFromDb == null)
            {
                return NotFound();
            }
            return View(playerFromDb);
        }
    }
}
