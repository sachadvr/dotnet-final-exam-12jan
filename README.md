Bonjour voici mon rendu pour l'examen de dotnet Master 2 Cyber 
C'est un projet blazor (j'avais pris ça comme stack au départ) -> il faudra rajouter des options pour l'exo 2 par exemple

J'ai fait 3 branches exo1,exo2,exo3 

Voici mon dernier commit sur exo3& main : 7a4a925a8ec6a1f1d8f689781d4175f701b8682a

La branche "main" correspond à l'exo 1, exo2, exo3, j'ai cherry-pick les commits pour que ça soit plus simple

Pour lancer les tests:
```sh
cd dotnet.Tests && dotnet test
```

Pour lancer le projet on a juste à faire (Exo1)
```sh
dotnet run
```

Pour lancer le projet (Exo2) avec le labyrinthe:
```sh
dotnet run -- --maze test_maze.txt #on peut changer test_maze par n'importe quel fichier texte qui correspond à un labyrinthe (cf sujet)
```

Pour l'exo 3, c'est la même chose que l'exo 1 

Bonne correction ;) 
