# Vos tests (séance 8)

Placez ici **vos** tests : au moins deux, dans une classe marquée `[Trait("Groupe", "etudiant")]`,
qui hérite de `TestCrc` (namespace `Mfa.Crc.Tests.Etudiant`).

Un test ne compte que s'il **échoue quand on casse** la règle qu'il protège : vérifiez-le en cassant
le code, puis notez dans un commentaire la modification qui le fait passer au rouge.

```bash
dotnet test --filter Groupe=etudiant
```
