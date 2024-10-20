using System.Runtime.InteropServices;
using System.Drawing;
using System.Runtime.Versioning;
using MinesweeperSolver.Properties;
using System.Drawing.Imaging;
using System.Linq;
using System.Globalization;
using System.Xml;
using System.Security.Cryptography.X509Certificates;

namespace MinesweeperSolver
{
    public partial class Form1 : Form
    {
        public int MineIndex;
        public int mouse_x_location;
        public int mouse_y_location;

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags);
        private const uint LeftClickDown = 0x02;
        private const uint LeftClickUp = 0x04;
        private const uint RightClickDown = 0x0008;
        private const uint RightClickUp = 0x0010;
        private const uint MiddleClickDown = 0x00000020;
        private const uint MiddleClickUp = 0x00000040;

        public Bitmap CurrentScreen()
        {

            //Takes a screenshot and returns it as a Bitmap

            //Prends une capture d'écran et la renvoie sous forme de Bitmap

            Bitmap GameInProgress = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(GameInProgress);
            g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, GameInProgress.Size, CopyPixelOperation.SourceCopy);
            return GameInProgress;
        }

        public List<int> GameBounds(Bitmap searchIn)
        {

            //Returns a list of important information about the minesweeper game in the screenshot Bitmap provided.
            //The information returned is, in order: x coordinate on the screen of the top left minesweeper square, y coordinate of that square, size of one side of a square in pixels, number of squares on the x axis, number of squares of the y axis.

            //Renvoie une liste d'informations importantes à propos du jeu de démineur dans le Bitmap de la capture d'écran donée.
            //Les informations renvoyées sont, dans cet ordre: coordonnée x sur l'écran du carré en haut à gauche du démineur, coordonnée y de ce carré, longueur du côté d'un carré en pixels, nombre de carrés sur l'axe x, nombre de carrés sur l'axe y.

            Color DebugColor;
            List<int> GridInfo = new List<int>();
            for (int y = 0; y < searchIn.Height; y++)
            {
                for (int x = 0; x < searchIn.Width; x++)
                {
                    if (searchIn.GetPixel(x, y) == Color.FromArgb(255, 0, 0) | searchIn.GetPixel(x, y) == Color.FromArgb(128, 0, 0))
                    {
                        DebugColor = searchIn.GetPixel(x, y);
                        while (searchIn.GetPixel(x, y) != Color.FromArgb(255, 255, 255))
                        {
                            y++;
                            DebugColor = searchIn.GetPixel(x, y);
                        }

                        while (searchIn.GetPixel(x, y) == Color.FromArgb(255, 255, 255))
                        {
                            y++;
                            DebugColor = searchIn.GetPixel(x, y);
                        }
                        while (searchIn.GetPixel(x, y) != Color.FromArgb(255, 255, 255))
                        {
                            y++;
                            DebugColor = searchIn.GetPixel(x, y);
                        }

                        while (searchIn.GetPixel(x, y) == Color.FromArgb(255, 255, 255))
                        {
                            y++;
                            DebugColor = searchIn.GetPixel(x, y);
                        }
                        while (searchIn.GetPixel(x, y) != Color.FromArgb(255, 255, 255))
                        {
                            y++;
                            DebugColor = searchIn.GetPixel(x, y);
                        }

                        while (searchIn.GetPixel(x, y) == Color.FromArgb(255, 255, 255))
                        {
                            x--;
                            DebugColor = searchIn.GetPixel(x, y);
                        }
                        GridInfo.Add(x + 1);
                        GridInfo.Add(y);
                        goto Found;
                    }

                }

            }
        Found:
            int DebugY;
            int DebugX;
            int GridHeight = 1;
            int GridWidth = 1;
            int SquareSize = 1;
            while (searchIn.GetPixel(GridInfo[0] + SquareSize, GridInfo[1]) == Color.FromArgb(255, 255, 255))
            {
                DebugY = GridInfo[0] + SquareSize;
                DebugX = GridInfo[1];
                DebugColor = searchIn.GetPixel(GridInfo[0] + SquareSize, GridInfo[1]);
                DebugColor = searchIn.GetPixel(GridInfo[0] + SquareSize, GridInfo[1]);
                SquareSize++;
            }
            while (searchIn.GetPixel(GridInfo[0] + SquareSize, GridInfo[1]) != Color.FromArgb(255, 255, 255))
            {
                DebugY = GridInfo[0] + SquareSize;
                DebugX = GridInfo[1];
                DebugColor = searchIn.GetPixel(GridInfo[0] + SquareSize, GridInfo[1]);
                SquareSize++;
            }
            GridInfo.Add(SquareSize);
            while (searchIn.GetPixel(GridInfo[0] + GridWidth * SquareSize, GridInfo[1]) == Color.FromArgb(255, 255, 255))
            {
                GridWidth++;
            }
            while (searchIn.GetPixel(GridInfo[0], GridInfo[1] + GridHeight * SquareSize) == Color.FromArgb(255, 255, 255))
            {
                GridHeight++;
            }
            GridInfo.Add(GridWidth - 1);
            GridInfo.Add(GridHeight - 1);
            return GridInfo;
        }

        public int[,,] AssignToArray(List<int> gameBounds, Bitmap Screenshot)
        {

            //Returns a 3D Array of the minesweeper game.
            //The first two dimensions are used simply as a representation of the minesweeper grid, containing the numbers one would expect from a minesweeper game, with 0 for a blank square, 9 for a flag, and 10 for an unknown square.
            //The third dimension contains the x and y coordinates of each square on the screen.

            //Renvoie une matrice 3D du jeu de démineur.
            //Les deux premières dimensions reprèsentent simplement la grille du démineur, contenant les numéros typiques d'un jeu de démineur, avec 0 pour une case vide, 9 pour un drapeau et 10 pour une case inconnue.
            //La troisième dimension est utilisée pour stocker les coordonnées x et y de chaque case sur l'écran.

            int[,,] MineFieldArray = new int[gameBounds[3] + 2, gameBounds[4] + 2, 3];
            for (int i = 1; i < gameBounds[3] + 1; i++)
            {
                for (int j = 1; j < gameBounds[4] + 1; j++)
                {
                    MineFieldArray[i, j, 0] = Number(gameBounds[0] + gameBounds[2] / 2 + (i - 1) * gameBounds[2], gameBounds[1] + gameBounds[2] / 2 + (j - 1) * gameBounds[2], Screenshot, gameBounds);
                    MineFieldArray[i, j, 1] = gameBounds[0] + gameBounds[2] / 2 + (i - 1) * gameBounds[2];
                    MineFieldArray[i, j, 2] = gameBounds[1] + gameBounds[2] / 2 + (j - 1) * gameBounds[2];
                }
            }
            return MineFieldArray;
        }

        public int Number(int x, int y, Bitmap GameInProgress, List<int> gameBounds)
        {

            //Returns the number found on a minesweeper square based on color information. 9 for a flag, 10 for an unknown, 0 for a blank, 1 through 6 as would be expected (7 and 8 aren't added as I never found them in tests, too rare)
            //11 is used as a debug if an unexpected color is found.

            //Renvoie le numéro trouvé sur une case du démineur en utilisant le code RGB de la case. 9 pour un drapeau, 10 pour un inconnu, 0 pour une case vide, 1 jusqu'à 6 comme dans le jeu classique (7 et 8 ne sont pas implémentés vu que je ne les ai jamais rencontré pendant mes tests, trop rares)
            //11 est utilisé si la couleur trouvée est différente qu'attendue.

            if (GameInProgress.GetPixel(x + 1, y + 3) == Color.FromArgb(0, 0, 0))
            {
                return 9;
            }
            if (GameInProgress.GetPixel(x - gameBounds[2] / 2 + 1, y - gameBounds[2]/2 + 1) == Color.FromArgb(255, 255, 255))
            {
                return 10;
            }
            if (GameInProgress.GetPixel(x, y) == Color.FromArgb(192, 192, 192))
            {
                return 0;
            }
            if (GameInProgress.GetPixel(x, y) == Color.FromArgb(0, 0, 255))
            {
                return 1;
            }
            if (GameInProgress.GetPixel(x, y).GetHue() == 120)
            {
                return 2;
            }
            if (GameInProgress.GetPixel(x, y) == Color.FromArgb(255, 0, 0))
            {
                return 3;
            }
            if (GameInProgress.GetPixel(x, y - 1).GetHue() == 240)
            {
                return 4;
            }
            if (GameInProgress.GetPixel(x, y) == Color.FromArgb(128, 0, 0))
            {
                return 5;
            }
            if (GameInProgress.GetPixel(x, y) == Color.FromArgb(0, 128, 128))
            {
                return 6;
            }
            else
                textBox2.AppendText(string.Join(",", GameInProgress.GetPixel(x, y)));
            return 11;
        }

        public List<int[]> RemoveDuplicates(List<int[]> Dupes)
        {

            //Returns a list of 1D arrays without duplicates, because the .Distinct() method doesn't seem to work on arrays so I convert them to strings and then back.

            //Renvoie un liste de matrices à une dimension car la fonction .Distinct() ne fonctionne pas sur des matrices donc je les convertis en strings d'abord et les reconvertis après.

            List<int[]> NoDupes = new List<int[]>();
            List<string> list = new List<string>();
            foreach (int[] i in Dupes)
            {
                list.Add(string.Join(",", i));
            }
            List<string> NoDupesS = list.Distinct().ToList();
            foreach (string i in NoDupesS)
            {
                string[] j = i.Split(",");
                NoDupes.Add(new int[4] { Int32.Parse(j[0]), Int32.Parse(j[1]), Int32.Parse(j[2]), Int32.Parse(j[3]) });
            }
            return NoDupes;
        }

        public List<int[]> RemoveDoubles(List<int[]> List1, List<int[]> List2)
        {

            //Same as last method, .Except() doesn't work on arrays so I convert to strings first.

            //De même que la fonction précédente, .Except() ne fonctionne pas sur des matrices donc je convertis en strings d'abord.

            List<int[]> NoDoubles = new List<int[]>();
            List<string> List1S = new List<string>();
            List<string> List2S = new List<string>();
            foreach (int[] i in List1)
            {
                List1S.Add(string.Join(",", i));
            }
            foreach (int[] i in List2)
            {
                List2S.Add(string.Join(",", i));
            }
            List<string> NoDoublesS = List1S.Except(List2S).ToList();
            foreach (string i in NoDoublesS)
            {
                string[] j = i.Split(",");
                NoDoubles.Add(new int[4] { Int32.Parse(j[0]), Int32.Parse(j[1]), Int32.Parse(j[2]), Int32.Parse(j[3]) });
            }
            return NoDoubles;
        }

        public void StartGame(List<int> gameBounds)
        {

            //Clicks in the corners of the minesweeper grid until it finds an opening.
            //The corners are the most likely places to contain an opening, for more information, check out this website: https://minesweepergame.com/strategy/first-click.php
            //If it can't find an opening in the four corners it returns an error, if it clicks on a mine the program stops naturally.

            //Clique les coins du jeu de démineur afin de trouver une ouverture pour commencer.
            //Les coins sont les endroits les plus probables de contenir un ouverture, pour plus d'infos, vous pouvez jeter un oeil à ce site: https://minesweepergame.com/strategy/first-click.php
            //Si le jeu ne trouve pas d'ouverture au quatre coins une erreur est renvoyée (celle-ci se traduit par: N'as pas pu démarrer, Aucune ouverture trouvée), si le programme clique sur une mine le programme s'arrête naturellement.

            List<int[]> Corners = new List<int[]>();
            Corners.Add(new int[] { gameBounds[0] + gameBounds[2] / 2, gameBounds[1] + gameBounds[2] / 2 });
            Corners.Add(new int[] { gameBounds[0] - gameBounds[2] / 2 + gameBounds[3] * gameBounds[2], gameBounds[1] + gameBounds[2] / 2 });
            Corners.Add(new int[] { gameBounds[0] + gameBounds[2] / 2, gameBounds[1] - gameBounds[2] / 2 + gameBounds[4] * gameBounds[2] });
            Corners.Add(new int[] { gameBounds[0] - gameBounds[2] / 2 + gameBounds[3] * gameBounds[2], gameBounds[1] - gameBounds[2] / 2 + gameBounds[4] * gameBounds[2] });
            foreach (int[] i in Corners)
            {
                Cursor.Position = new Point(i[0], i[1]);
                mouse_event(LeftClickDown | LeftClickUp);
                Bitmap StartScreenshot = new Bitmap(CurrentScreen());
                if (StartScreenshot.GetPixel(i[0], i[1]) == Color.FromArgb(192, 192, 192))
                {
                    goto Start;
                }
            }
            throw new ArgumentException("Couldn't Start, No Opening Found");
            Start:;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

            //Starts solving the minesweeper once the checkbox is clicked.
            //!!! If the number of mines isn't provided the program will return an error!

            //Commence à résoudre le démineur après que l'utilisateur a cliqué sur la boite.
            //!!! Si le nombre de mines n'est pas donné le program va renvoyer une erreur!

            textBox1.Text = "";
            textBox2.Text = "";
            List<int[]> MinesList = new List<int[]>();
            List<int[]> RevealSpots = new List<int[]>();
            int ProgressTracker = 0;
            int NoProgressCounter = 0;
            Cursor.Position = new Point(0, 0);
            Bitmap Screenshot = CurrentScreen();
            var gameBounds = new List<int>(GameBounds(Screenshot));
            StartGame(gameBounds);
            do
            {
                Cursor.Position = new Point(0, 0);
                Thread.Sleep(10);
                Screenshot = CurrentScreen();
                int[,,] MineFieldArray = AssignToArray(gameBounds, Screenshot);
                List<int[]> NewMines = MineFinder(MineFieldArray, MinesList);
                FlagMines(NewMines, MineFieldArray);                
                RevealSpots = RevealFinder(MineFieldArray, MinesList);
                while (RevealSpots.Count > 0)
                {                    
                    RevealSpaces(RevealSpots, MineFieldArray);                   
                    Cursor.Position = new Point(0, 0);
                    Thread.Sleep(10);
                    Screenshot = CurrentScreen();
                    MineFieldArray = AssignToArray(gameBounds, Screenshot);
                    RevealSpots = RevealFinder(MineFieldArray, MinesList);
                }
                if (MinesList.Count > ProgressTracker)
                {
                    ProgressTracker = MinesList.Count;
                    NoProgressCounter = 0;
                }
                else
                    NoProgressCounter++;
                if (NoProgressCounter == 1)
                {
                    int[,,] ReducedArray = CurrentArrayReduced(MineFieldArray, MinesList);
                    OneBasedPatterns(ReducedArray, MinesList);
                }
                if (NoProgressCounter == 2)
                {
                    int[,,] ReducedArray = CurrentArrayReduced(MineFieldArray, MinesList);
                    HoleBasedPatterns(ReducedArray, MinesList);
                    textBox2.AppendText("Used Pattern 1, ");
                }
                if (NoProgressCounter == 4)
                {
                    textBox4.Text = "Couldn't Solve";
                    goto End;
                }
            }
            while (MinesList.Count < Int32.Parse(textBox3.Text));
            textBox4.Text = "Solved";
        End:;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
        public Form1()
        {
            InitializeComponent();
        }

        public List<int[]> CheckSurroundingBlocks(int[,,] CurrentArray, int[] Coordinate, int[] CheckFor)
        {

            //Method that returns a list of 1D arrays containing the coordinates of all of the squares, surrounding the square defined by the array coordinates (!! Not screen coordinates !!) provided in the array Coordinate, whose number is in between the two numbers provided in the array CheckFor.
            //Each array of this list contains, in order: x array coordinate of the square, y array coordinate of the square, x screen coordinate of the square, y screen coordinate of the square.

            //Fonction qui renvoie une liste de matrices à une dimension qui contient les coordonnées de toutes les cases, qui entourent la case définie par les coordonnées de matrice (!! Pas les coordonnées d'écran !!) données par la matrice Coordinate, dont le numéro est entre les deux numéros données par la matrice CheckFor.
            //Chaque matrice de cette liste contiens, dans cet ordre: coordonnée de matrice x de la case, coordonnée de matrice y de la case, coordonnée de l'écran x de la case, coordonnée de l'écran y de la case.

            List<int[]> FoundCoords = new List<int[]>();
            if (CurrentArray[Coordinate[0] - 1, Coordinate[1] - 1, 0] <= CheckFor[1] && CurrentArray[Coordinate[0] - 1, Coordinate[1] - 1, 0] >= CheckFor[0])
            {
                FoundCoords.Add(new int[4] { Coordinate[0] - 1, Coordinate[1] - 1, CurrentArray[Coordinate[0] - 1, Coordinate[1] - 1, 1], CurrentArray[Coordinate[0] - 1, Coordinate[1] - 1, 2] });
            }
            if (CurrentArray[Coordinate[0], Coordinate[1] - 1, 0] <= CheckFor[1] && CurrentArray[Coordinate[0], Coordinate[1] - 1, 0] >= CheckFor[0])
            {
                FoundCoords.Add(new int[4] { Coordinate[0], Coordinate[1] - 1, CurrentArray[Coordinate[0], Coordinate[1] - 1, 1], CurrentArray[Coordinate[0], Coordinate[1] - 1, 2] });
            }
            if (CurrentArray[Coordinate[0] + 1, Coordinate[1] - 1, 0] <= CheckFor[1] && CurrentArray[Coordinate[0] + 1, Coordinate[1] - 1, 0] >= CheckFor[0])
            {
                FoundCoords.Add(new int[4] { Coordinate[0] + 1, Coordinate[1] - 1, CurrentArray[Coordinate[0] + 1, Coordinate[1] - 1, 1], CurrentArray[Coordinate[0] + 1, Coordinate[1] - 1, 2] });
            }
            if (CurrentArray[Coordinate[0] - 1, Coordinate[1], 0] <= CheckFor[1] && CurrentArray[Coordinate[0] - 1, Coordinate[1], 0] >= CheckFor[0])
            {
                FoundCoords.Add(new int[4] { Coordinate[0] - 1, Coordinate[1], CurrentArray[Coordinate[0] - 1, Coordinate[1], 1], CurrentArray[Coordinate[0] - 1, Coordinate[1], 2] });
            }
            if (CurrentArray[Coordinate[0] + 1, Coordinate[1], 0] <= CheckFor[1] && CurrentArray[Coordinate[0] + 1, Coordinate[1], 0] >= CheckFor[0])
            {
                FoundCoords.Add(new int[4] { Coordinate[0] + 1, Coordinate[1], CurrentArray[Coordinate[0] + 1, Coordinate[1], 1], CurrentArray[Coordinate[0] + 1, Coordinate[1], 2] });
            }
            if (CurrentArray[Coordinate[0] - 1, Coordinate[1] + 1, 0] <= CheckFor[1] && CurrentArray[Coordinate[0] - 1, Coordinate[1] + 1, 0] >= CheckFor[0])
            {
                FoundCoords.Add(new int[4] { Coordinate[0] - 1, Coordinate[1] + 1, CurrentArray[Coordinate[0] - 1, Coordinate[1] + 1, 1], CurrentArray[Coordinate[0] - 1, Coordinate[1] + 1, 2] });
            }
            if (CurrentArray[Coordinate[0], Coordinate[1] + 1, 0] <= CheckFor[1] && CurrentArray[Coordinate[0], Coordinate[1] + 1, 0] >= CheckFor[0])
            {
                FoundCoords.Add(new int[4] { Coordinate[0], Coordinate[1] + 1, CurrentArray[Coordinate[0], Coordinate[1] + 1, 1], CurrentArray[Coordinate[0], Coordinate[1] + 1, 2] });
            }
            if (CurrentArray[Coordinate[0] + 1, Coordinate[1] + 1, 0] <= CheckFor[1] && CurrentArray[Coordinate[0] + 1, Coordinate[1] + 1, 0] >= CheckFor[0])
            {
                FoundCoords.Add(new int[4] { Coordinate[0] + 1, Coordinate[1] + 1, CurrentArray[Coordinate[0] + 1, Coordinate[1] + 1, 1], CurrentArray[Coordinate[0] + 1, Coordinate[1] + 1, 2] });
            }
            return FoundCoords;
        }

        public List<int[]> MineFinder(int[,,] CurrentArray, List<int[]> MinesList)
        {

            //Method that returns a list of arrays containing the coordinates of every new mine that has been found, arrays have the same format as explained in the method CheckSurroundingBlocks().

            //Fonction qui renvoie une liste de matrices contenant les coordonnées de chaque nouvelle mine qui a été trouvée, ces matrices ont le même format que celle expliquée dans la fonction CheckSurroundingBlocks().

            List<int[]> MineCoords = new List<int[]>();
            List<int[]> CoordsToCheck = new List<int[]>();
            for (int j = 1; j < CurrentArray.GetLength(1) - 1; j++)
            {
                for (int i = 1; i < CurrentArray.GetLength(0) - 1; i++)
                {
                    CoordsToCheck.Add(new int[] { i, j });
                }
            }
            foreach (int[] i in CoordsToCheck)
            {
                List<int[]> SurroundingMines = CheckSurroundingBlocks(CurrentArray, i, new int[2] { 9, 10 });
                if (SurroundingMines.Count == CurrentArray[i[0], i[1], 0])
                    foreach (int[] j in SurroundingMines)
                    {
                        MineCoords.Add(j);
                    }
            }
            List<int[]> NewMines = RemoveDoubles(RemoveDuplicates(MineCoords), MinesList);
            MinesList.AddRange(NewMines);
            return NewMines;
        }

        public List<int[]> RevealFinder(int[,,] CurrentArray, List<int[]> MinesList)
        {

            //Method that returns a list of arrays containing the coordinates of every square whose number is satisfied, and as such, should be clicked to reveal all surrounding unknowns.

            //Fonction qui renvoie une liste de matrices qui contient les coordonnées de chaque case pour laquelle le numéro indiqué est satisfait, et donc, qui doit être cliqué pour dévoiler toutes les cases inconnues aux alentours.

            List<int[]> RevealSpots = new List<int[]>();
            List<int[]> NumberSpots = new List<int[]>();
            foreach (int[] i in MinesList)
                NumberSpots.AddRange(CheckSurroundingBlocks(CurrentArray, i, new int[] { 1, 8 }));
            foreach (int[] i in NumberSpots)
            {
                List<int[]> ProxMines = CheckSurroundingBlocks(CurrentArray, i, new int[] { 9, 9 });
                List<int[]> ProxUnknowns = CheckSurroundingBlocks(CurrentArray, i, new int[] { 10, 10 });
                if (ProxMines.Count == CurrentArray[i[0], i[1], 0] && ProxUnknowns.Count > 0)
                    RevealSpots.Add(i);
            }
            return RevealSpots;
        }

        public void FlagMines(List<int[]> MineCoords, int[,,] CurrentArray)
        {

            //Places a flag on every square in the list MineCoords

            //Place un drapeau sur chaque case dans la liste MineCoords

            foreach (int[] i in MineCoords)
            {
                CurrentArray[i[0], i[1], 0] = 9;
                Cursor.Position = new Point(i[2], i[3]);
                mouse_event(RightClickDown | RightClickUp);
                Thread.Sleep(10);
            }
        }

        public void RevealSpaces(List<int[]> RevealCoords, int[,,] CurrentArray)
        {

            //Reveals all unknown squares surrounding each square from the list RevealCoords

            //Dévoile toutes les cases inconnue qui entourent les cases de la liste RevealCoords

            foreach (int[] i in RevealCoords)
            {
                Cursor.Position = new Point(i[2], i[3]);
                mouse_event(MiddleClickDown | MiddleClickUp);
            }
        }

        private void textBox3_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void label4_Click(object sender, EventArgs e)
        {

        }


        //Pattern Implementation Begins Here

        //All methods in this next section are for more complex minesweeper positions that require use of logic rather than simply looking to see if the number one is next to 1 square,
        //the number 2 next to two squares, etc... For more information on patterns and the logic behind them, check out this website: https://minesweeper.online/help/patterns

        //Toutes les fonctions dans la section qui suit sont pour des positions de démineur plus complexes qui requièrent l'utilisation de logique plutôt que de simplement regarder si
        //le numéro 1 est à côté d'une case, le numéro 2 à côté de deux cases, etc... Pour plus d'informations sur les schémas récurrents et la logique derrière, vous pouvez jeter un oeil à ce site: https://minesweeper.online/help/patterns (le site est disponible en français)


        private int[,,] CurrentArrayReduced(int[,,] CurrentArray, List<int[]> MinesList)
        {

            //This method performs "Reduction" on the array we use as the minesweeper grid. The resulting array, would be equivalent to the array obtained if every flag and number
            //whose surrounding flag count is satisfied is set to 0, the same as a blank square. And every number with a partially filled surrounding flag count is reduced by the number
            //of surrounding flags. The section on reduction on the website I previously cited might help in understanding the idea.

            //Cette fonction réalise une "Réduction" sur la matrice que nous utilisons en lieu de la grille du démineur. La matrice résultante sera équivalente à la matrice obtenue si chaque
            //drapeau et numéro pour laquelle le nombre de drapeaux qui l'entoure est égal au numéro en question, deviennent des zéros, comme s'ils étaient des cases vides. Chaque numéro pour lequel
            //le compte de drapeaux aux alentours est partiellement remplie, est réduit par ce compte. La section sur les réductions sur le site cité auparavant peut aider à comprendre ce principe.

            int[,,] ReducedArray = CurrentArray;
            foreach (int[] i in MinesList)
            {
                ReducedArray[i[0], i[1], 0] = 9;
            }
            for (int j = 1; j < ReducedArray.GetLength(1) - 1; j++)
            {
                for (int i = 1; i < ReducedArray.GetLength(0) - 1; i++)
                {
                    if (ReducedArray[i, j, 0] >= 1 && ReducedArray[i, j, 0] <= 8)
                    {
                        ReducedArray[i, j, 0] -= CheckSurroundingBlocks(ReducedArray, new int[2] { i, j }, new int[2] { 9, 9 }).Count;
                    }
                }
            }
            foreach (int[] i in MinesList)
            {
                ReducedArray[i[0], i[1], 0] = 0;
            }
            return ReducedArray;
        }

        public List<int[]> CheckSpecificSurroundingBlocks(int[,,] CurrentArray, int[] Coordinate, int[] CheckFor, int[,] WhichOnes)
        {

            //This method works like CheckSurroundingBlocks() but needs a 3 by 3 binary array to know which of the surrounding blocks to check.

            //Cette fonction fonctionne de la même manière que CheckSurroundingBlocks() mais a besoin d'un matrice binaire de dimensions 3 fois 3 pour savoir lesquels des cases aux alentours vérifier.

            List<int[]> FoundCoords = new List<int[]>();
            if (CurrentArray[Coordinate[0] - 1, Coordinate[1] - 1, 0] <= CheckFor[1] && CurrentArray[Coordinate[0] - 1, Coordinate[1] - 1, 0] >= CheckFor[0] && WhichOnes[0, 0] == 1)
            {
                FoundCoords.Add(new int[4] { Coordinate[0] - 1, Coordinate[1] - 1, CurrentArray[Coordinate[0] - 1, Coordinate[1] - 1, 1], CurrentArray[Coordinate[0] - 1, Coordinate[1] - 1, 2] });
            }
            if (CurrentArray[Coordinate[0], Coordinate[1] - 1, 0] <= CheckFor[1] && CurrentArray[Coordinate[0], Coordinate[1] - 1, 0] >= CheckFor[0] && WhichOnes[0, 1] == 1)
            {
                FoundCoords.Add(new int[4] { Coordinate[0], Coordinate[1] - 1, CurrentArray[Coordinate[0], Coordinate[1] - 1, 1], CurrentArray[Coordinate[0], Coordinate[1] - 1, 2] });
            }
            if (CurrentArray[Coordinate[0] + 1, Coordinate[1] - 1, 0] <= CheckFor[1] && CurrentArray[Coordinate[0] + 1, Coordinate[1] - 1, 0] >= CheckFor[0] && WhichOnes[0, 2] == 1)
            {
                FoundCoords.Add(new int[4] { Coordinate[0] + 1, Coordinate[1] - 1, CurrentArray[Coordinate[0] + 1, Coordinate[1] - 1, 1], CurrentArray[Coordinate[0] + 1, Coordinate[1] - 1, 2] });
            }
            if (CurrentArray[Coordinate[0] - 1, Coordinate[1], 0] <= CheckFor[1] && CurrentArray[Coordinate[0] - 1, Coordinate[1], 0] >= CheckFor[0] && WhichOnes[1, 0] == 1)
            {
                FoundCoords.Add(new int[4] { Coordinate[0] - 1, Coordinate[1], CurrentArray[Coordinate[0] - 1, Coordinate[1], 1], CurrentArray[Coordinate[0] - 1, Coordinate[1], 2] });
            }
            if (CurrentArray[Coordinate[0] + 1, Coordinate[1], 0] <= CheckFor[1] && CurrentArray[Coordinate[0] + 1, Coordinate[1], 0] >= CheckFor[0] && WhichOnes[1, 2] == 1)
            {
                FoundCoords.Add(new int[4] { Coordinate[0] + 1, Coordinate[1], CurrentArray[Coordinate[0] + 1, Coordinate[1], 1], CurrentArray[Coordinate[0] + 1, Coordinate[1], 2] });
            }
            if (CurrentArray[Coordinate[0] - 1, Coordinate[1] + 1, 0] <= CheckFor[1] && CurrentArray[Coordinate[0] - 1, Coordinate[1] + 1, 0] >= CheckFor[0] && WhichOnes[2, 0] == 1)
            {
                FoundCoords.Add(new int[4] { Coordinate[0] - 1, Coordinate[1] + 1, CurrentArray[Coordinate[0] - 1, Coordinate[1] + 1, 1], CurrentArray[Coordinate[0] - 1, Coordinate[1] + 1, 2] });
            }
            if (CurrentArray[Coordinate[0], Coordinate[1] + 1, 0] <= CheckFor[1] && CurrentArray[Coordinate[0], Coordinate[1] + 1, 0] >= CheckFor[0] && WhichOnes[2, 1] == 1)
            {
                FoundCoords.Add(new int[4] { Coordinate[0], Coordinate[1] + 1, CurrentArray[Coordinate[0], Coordinate[1] + 1, 1], CurrentArray[Coordinate[0], Coordinate[1] + 1, 2] });
            }
            if (CurrentArray[Coordinate[0] + 1, Coordinate[1] + 1, 0] <= CheckFor[1] && CurrentArray[Coordinate[0] + 1, Coordinate[1] + 1, 0] >= CheckFor[0] && WhichOnes[2, 2] == 1)
            {
                FoundCoords.Add(new int[4] { Coordinate[0] + 1, Coordinate[1] + 1, CurrentArray[Coordinate[0] + 1, Coordinate[1] + 1, 1], CurrentArray[Coordinate[0] + 1, Coordinate[1] + 1, 2] });
            }
            return FoundCoords;
        }

        public List<int[]> NumberCoords(int Number, int[,,] ReducedArray)
        {

            //Returns the list of coordinates of squares with the number that is being looked for, coordinates in the same format as before.

            //Renvoie une liste des coordonnées des cases contenant le numéro recherché, coordonnées dans le même format que précédemment.

            List<int[]> numberCoords = new List<int[]>();
            for (int j = 1; j < ReducedArray.GetLength(1) - 1; j++)
            {
                for (int i = 1; i < ReducedArray.GetLength(0) - 1; i++)
                {
                    if (ReducedArray[i, j, 0] == Number)
                    {
                        numberCoords.Add(new int[] { i, j, ReducedArray[i, j, 1], ReducedArray[i, j, 2] });
                    }
                }
            }
            return numberCoords;
        }

        public void OneBasedPatterns(int[,,] ReducedArray, List<int[]> MinesList)
        {

            //This method executes all of the basic patterns found in the website previously provided.

            //Cette fonction execute tous les schémas de base du site cité auparavant.

            List<int[]> OneCoords = NumberCoords(1, ReducedArray);
            List<int[]> LClickCoords = new List<int[]>();
            List<int[]> RClickCoords = new List<int[]>();
            foreach (int[] i in OneCoords)
            {
                /// One-One Implementation
                if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] {0,8}, new int[3, 3] { { 1, 1, 1 }, { 1, 0, 0 }, { 1, 0, 0 } }).Count == 5)
                {
                    if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 1, 1 }, new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 1, 0 } }).Count == 1)
                    {
                        LClickCoords.AddRange(CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0], i[1] + 1 }, new int[] { 10, 10 }, new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 1, 1, 1 } }));
                    }
                    if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 1, 1 }, new int[3, 3] { { 0, 0, 0 }, { 0, 0, 1 }, { 0, 0, 0 } }).Count == 1)
                    {
                        LClickCoords.AddRange(CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0] + 1, i[1] }, new int[] { 10, 10 }, new int[3, 3] { { 0, 0, 1 }, { 0, 0, 1 }, { 0, 0, 1 } }));
                    }
                }
                if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 0, 8 }, new int[3, 3] { { 1, 1, 1 }, { 0, 0, 1 }, { 0, 0, 1 } }).Count == 5)
                {
                    if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 1, 1 }, new int[3, 3] { { 0, 0, 0 }, { 1, 0, 0 }, { 0, 0, 0 } }).Count == 1)
                    {
                        LClickCoords.AddRange(CheckSpecificSurroundingBlocks(ReducedArray, new int[] {i[0] - 1, i[1] }, new int[] { 10, 10 }, new int[3, 3] { { 1, 0, 0 }, { 1, 0, 0 }, { 1, 0, 0 } }));
                    }
                    if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 1, 1 }, new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 1, 0 } }).Count == 1)
                    {
                        LClickCoords.AddRange(CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0], i[1] + 1 }, new int[] { 10, 10 }, new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 1, 1, 1 } }));
                    }
                }
                if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 0, 8 }, new int[3, 3] { { 1, 0, 0 }, { 1, 0, 0 }, { 1, 1, 1 } }).Count == 5)
                {
                    if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 1, 1 }, new int[3, 3] { { 0, 1, 0 }, { 0, 0, 0 }, { 0, 0, 0 } }).Count == 1)
                    {
                        LClickCoords.AddRange(CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0], i[1] - 1 }, new int[] { 10, 10 }, new int[3, 3] { { 1, 1, 1 }, { 0, 0, 0 }, { 0, 0, 0 } }));
                    }
                    if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 1, 1 }, new int[3, 3] { { 0, 0, 0 }, { 0, 0, 1 }, { 0, 0, 0 } }).Count == 1)
                    {
                        LClickCoords.AddRange(CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0] + 1, i[1] }, new int[] { 10, 10 }, new int[3, 3] { { 0, 0, 1 }, { 0, 0, 1 }, { 0, 0, 1 } }));
                    }
                }
                if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 0, 8 }, new int[3, 3] { { 0, 0, 1 }, { 0, 0, 1 }, { 1, 1, 1 } }).Count == 5)
                {
                    if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 1, 1 }, new int[3, 3] { { 0, 0, 0 }, { 1, 0, 0 }, { 0, 0, 0 } }).Count == 1)
                    {
                        LClickCoords.AddRange(CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0] - 1, i[1] }, new int[] { 10, 10 }, new int[3, 3] { { 1, 0, 0 }, { 1, 0, 0 }, { 1, 0, 0 } }));
                    }
                    if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 1, 1 }, new int[3, 3] { { 0, 1, 0 }, { 0, 0, 0 }, { 0, 0, 0 } }).Count == 1)
                    {
                        LClickCoords.AddRange(CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0], i[1] - 1 }, new int[] { 10, 10 }, new int[3, 3] { { 1, 1, 1 }, { 0, 0, 0 }, { 0, 0, 0 } }));
                    }
                }
                /// One-Two Implementation
                if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 0, 8 }, new int[3, 3] { { 1, 1, 1 }, { 1, 0, 0 }, { 0, 0, 0 } }).Count == 4 && ReducedArray[i[0] + 1, i[1], 0] > 1 && ReducedArray[i[0] + 1, i[1], 0] < 5)
                {
                    List<int[]> ClickPoints = CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0] + 1, i[1] }, new int[] { 10, 10 }, new int[3, 3] { { 0, 0, 1 }, { 0, 0, 1 }, { 0, 0, 1 } });
                    if (ClickPoints.Count == ReducedArray[i[0] + 1, i[1], 0] - 1)
                        RClickCoords.AddRange(ClickPoints);
                }
                if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 0, 8 }, new int[3, 3] { { 1, 1, 1 }, { 0, 0, 1 }, { 0, 0, 0 } }).Count == 4 && ReducedArray[i[0] - 1, i[1], 0] > 1 && ReducedArray[i[0] - 1, i[1], 0] < 5)
                {
                    List<int[]> ClickPoints = CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0] - 1, i[1] }, new int[] { 10, 10 }, new int[3, 3] { { 1, 0, 0 }, { 1, 0, 0 }, { 1, 0, 0 } });
                    if (ClickPoints.Count == ReducedArray[i[0] - 1, i[1], 0] - 1)
                        RClickCoords.AddRange(ClickPoints);
                }
                if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 0, 8 }, new int[3, 3] { { 1, 1, 0 }, { 1, 0, 0 }, { 1, 0, 0 } }).Count == 4 && ReducedArray[i[0], i[1] + 1, 0] > 1 && ReducedArray[i[0], i[1] + 1, 0] < 5)
                {
                    List<int[]> ClickPoints = CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0], i[1] + 1}, new int[] { 10, 10 }, new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 1, 1, 1 } });
                    if (ClickPoints.Count == ReducedArray[i[0], i[1] + 1, 0] - 1)
                        RClickCoords.AddRange(ClickPoints);
                }
                if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 0, 8 }, new int[3, 3] { { 1, 0, 0 }, { 1, 0, 0 }, { 1, 1, 0 } }).Count == 4 && ReducedArray[i[0], i[1] - 1, 0] > 1 && ReducedArray[i[0], i[1] - 1, 0] < 5)
                {
                    List<int[]> ClickPoints = CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0], i[1] - 1 }, new int[] { 10, 10 }, new int[3, 3] { { 1, 1, 1 }, { 0, 0, 0 }, { 0, 0, 0 } });
                    if (ClickPoints.Count == ReducedArray[i[0], i[1] - 1, 0] - 1)
                        RClickCoords.AddRange(ClickPoints);
                }
                if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 0, 8 }, new int[3, 3] { { 0, 1, 1 }, { 0, 0, 1 }, { 0, 0, 1 } }).Count == 4 && ReducedArray[i[0], i[1] + 1, 0] > 1 && ReducedArray[i[0], i[1] + 1, 0] < 5)
                {
                    List<int[]> ClickPoints = CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0], i[1] + 1 }, new int[] { 10, 10 }, new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 1, 1, 1 } });
                    if (ClickPoints.Count == ReducedArray[i[0], i[1] + 1, 0] - 1)
                        RClickCoords.AddRange(ClickPoints);
                }
                if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 0, 8 }, new int[3, 3] { { 0, 0, 1 }, { 0, 0, 1 }, { 0, 1, 1 } }).Count == 4 && ReducedArray[i[0], i[1] - 1, 0] > 1 && ReducedArray[i[0], i[1] - 1, 0] < 5)
                {
                    List<int[]> ClickPoints = CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0], i[1] - 1 }, new int[] { 10, 10 }, new int[3, 3] { { 1, 1, 1 }, { 0, 0, 0 }, { 0, 0, 0 } });
                    if (ClickPoints.Count == ReducedArray[i[0], i[1] - 1, 0] - 1)
                        RClickCoords.AddRange(ClickPoints);
                }
                if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 0, 8 }, new int[3, 3] { { 0, 0, 0 }, { 1, 0, 0 }, { 1, 1, 1 } }).Count == 4 && ReducedArray[i[0] + 1, i[1], 0] > 1 && ReducedArray[i[0] + 1, i[1], 0] < 5)
                {
                    List<int[]> ClickPoints = CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0] + 1, i[1] }, new int[] { 10, 10 }, new int[3, 3] { { 0, 0, 1 }, { 0, 0, 1 }, { 0, 0, 1 } });
                    if (ClickPoints.Count == ReducedArray[i[0] + 1, i[1], 0] - 1)
                        RClickCoords.AddRange(ClickPoints);
                }
                if (CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 0, 8 }, new int[3, 3] { { 0, 0, 0 }, { 0, 0, 1 }, { 1, 1, 1 } }).Count == 4 && ReducedArray[i[0] - 1, i[1], 0] > 1 && ReducedArray[i[0] - 1, i[1], 0] < 5)
                {
                    List<int[]> ClickPoints = CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0] - 1, i[1] }, new int[] { 10, 10 }, new int[3, 3] { { 1, 0, 0 }, { 1, 0, 0 }, { 1, 0, 0 } });
                    if (ClickPoints.Count == ReducedArray[i[0] - 1, i[1], 0] - 1)
                        RClickCoords.AddRange(ClickPoints);
                }
                /// End of Simple Pattern Implementation
            }
            LClickCoords = RemoveDuplicates(LClickCoords);
            RClickCoords = RemoveDuplicates(RClickCoords);
            foreach (int[] j in RClickCoords)
            {
                MinesList.Add(j);
                Cursor.Position = new Point(j[2], j[3]);
                mouse_event(RightClickDown | RightClickUp);
            }
            foreach (int[] j in LClickCoords)
            {
                Cursor.Position = new Point(j[2], j[3]);
                mouse_event(LeftClickDown | LeftClickUp);
            }         
        }

        public void HoleBasedPatterns(int[,,] ReducedArray, List<int[]> MinesList)
        {

            //This method executes H1 and H2 of the Holes patterns in the website from before

            //Cette fonction execute les schémas H1 et H2 de la section Trous du site internet cité auparavant.

            List<int[]> OneCoords = NumberCoords(1, ReducedArray);
            List<int[]> LClickCoords = new List<int[]>();
            foreach (int[] i in OneCoords)
            {
                if (ReducedArray[i[0], i[1] + 1, 0] == 1 && CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0], i[1] + 1 }, new int[] { 0, 8 }, new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 1, 1, 1 } }).Count == 3)
                {
                    List<int[]> ClickPoints = CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 10, 10 }, new int[3, 3] { { 1, 1, 1 }, { 0, 0, 0 }, { 0, 0, 0 } });
                    if (ClickPoints.Count > 0)
                        LClickCoords.AddRange(ClickPoints);
                }
                if (ReducedArray[i[0], i[1] - 1, 0] == 1 && CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0], i[1] - 1 }, new int[] { 0, 8 }, new int[3, 3] { { 1, 1, 1 }, { 0, 0, 0 }, { 0, 0, 0 } }).Count == 3)
                {
                    List<int[]> ClickPoints = CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 10, 10 }, new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 1, 1, 1 } });
                    if (ClickPoints.Count > 0)
                        LClickCoords.AddRange(ClickPoints);
                }
                if (ReducedArray[i[0] + 1, i[1], 0] == 1 && CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0] + 1, i[1] }, new int[] { 0, 8 }, new int[3, 3] { { 0, 0, 1 }, { 0, 0, 1 }, { 0, 0, 1 } }).Count == 3)
                {
                    List<int[]> ClickPoints = CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 10, 10 }, new int[3, 3] { { 1, 0, 0 }, { 1, 0, 0 }, { 1, 0, 0 } });
                    if (ClickPoints.Count > 0)
                        LClickCoords.AddRange(ClickPoints);
                }
                if (ReducedArray[i[0] - 1, i[1], 0] == 1 && CheckSpecificSurroundingBlocks(ReducedArray, new int[] { i[0] - 1, i[1] }, new int[] { 0, 8 }, new int[3, 3] { { 1, 0, 0 }, { 1, 0, 0 }, { 1, 0, 0 } }).Count == 3)
                {
                    List<int[]> ClickPoints = CheckSpecificSurroundingBlocks(ReducedArray, i, new int[] { 10, 10 }, new int[3, 3] { { 0, 0, 1 }, { 0, 0, 1 }, { 0, 0, 1 } });
                    if (ClickPoints.Count > 0)
                        LClickCoords.AddRange(ClickPoints);
                }
            }
            foreach (int[] j in LClickCoords)
            {
                Cursor.Position = new Point(j[2], j[3]);
                mouse_event(LeftClickDown | LeftClickUp);
            }             
        }

        //More patterns from the website could be implemented using the same ideas but most of them are too uncommon for it to add actual performance. I made this code as C# practice not to create an
        //infallible minesweeper solver and this is already very good at solving minesweepers.

        //Plus de schémas du site pourraient être implémentés en utilisant les mêmes méthodes mais la plupart sont trop rares pour êtres utiles. J'ai créé ce code pour devenir meilleur au langage C#, pas
        //pour créer un solveur de démineur infaillible et ce code est déjà très performant à cette tâche.
    }
}
