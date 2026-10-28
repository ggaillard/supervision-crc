using Mfa.Crc.Supervision.Domaine;
using Mfa.Crc.Supervision.Donnees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Mfa.Crc.Supervision.Pages;

public class SocietairesModel(CrcContext db) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Nom { get; set; }

    public List<Societaire> Resultats { get; private set; } = [];

    public void OnGet()
    {
        if (Nom is null) return;

        // Recherche « contient », insensible à la casse (les noms sont en majuscules en base)
        var sql = "SELECT * FROM Societaires WHERE Nom LIKE '%" + Nom.Trim().ToUpperInvariant() + "%' ORDER BY Nom";
        Resultats = db.Societaires.FromSqlRaw(sql).Include(s => s.Contrats).ToList();
    }
}
