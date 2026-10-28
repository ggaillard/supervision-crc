using Mfa.Crc.Supervision.TempsReel;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mfa.Crc.Supervision.Pages.Moniteurs;

public class FileAttenteModel(EtatCrc etat) : PageModel
{
    public IReadOnlyList<ContactEnAttente> Contacts { get; private set; } = [];

    public void OnGet()
    {
        Contacts = etat.FileAttente;
    }
}
