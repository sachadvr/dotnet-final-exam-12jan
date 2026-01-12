using dotnet.Models;
using Xunit;

namespace dotnet.Tests;

public sealed class MazeTests
{
    private const string ExampleMaze = """
        D..#.
        ##...
        .#.#.
        ...#.
        ####S
        """;

    [Fact]
    public void Constructor_WithValidMaze_ShouldSetStartAndExitCorrectly()
    {
        var labyrinth = new Maze(ExampleMaze);

        Assert.Equal((0, 0), labyrinth.Start);
        Assert.Equal((4, 4), labyrinth.Exit);
    }

    [Fact]
    public void Constructor_WithValidMaze_ShouldParseGridCorrectly()
    {

        var labyrinth = new Maze(ExampleMaze);

        // Vérifier quelques cases spécifiques
        // D (départ) à (0,0) doit être considéré comme allée (false = pas de mur)
        Assert.False(labyrinth.Grid[0, 0]);
        
        // . (allée) à (0,1) doit être une allée
        Assert.False(labyrinth.Grid[0, 1]);
        
        // # (mur) à (0,3) doit être un mur (true = mur)
        Assert.True(labyrinth.Grid[0, 3]);
        
        // # (mur) à (1,0) doit être un mur
        Assert.True(labyrinth.Grid[1, 0]);
        
        // # (mur) à (1,1) doit être un mur
        Assert.True(labyrinth.Grid[1, 1]);
        
        // . (allée) à (1,2) doit être une allée
        Assert.False(labyrinth.Grid[1, 2]);
        
        // S (sortie) à (4,4) doit être considéré comme allée (false = pas de mur)
        Assert.False(labyrinth.Grid[4, 4]);
    }

    [Fact]
    public void Constructor_WithValidMaze_ShouldInitializeDistancesArrayWithCorrectSize()
    {
        var labyrinth = new Maze(ExampleMaze);

        // Vérifier que le tableau Distances a la bonne taille (5x5)
        Assert.Equal(5, labyrinth.Distances.GetLength(0)); // Nombre de lignes
        Assert.Equal(5, labyrinth.Distances.GetLength(1)); // Nombre de colonnes
    }

    [Fact]
    public void Constructor_WithValidMaze_ShouldInitializeAllDistancesToMinusOne()
    {
        var labyrinth = new Maze(ExampleMaze);

        // Vérifier que toutes les cases sont initialisées à -1 (pas encore visitées)
        for (int row = 0; row < labyrinth.Distances.GetLength(0); row++)
        {
            for (int col = 0; col < labyrinth.Distances.GetLength(1); col++)
            {
                Assert.Equal(-1, labyrinth.Distances[row, col]);
            }
        }
    }

    [Theory]
    [InlineData("D\nS", 0, 0, 1, 0)]
    [InlineData("D.\n.S", 0, 0, 1, 1)]
    [InlineData("D..\n..S", 0, 0, 1, 2)]
    public void Constructor_WithDifferentLayouts_ShouldIdentifyStartAndExitCorrectly(
        string layout, 
        int expectedStartRow, 
        int expectedStartCol, 
        int expectedExitRow, 
        int expectedExitCol)
    {
        var labyrinth = new Maze(layout);

        Assert.Equal((expectedStartRow, expectedStartCol), labyrinth.Start);
        Assert.Equal((expectedExitRow, expectedExitCol), labyrinth.Exit);
    }

    [Fact]
    public void Constructor_WithComplexMaze_ShouldParseWallsAndPathsCorrectly()
    {
        var complexMaze = """
            D#.
            .#.
            ..S
            """;

        var labyrinth = new Maze(complexMaze);

        Assert.False(labyrinth.Grid[0, 0]); // D = allée
        Assert.True(labyrinth.Grid[0, 1]);  // # = mur
        Assert.False(labyrinth.Grid[0, 2]); // . = allée
        Assert.False(labyrinth.Grid[1, 0]); // . = allée
        Assert.True(labyrinth.Grid[1, 1]);  // # = mur
        Assert.False(labyrinth.Grid[1, 2]); // . = allée
        Assert.False(labyrinth.Grid[2, 0]); // . = allée
        Assert.False(labyrinth.Grid[2, 1]); // . = allée
        Assert.False(labyrinth.Grid[2, 2]); // S = allée
    }

