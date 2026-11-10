using Math_functions.Data;
using Math_Functions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Math_functions.Controllers;

public class FunktsiooniUurimineController : Controller
{
    private readonly AndmeKontekst _kontekst;
    public FunktsiooniUurimineController(AndmeKontekst kontekst) => _kontekst = kontekst;

    // READ: nimekiri
    public async Task<IActionResult> Index() =>
        View(await _kontekst.FunktsiooniUurimised.OrderByDescending(x => x.LuodudAeg).ToListAsync());

    // READ: detailvaade
    public async Task<IActionResult> Details(int? id)
    {
        var olem = id is null ? null : await _kontekst.FunktsiooniUurimised.FindAsync(id);
        return olem is null ? NotFound() : View(olem);
    }

    // CREATE
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Valem,Maaramispiirkond,Tuletis,KriitilisedPunktid,Ekstreemumid")] FunktsiooniUurimine olem)
    {
        if (!ModelState.IsValid) return View(olem);
        olem.LuodudAeg = DateTime.UtcNow;
        await _kontekst.FunktsiooniUurimised.AddAsync(olem);
        await _kontekst.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = olem.Id });
    }

    // UPDATE
    public async Task<IActionResult> Edit(int? id)
    {
        var olem = id is null ? null : await _kontekst.FunktsiooniUurimised.FindAsync(id);
        return olem is null ? NotFound() : View(olem);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,Valem,Maaramispiirkond,Tuletis,KriitilisedPunktid,Ekstreemumid")] FunktsiooniUurimine vorm)
    {
        if (id != vorm.Id) return NotFound();
        if (!ModelState.IsValid) return View(vorm);

        var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);
        if (olem is null) return NotFound();

        olem.Valem = vorm.Valem;
        olem.Maaramispiirkond = vorm.Maaramispiirkond;
        olem.Tuletis = vorm.Tuletis;
        olem.KriitilisedPunktid = vorm.KriitilisedPunktid;
        olem.Ekstreemumid = vorm.Ekstreemumid;   // LuodudAeg jääb samaks
        await _kontekst.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    // DELETE
    public async Task<IActionResult> Delete(int? id)
    {
        var olem = id is null ? null : await _kontekst.FunktsiooniUurimised.FindAsync(id);
        return olem is null ? NotFound() : View(olem);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);
        if (olem is not null)
        {
            _kontekst.FunktsiooniUurimised.Remove(olem);
            await _kontekst.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}