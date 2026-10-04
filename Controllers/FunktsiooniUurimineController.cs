using Math_Functions.Data;
using Math_Functions.Models;
using Math_Functions.Teenused;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using ValidateAntiForgeryTokenAttribute = Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute;

namespace Math_Functions.Controllers
{
    public class FunktsiooniUurimineController : Controller
    {
        private readonly AndmeKontekst _kontekst;
        private readonly FunktsiooniArvutaja _arvutaja;

        public FunktsiooniUurimineController(AndmeKontekst kontekst, FunktsiooniArvutaja arvutaja)
        {
            _kontekst = kontekst;
            _arvutaja = arvutaja;
        }

        public async Task<IActionResult> Index()
        {
            var nimekiri = await _kontekst.FunktsiooniUurimised
                .OrderByDescending(x => x.LuodudAeg)
                .ToListAsync();
            return View(nimekiri);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id.Value);
            if (olem == null)
            {
                return NotFound();
            }
            return View(olem);
        }

        public IActionResult Create()
        {
            return View(new FunktsiooniUurimine());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Valem,Tuletis")] FunktsiooniUurimine olem)
        {
            KontrolliValemeid(olem);
            if (!ModelState.IsValid)
            {
                return View(olem);
            }

            _arvutaja.Analuusi(olem);

            await _kontekst.FunktsiooniUurimised.AddAsync(olem);
            await _kontekst.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = olem.Id });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id.Value);
            if (olem == null)
            {
                return NotFound();
            }
            return View(olem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Valem,Tuletis")] FunktsiooniUurimine vorm)
        {
            var olem = await _kontekst.FunktsiooniUurimised.FindAsync(vorm.Id);
            if (olem == null)
            {
                return NotFound();
            }

            KontrolliValemeid(vorm);
            if (!ModelState.IsValid)
            {
                return View(vorm);
            }

            olem.Valem = vorm.Valem;
            olem.Tuletis = vorm.Tuletis;
            _arvutaja.Analuusi(olem);

            await _kontekst.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = olem.Id });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id.Value);
            if (olem == null)
            {
                return NotFound();
            }
            return View(olem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteKinnitatud(int id)
        {
            var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);
            if (olem != null)
            {
                _kontekst.FunktsiooniUurimised.Remove(olem);
                await _kontekst.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Punktid(int id, [FromBody] double[] xVaartused)
        {
            var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);
            if (olem == null)
            {
                return NotFound();
            }

            if (xVaartused == null || xVaartused.Length == 0 || xVaartused.Length > 5000)
            {
                return BadRequest("Punktide arv peab olema 1 kuni 5000.");
            }

            var funktsioon = new double?[xVaartused.Length];
            var tuletis = new double?[xVaartused.Length];

            for (int i = 0; i < xVaartused.Length; i++)
            {
                funktsioon[i] = _arvutaja.ArvutaVaartus(olem, false, xVaartused[i]);
                tuletis[i] = _arvutaja.ArvutaVaartus(olem, true, xVaartused[i]);
            }

            return Json(new { funktsioon, tuletis });
        }

        private void KontrolliValemeid(FunktsiooniUurimine olem)
        {
            if (!string.IsNullOrWhiteSpace(olem.Valem) && !_arvutaja.ValemOnKorras(olem.Valem))
            {
                ModelState.AddModelError(nameof(olem.Valem),
                    "Valem on vigane. Näide: x^3 - 3*x (korrutamiseks kasuta *)");
            }

            if (!FunktsiooniArvutaja.TuletisOnTuhi(olem.Tuletis) && !_arvutaja.ValemOnKorras(olem.Tuletis))
            {
                ModelState.AddModelError(nameof(olem.Tuletis), "Tuletise valem on vigane");
            }
        }
    }
}