    [Fact]
    public void Constructor_WithRectangularMaze_ShouldHandleDifferentDimensions()
    {
        var rectangularMaze = """
            D...
            ####
            ...S
            """;

        var labyrinth = new Maze(rectangularMaze);

        Assert.Equal((0, 0), labyrinth.Start);
        Assert.Equal((2, 3), labyrinth.Exit);
        Assert.Equal(3, labyrinth.Distances.GetLength(0)); // 3 lignes
        Assert.Equal(4, labyrinth.Distances.GetLength(1)); // 4 colonnes
    }

    #region GetNeighbours Tests

    [Fact]
    public void GetNeighbours_WithCenterCellAndAllNeighboursAvailable_ShouldReturnFourNeighbours()
    {
        // Arrange - Case au centre avec tous les voisins disponibles
        var labyrinth = new Maze("""
            D....
            .....
            .....
            .....
            ....S
            """);

        //  Obtenir les voisins de la case (2,2)
        var neighbours = labyrinth.GetNeighbours(2, 2);

        // Doit retourner 4 voisins (haut, bas, gauche, droite)
        Assert.Equal(4, neighbours.Count);
        Assert.Contains((1, 2), neighbours); // Haut
        Assert.Contains((3, 2), neighbours); // Bas
        Assert.Contains((2, 1), neighbours); // Gauche
        Assert.Contains((2, 3), neighbours); // Droite
    }

    [Fact]
    public void GetNeighbours_WithWallNeighbour_ShouldNotIncludeWall()
    {
        // Arrange - Case avec un mur à droite
        var labyrinth = new Maze("""
            D....
            .....
            ...#.
            .....
            ....S
            """);

        //  Obtenir les voisins de la case (2,2)
        var neighbours = labyrinth.GetNeighbours(2, 2);

        // Doit retourner 3 voisins (pas le mur à droite)
        Assert.Equal(3, neighbours.Count);
        Assert.Contains((1, 2), neighbours); // Haut
        Assert.Contains((3, 2), neighbours); // Bas
        Assert.Contains((2, 1), neighbours); // Gauche
        Assert.DoesNotContain((2, 3), neighbours); // Droite (mur) ne doit pas être inclus
    }

    [Fact]
    public void GetNeighbours_WithStartNeighbour_ShouldNotIncludeStart()
    {
        // Arrange - Case adjacente au départ
        var labyrinth = new Maze("""
            D....
            .....
            .....
            .....
            ....S
            """);

        //  Obtenir les voisins de la case (1,0) qui est juste en dessous du départ
        var neighbours = labyrinth.GetNeighbours(1, 0);

        // Doit retourner 2 voisins (pas le départ en haut)
        Assert.Equal(2, neighbours.Count);
        Assert.DoesNotContain((0, 0), neighbours); // Haut (départ) ne doit pas être inclus
        Assert.Contains((2, 0), neighbours); // Bas
        Assert.Contains((1, 1), neighbours); // Droite
    }

    [Fact]
    public void GetNeighbours_OnLeftBorder_ShouldNotIncludeOutOfBounds()
    {
        // Arrange - Case sur la bordure gauche (col=0), loin du départ
        var labyrinth = new Maze("""
            D....
            .....
            .....
            .....
            ....S
            """);

        //  Obtenir les voisins de la case (2,0)
        var neighbours = labyrinth.GetNeighbours(2, 0);

        // Doit retourner 3 voisins (pas de voisin à gauche hors limites)
        Assert.Equal(3, neighbours.Count);
        Assert.Contains((1, 0), neighbours); // Haut
        Assert.Contains((3, 0), neighbours); // Bas
        Assert.Contains((2, 1), neighbours); // Droite
        // Pas de voisin à gauche (hors limites)
    }

    [Fact]
    public void GetNeighbours_OnRightBorder_ShouldNotIncludeOutOfBounds()
    {
        // Arrange - Case sur la bordure droite (col=4)
        var labyrinth = new Maze("""
            D....
            .....
            .....
            .....
            ....S
            """);

        //  Obtenir les voisins de la case (1,4)
        var neighbours = labyrinth.GetNeighbours(1, 4);

        // Doit retourner 3 voisins (pas de voisin à droite hors limites)
        Assert.Equal(3, neighbours.Count);
        Assert.Contains((0, 4), neighbours); // Haut
        Assert.Contains((2, 4), neighbours); // Bas
        Assert.Contains((1, 3), neighbours); // Gauche
        // Pas de voisin à droite (hors limites)
    }

