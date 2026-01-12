namespace dotnet.Models;

/// <summary>
/// Représente un labyrinthe en 2D avec des murs et des allées.
/// </summary>
public sealed class Maze
{
    /// <summary>
    /// Grille du labyrinthe : true = mur, false = allée
    /// </summary>
    public bool[,] Grid { get; }

    /// <summary>
    /// Coordonnées du point de départ (ligne, colonne)
    /// </summary>
    public (int Row, int Column) Start { get; }

    /// <summary>
    /// Coordonnées de la sortie (ligne, colonne)
    /// </summary>
    public (int Row, int Column) Exit { get; }

    /// <summary>
    /// Tableau des distances depuis le départ pour chaque case
    /// </summary>
    public int[,] Distances { get; }

    /// <summary>
    /// Queue contenant les cases à visiter avec leur distance
    /// </summary>
    public Queue<(int Distance, int Row, int Col)> ExplorationQueue { get; }

    public Maze(string input)
    {
        var lines = input.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var rows = lines.Length;
        var cols = lines[0].Length;

        Grid = new bool[rows, cols];
        Distances = new int[rows, cols];

        (int Row, int Column)? start = null;
        (int Row, int Column)? exit = null;

        // Parser le labyrinthe
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var currentChar = lines[row][col];

                switch (currentChar)
                {
                    case '#':
                        Grid[row, col] = true; // Mur
                        break;
                    case '.':
                        Grid[row, col] = false; // Allée
                        break;
                    case 'D':
                        Grid[row, col] = false; // Départ est une allée
                        start = (row, col);
                        break;
                    case 'S':
                        Grid[row, col] = false; // Sortie est une allée
                        exit = (row, col);
                        break;
                    default:
                        throw new ArgumentException($"Caractère invalide '{currentChar}' à la position ({row}, {col})");
                }

                // Initialiser les distances à -1 (pas encore visité)
                Distances[row, col] = -1;
            }
        }

        if (!start.HasValue)
        {
            throw new ArgumentException("Le labyrinthe doit contenir un point de départ 'D'");
        }

        if (!exit.HasValue)
        {
            throw new ArgumentException("Le labyrinthe doit contenir une sortie 'S'");
        }

        Start = start.Value;
        Exit = exit.Value;

        // Initialiser la file d'exploration avec le point de départ à distance 0
        ExplorationQueue = new Queue<(int Distance, int Row, int Col)>();
        ExplorationQueue.Enqueue((0, Start.Row, Start.Column));
    }

    /// <summary>
    /// Retourne la liste des cases voisines visitables pour une position donnée.
    /// Une case est visitable si :
    /// - Elle n'est pas un mur
    /// - Elle n'est pas le départ
    /// - Elle est dans les limites du labyrinthe
    /// </summary>
    public IList<(int Row, int Col)> GetNeighbours(int row, int col)
    {
        var neighbours = new List<(int Row, int Col)>();
        var rows = Grid.GetLength(0);
        var cols = Grid.GetLength(1);

        // Définir les 4 directions : haut, bas, gauche, droite
        var directions = new[]
        {
            (-1, 0), // Haut
            (1, 0),  // Bas
            (0, -1), // Gauche
            (0, 1)   // Droite
        };

        foreach (var (deltaRow, deltaCol) in directions)
        {
            var newRow = row + deltaRow;
            var newCol = col + deltaCol;

            // Vérifier si la case est dans les limites
            if (newRow < 0 || newRow >= rows || newCol < 0 || newCol >= cols)
            {
                continue;
            }

            // Vérifier si la case n'est pas un mur
            if (Grid[newRow, newCol])
            {
                continue;
            }

            // Vérifier si la case n'est pas le départ
            if ((newRow, newCol) == Start)
            {
                continue;
            }

            neighbours.Add((newRow, newCol));
        }

        return neighbours;
    }

    /// <summary>
    /// Traite la prochaine case de la file d'exploration.
    /// Retourne true si la sortie est atteinte, false sinon.
    /// </summary>
    public bool Fill()
    {
        // Vérifier qu'il y a des éléments dans la file
        if (ExplorationQueue.Count == 0)
        {
            return false;
        }

        // Sortir le premier élément de la file
        var (distance, row, col) = ExplorationQueue.Dequeue();

        // Si cette case a déjà une distance (>= 0), on l'ignore
        if (Distances[row, col] >= 0)
        {
            return false;
        }

        // Mettre à jour la distance de cette case
        Distances[row, col] = distance;

        // Si on a atteint la sortie, retourner true
        if ((row, col) == Exit)
        {
            return true;
        }

        // Obtenir les voisins et les ajouter à la file avec distance + 1
        var neighbours = GetNeighbours(row, col);
        foreach (var (neighbourRow, neighbourCol) in neighbours)
        {
            ExplorationQueue.Enqueue((distance + 1, neighbourRow, neighbourCol));
        }

        return false;
    }

    /// <summary>
    /// Calcule et retourne la distance minimale entre le départ et la sortie.
    /// Utilise l'algorithme BFS pour explorer le labyrinthe.
    /// </summary>
    public int GetDistance()
    {
        // Appeler Fill() en boucle jusqu'à atteindre la sortie
        var foundExit = false;
        while (ExplorationQueue.Count > 0 && !foundExit)
        {
            foundExit = Fill();
        }

        // Vérifier si on a atteint la sortie
        if (!foundExit)
        {
            throw new InvalidOperationException("La sortie n'est pas accessible depuis le départ");
        }

        // Retourner la distance de la sortie
        return Distances[Exit.Row, Exit.Column];
    }

    /// <summary>
    /// Retourne le chemin le plus court du départ à la sortie.
    /// Le chemin est calculé en partant de la sortie et en remontant vers le départ
    /// en cherchant à chaque fois le voisin avec une distance inférieure de 1.
    /// </summary>
    public IList<(int Row, int Col)> GetShortestPath()
    {
        // Vérifier que le labyrinthe a été résolu (la sortie a une distance >= 0)
        if (Distances[Exit.Row, Exit.Column] < 0)
        {
            throw new InvalidOperationException("Le labyrinthe n'a pas été résolu. Appelez GetDistance() d'abord.");
        }

        var path = new List<(int Row, int Col)>();
        var current = Exit;

        // Ajouter la sortie au chemin
        path.Add(current);

        // Remonter du Exit vers le Start en cherchant les cases avec distance - 1
        while (current != Start)
        {
            var currentDistance = Distances[current.Row, current.Column];
            var targetDistance = currentDistance - 1;

            // Chercher dans les voisins (incluant le départ cette fois)
            var neighbours = GetAllNeighbours(current.Row, current.Column);
            
            // Trouver le premier voisin avec la distance cible
            var found = false;
            foreach (var (neighbourRow, neighbourCol) in neighbours)
            {
                if (Distances[neighbourRow, neighbourCol] == targetDistance)
                {
                    current = (neighbourRow, neighbourCol);
                    path.Add(current);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw new InvalidOperationException("Impossible de retrouver le chemin (données incohérentes)");
            }
        }

        // Inverser le chemin pour avoir départ -> sortie
        path.Reverse();
        return path;
    }

    /// <summary>
    /// Retourne tous les voisins valides d'une case (incluant le départ, sans vérifier s'il est visité).
    /// Utilisé pour retrouver le chemin optimal.
    /// </summary>
    private IList<(int Row, int Col)> GetAllNeighbours(int row, int col)
    {
        var neighbours = new List<(int Row, int Col)>();
        var rows = Grid.GetLength(0);
        var cols = Grid.GetLength(1);

        // Définir les 4 directions : haut, bas, gauche, droite
        var directions = new[]
        {
            (-1, 0), // Haut
            (1, 0),  // Bas
            (0, -1), // Gauche
            (0, 1)   // Droite
        };

        foreach (var (deltaRow, deltaCol) in directions)
        {
            var newRow = row + deltaRow;
            var newCol = col + deltaCol;

            // Vérifier si la case est dans les limites
            if (newRow < 0 || newRow >= rows || newCol < 0 || newCol >= cols)
            {
                continue;
            }

            // Vérifier si la case n'est pas un mur
            if (Grid[newRow, newCol])
            {
                continue;
            }

            neighbours.Add((newRow, newCol));
        }

        return neighbours;
    }
}
