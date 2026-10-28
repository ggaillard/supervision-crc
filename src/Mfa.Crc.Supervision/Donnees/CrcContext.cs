using Mfa.Crc.Supervision.Domaine;
using Microsoft.EntityFrameworkCore;

namespace Mfa.Crc.Supervision.Donnees;

public class CrcContext(DbContextOptions<CrcContext> options) : DbContext(options)
{
    public DbSet<FileTraitement> Files => Set<FileTraitement>();
    public DbSet<Conseiller> Conseillers => Set<Conseiller>();
    public DbSet<Societaire> Societaires => Set<Societaire>();
    public DbSet<Contrat> Contrats => Set<Contrat>();
    public DbSet<Sinistre> Sinistres => Set<Sinistre>();
    public DbSet<Reclamation> Reclamations => Set<Reclamation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileTraitement>(e =>
        {
            e.HasKey(f => f.Code);
            e.Property(f => f.Code).HasMaxLength(20);
            e.Property(f => f.Libelle).HasMaxLength(50);
        });

        modelBuilder.Entity<Conseiller>(e =>
        {
            e.Property(c => c.Id).HasMaxLength(3);
            e.Property(c => c.Nom).HasMaxLength(50);
            e.Property(c => c.Prenom).HasMaxLength(50);
            e.Property(c => c.Competences).HasMaxLength(100);
            e.Ignore(c => c.NomComplet);
        });

        modelBuilder.Entity<Societaire>(e =>
        {
            e.Property(s => s.Id).HasMaxLength(4);
            e.Property(s => s.Nom).HasMaxLength(50);
            e.Property(s => s.Prenom).HasMaxLength(50);
            e.Property(s => s.Ville).HasMaxLength(60);
        });

        modelBuilder.Entity<Contrat>(e =>
        {
            e.HasKey(c => c.Numero);
            e.Property(c => c.Numero).HasMaxLength(20);
            e.HasOne(c => c.Societaire).WithMany(s => s.Contrats).HasForeignKey(c => c.SocietaireId);
        });

        modelBuilder.Entity<Sinistre>(e =>
        {
            e.Property(s => s.Id).HasMaxLength(20);
            e.HasOne(s => s.Contrat).WithMany(c => c.Sinistres).HasForeignKey(s => s.ContratNumero);
        });

        modelBuilder.Entity<Reclamation>(e =>
        {
            e.Property(r => r.Id).HasMaxLength(20);
            e.HasOne(r => r.Societaire).WithMany(s => s.Reclamations).HasForeignKey(r => r.SocietaireId);
        });
    }
}