    [Fact]
    public void GetNeighbours_OnTopBorder_ShouldNotIncludeOutOfBounds()
    {
        // Arrange - Case sur la bordure haute (row=0)
        var labyrinth = new Maze("""
            D....
            .....
            .....
            .....
            ....S
            """);

        //  Obtenir les voisins de la case (0,2)
        var neighbours = labyrinth.GetNeighbours(0, 2);

        // Doit retourner 3 voisins (pas de voisin en haut hors limites)
        Assert.Equal(3, neighbours.Count);
        Assert.Contains((1, 2), neighbours); // Bas
        Assert.Contains((0, 1), neighbours); // Gauche
        Assert.Contains((0, 3), neighbours); // Droite
        // Pas de voisin en haut (hors limites)
    }

    [Fact]
    public void GetNeighbours_OnBottomBorder_ShouldNotIncludeOutOfBounds()
    {
        // Arrange - Case sur la bordure basse (row=4)
        var labyrinth = new Maze("""
            D....
            .....
            .....
            .....
            ....S
            """);

        //  Obtenir les voisins de la case (4,2)
        var neighbours = labyrinth.GetNeighbours(4, 2);

        // Doit retourner 3 voisins (pas de voisin en bas hors limites)
        Assert.Equal(3, neighbours.Count);
        Assert.Contains((3, 2), neighbours); // Haut
        Assert.Contains((4, 1), neighbours); // Gauche
        Assert.Contains((4, 3), neighbours); // Droite
        // Pas de voisin en bas (hors limites)
    }

    #endregion

    #region Fill Tests

    [Fact]
    public void Constructor_ShouldInitializeQueueWithStartPositionAtDistanceZero()
    {
        var labyrinth = new Maze(ExampleMaze);

        Assert.NotEmpty(labyrinth.ExplorationQueue);
        Assert.Equal(1, labyrinth.ExplorationQueue.Count);
        
        var firstElement = labyrinth.ExplorationQueue.Peek();
        Assert.Equal(0, firstElement.Distance);
        Assert.Equal(0, firstElement.Row);
        Assert.Equal(0, firstElement.Col);
    }

    [Fact]
    public void Fill_WhenNotReachedExit_ShouldReturnFalse()
    {
        // Arrange - Un grand labyrinthe où on n'atteindra pas la sortie en un seul Fill
        var labyrinth = new Maze("""
            D....
            .....
            .....
            .....
            ....S
            """);

        //  Premier appel à Fill (traite le départ)
        var result = labyrinth.Fill();

        Assert.False(result);
    }

    [Fact]
    public void Fill_WhenReachedExit_ShouldReturnTrue()
    {
        // Arrange - Un très petit labyrinthe où la sortie est proche
        var labyrinth = new Maze("""
            DS
            """);

        //  Appeler Fill jusqu'à atteindre la sortie
        var result = false;
        while (!result && labyrinth.ExplorationQueue.Count > 0)
        {
            result = labyrinth.Fill();
        }

        Assert.True(result);
        Assert.Equal(1, labyrinth.Distances[0, 1]); // La sortie doit avoir une distance de 1
    }

    [Fact]
    public void Fill_WithAlreadyVisitedCell_ShouldIgnoreAndNotAddNeighbours()
    {
        // Arrange - Mazee simple
        var labyrinth = new Maze("""
            D..
            ...
            ..S
            """);

        //  Appeler Fill() plusieurs fois pour traiter plusieurs cases
        labyrinth.Fill(); // Traite (0,0) - départ avec distance 0
        labyrinth.Fill(); // Traite un voisin avec distance 1
        labyrinth.Fill(); // Traite un autre voisin avec distance 1
        
        // Trouver une case qui a été visitée
        int visitedRow = -1, visitedCol = -1;
        for (int row = 0; row < labyrinth.Distances.GetLength(0); row++)
        {
            for (int col = 0; col < labyrinth.Distances.GetLength(1); col++)
            {
                if (labyrinth.Distances[row, col] >= 0 && (row, col) != labyrinth.Start)
                {
                    visitedRow = row;
                    visitedCol = col;
                    break;
                }
            }
            if (visitedRow >= 0) break;
        }
        
        Assert.True(visitedRow >= 0, "Devrait avoir au moins une case visitée");
        
        var oldDistance = labyrinth.Distances[visitedRow, visitedCol];
        var queueCountBefore = labyrinth.ExplorationQueue.Count;
        
        // Ajouter manuellement cette case déjà visitée à la queue
        labyrinth.ExplorationQueue.Enqueue((999, visitedRow, visitedCol));
        
        // Vider la queue jusqu'à cette case
        while (labyrinth.ExplorationQueue.Count > 0)
        {
            var current = labyrinth.ExplorationQueue.Peek();
            if (current.Row == visitedRow && current.Col == visitedCol && current.Distance == 999)
            {
                var result = labyrinth.Fill();
                
                // Le Fill doit retourner false car case déjà visitée
                Assert.False(result);
                // La distance ne doit pas changer
                Assert.Equal(oldDistance, labyrinth.Distances[visitedRow, visitedCol]);
                break;
            }
            labyrinth.Fill();
        }
    }

