using System;
using System.Dynamic;

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

using Ham.TicTacToe;
using Ham.GameControl;

namespace Ham.Board
{

    public class BoardController : Controller
    {
        public GameObject CellPrefab; // Prefab for the cells

        public Cell[][] Cells; // 2D array to hold the cells

        public UnityEvent OnReset;


        public Vector3 DefaultCellLocationOffset = Vector3.zero;
        public Vector3 DefaultCellRotation = Vector3.zero;

        public event EventHandler<CellOwnerChangedEventArgs> OnCellOwnerChanged;
        public event EventHandler<CellSelectedEventArgs> OnCellSelected;

        [Header("Board options")]
        public int Width = 3;
        public int Height = 3;

        protected override void Awake()
        {
            base.Awake();
            this.Reset();
        }

        protected override void Start()
        {
            base.Awake();
        }

        public void Reset()
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                GameObject child = this.transform.GetChild(i).gameObject;
                if (child != null && child.name == "Cells")
                {
                    Destroy(child.gameObject);
                }
            }
            
            if (this.Cells != null && this.Cells.Length > 0)
            {
                this.Cells = null;
            }
            this.Setup();
            this.OnReset?.Invoke();

        }

        [ContextMenu("Reset")]
        public void ResetImmediate()
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                GameObject child = this.transform.GetChild(i).gameObject;
                if (child != null && child.name == "Cells")
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            Debug.Log(this.Cells);
            if (this.Cells != null && this.Cells.Length > 0)
            {
                this.Cells = null;
            }
            this.Setup();
            this.OnReset?.Invoke();

        }

        private void Setup()
        {
            Debug.Log("BoardController: Setup()");
            this.Cells = new Cell[Width][];
            for (int i = 0; i < Width; i++)
            {
                this.Cells[i] = new Cell[Height];
            }
            // Create the board
            GameObject cells = new GameObject("Cells");
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // Instantiate a new cell
                    GameObject bgTile = Instantiate(this.CellPrefab, new Vector3(x, y, 0), Quaternion.identity);
                    Cell currentCell = bgTile.GetComponent<Cell>();
                    bgTile.transform.SetParent(cells.transform);
                    cells.transform.SetParent(this.transform);
                    bgTile.name = $"Cell {x} {y}";
                    bgTile.transform.Translate(this.DefaultCellLocationOffset);
                    bgTile.transform.Rotate(this.DefaultCellRotation);

                    /*currentCell.OnOwnerChanged += (object sender, CellOwnerChangedEventArgs e) =>
                    {
                        Debug.Log("Board responding to Cell.OnOwnerChanged");
                        this.OnCellOwnerChanged?.Invoke(this, e);
                    };*/

                    currentCell.OnSelected += (object sender, CellSelectedEventArgs e) =>
                    {
                        Debug.Log("Board responding to Cell.OnSelected");
                        this.OnCellSelected?.Invoke(this, e);
                    };

                    this.Cells[x][y] = currentCell;
                }
            }

            // Second loop to link the Cells.
            for (int x = 0; x < this.Cells.Length; x++)
            {
                for (int y = 0; y < this.Cells[x].Length; y++)
                {
                    Cell cell = this.Cells[x][y];
                    cell.neighbours["north"] = (y == this.Cells[x].Length - 1) ? null : this.Cells[x][y + 1];
                    cell.neighbours["south"] = (y == 0) ? null : this.Cells[x][y - 1];
                    cell.neighbours["east"] = (x == this.Cells.Length - 1) ? null : this.Cells[x + 1][y];
                    cell.neighbours["west"] = (x == 0) ? null : this.Cells[x - 1][y];
                }
            }

        }

        public void Explode()
        {
            Cell middlecell = this.Cells[(int)Math.Floor((double)this.Cells.Length / 2)][0];
            Rigidbody rb = middlecell.GetComponent<Rigidbody>();
            rb.AddForce(new Vector3(0, 50, 0));
        }

        public Cell GetCell(int x, int y)
        {
            if (this.Cells == null) { return null; }
            if (this.Cells.Length < 1) { return null; }
            return this.Cells[x][y];
        }

        public bool SetCell(int x, int y, Cell cell){
            this.Cells[x][y] = cell;
            return true;
        }
    }
}