    [Fact]
    public void Fill_ShouldAddNeighboursToQueueWithIncrementedDistance()
    {
        var labyrinth = new Maze("""
            D....
            .....
            .....
            .....
            ....S
            """);

        //  Traiter le départ
        var initialQueueCount = labyrinth.ExplorationQueue.Count; // Devrait être 1 (le départ)
        Assert.Equal(1, initialQueueCount);
        
        labyrinth.Fill(); // Traite (0,0) avec distance 0
        
        // Vérifier que le départ a maintenant une distance de 0
        Assert.Equal(0, labyrinth.Distances[0, 0]);
        
        // La queue devrait maintenant contenir les voisins du départ avec distance 1
        // Le départ (0,0) a 2 voisins : (1,0) en bas et (0,1) à droite
        Assert.Equal(2, labyrinth.ExplorationQueue.Count);
        
        // Vérifier que tous les éléments de la queue ont une distance de 1
        var queueElements = labyrinth.ExplorationQueue.ToList();
        Assert.All(queueElements, element => Assert.Equal(1, element.Distance));
        
        // Vérifier que les voisins corrects sont dans la queue
        Assert.Contains(queueElements, e => e.Row == 1 && e.Col == 0); // Bas
        Assert.Contains(queueElements, e => e.Row == 0 && e.Col == 1); // Droite
    }

    [Fact]
    public void Fill_CompleteMazeSolving_ShouldCalculateCorrectDistances()
    {
        // Arrange - Un petit labyrinthe simple pour vérification complète
        var labyrinth = new Maze("""
            D..
            ...
            ..S
            """);

        //  Résoudre complètement le labyrinthe
        var foundExit = false;
        while (labyrinth.ExplorationQueue.Count > 0 && !foundExit)
        {
            foundExit = labyrinth.Fill();
        }

        Assert.True(foundExit);
        
        // Vérifier quelques distances clés
        Assert.Equal(0, labyrinth.Distances[0, 0]); // Départ
        Assert.Equal(1, labyrinth.Distances[0, 1]); // Voisin direct du départ
        Assert.Equal(1, labyrinth.Distances[1, 0]); // Voisin direct du départ
        Assert.Equal(4, labyrinth.Distances[2, 2]); // Sortie (chemin le plus court)
    }

    #endregion

    #region GetDistance Tests

    [Fact]
    public void GetDistance_WithSimpleMaze_ShouldReturnCorrectDistance()
    {
        // Arrange - Mazee simple 3x3 sans murs
        var labyrinth = new Maze("""
            D..
            ...
            ..S
            """);
        
        // Calcul manuel du chemin optimal :
        // D(0,0) -> .(0,1) -> .(0,2) -> .(1,2) -> .(2,2) S
        // Distance = 4

        var distance = labyrinth.GetDistance();

        Assert.Equal(4, distance);
    }

    [Fact]
    public void GetDistance_WithComplexMazeWithWalls_ShouldReturnCorrectDistance()
    {
        // Arrange - Mazee avec murs (exemple du début)
        var labyrinth = new Maze(ExampleMaze);
        
        // Calcul manuel du chemin optimal pour :
        // D..#.
        // ##...
        // .#.#.
        // ...#.
        // ####S
        //
        // Départ : (0,0)
        // Chemin optimal :
        // D(0,0) -> .(0,1) -> .(0,2) -> .(1,2) -> .(1,3) -> .(1,4) 
        // -> .(2,4) -> .(3,4) -> S(4,4)
        // Distance = 8

        var distance = labyrinth.GetDistance();

        Assert.Equal(8, distance);
    }

    [Fact]
    public void GetDistance_WithStraightPath_ShouldReturnCorrectDistance()
    {
        // Arrange - Chemin simple en ligne droite
        var labyrinth = new Maze("""
            D....S
            """);
        
        // Calcul manuel : D -> . -> . -> . -> . -> S
        // Distance = 5

        var distance = labyrinth.GetDistance();

        Assert.Equal(5, distance);
    }

    #endregion

    #region GetShortestPath Tests

    [Fact]
    public void GetShortestPath_WithSimpleMaze_ShouldReturnCorrectPath()
    {
        var labyrinth = new Maze("""
            D..
            ...
            ..S
            """);
        
        // Calcul manuel du chemin optimal :
        // D(0,0) -> .(0,1) -> .(0,2) -> .(1,2) -> S(2,2)
        // ou
        // D(0,0) -> .(1,0) -> .(2,0) -> .(2,1) -> S(2,2)
        // Les deux sont valides avec distance 4
        
        // On s'attend à un chemin de 5 cases (départ + 3 intermédiaires + sortie)
        var expectedPathLength = 5;

        //  Résoudre d'abord le labyrinthe
        labyrinth.GetDistance();
        var path = labyrinth.GetShortestPath();

        Assert.Equal(expectedPathLength, path.Count);
        
        // Vérifier que le chemin commence au départ
        Assert.Equal((0, 0), path[0]);
        
        // Vérifier que le chemin finit à la sortie
        Assert.Equal((2, 2), path[^1]);
        
        // Vérifier que chaque case est adjacente à la suivante (distance Manhattan = 1)
        for (int i = 0; i < path.Count - 1; i++)
        {
            var current = path[i];
            var next = path[i + 1];
            var manhattanDistance = Math.Abs(next.Row - current.Row) + Math.Abs(next.Col - current.Col);
            Assert.Equal(1, manhattanDistance);
        }
    }

    [Fact]
    public void GetShortestPath_WithComplexMazeWithWalls_ShouldReturnCorrectPath()
    {
        var labyrinth = new Maze(ExampleMaze);
        
        // Calcul manuel du chemin optimal pour :
        // D..#.
        // ##...
        // .#.#.
        // ...#.
        // ####S
        //
        // Chemin optimal (distance = 8, donc 9 cases au total) :
        // D(0,0) -> .(0,1) -> .(0,2) -> .(1,2) -> .(1,3) -> .(1,4) 
        // -> .(2,4) -> .(3,4) -> S(4,4)
        
        var expectedPath = new List<(int Row, int Col)>
        {
            (0, 0), // D
            (0, 1), // .
            (0, 2), // .
            (1, 2), // .
            (1, 3), // .
            (1, 4), // .
            (2, 4), // .
            (3, 4), // .
            (4, 4)  // S
        };

        //  Résoudre d'abord le labyrinthe
        labyrinth.GetDistance();
        var path = labyrinth.GetShortestPath();

        Assert.Equal(expectedPath.Count, path.Count);
        
        // Vérifier que le chemin commence au départ
        Assert.Equal((0, 0), path[0]);
        
        // Vérifier que le chemin finit à la sortie
        Assert.Equal((4, 4), path[^1]);
        
        // Vérifier le chemin complet
        Assert.Equal(expectedPath, path);
        
        // Vérifier que chaque case est adjacente à la suivante
        for (int i = 0; i < path.Count - 1; i++)
        {
            var current = path[i];
            var next = path[i + 1];
            var manhattanDistance = Math.Abs(next.Row - current.Row) + Math.Abs(next.Col - current.Col);
            Assert.Equal(1, manhattanDistance);
        }
    }

    [Fact]
    public void GetShortestPath_WithStraightPath_ShouldReturnCorrectPath()
    {
        var labyrinth = new Maze("""
            D....S
            """);
        
        // Chemin : D(0,0) -> .(0,1) -> .(0,2) -> .(0,3) -> .(0,4) -> S(0,5)
        var expectedPath = new List<(int Row, int Col)>
        {
            (0, 0), (0, 1), (0, 2), (0, 3), (0, 4), (0, 5)
        };

        // Résoudre d'abord le labyrinthe
        labyrinth.GetDistance();
        var path = labyrinth.GetShortestPath();

        Assert.Equal(expectedPath.Count, path.Count);
        Assert.Equal(expectedPath, path);
    }

    #endregion
}
