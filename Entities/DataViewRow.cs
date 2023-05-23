namespace BigMod.Entities
{
    public class DataViewRow : Panel
    {
        /// <summary>
        /// Is this Item currently Selected in its parent DataView?
        /// </summary>
        public override bool IsSelected { get; set; }

        public List<Panel> _NonCellChildren = new List<Panel>();
        public List<DataViewRowCell> Cells = new List<DataViewRowCell>();
        public event EventHandler OnCellsChanged;
        /// <summary>
        /// They Y Index of this Row inside the DataView.
        /// </summary>
        public int Index;
        private DataView _DataView;
        /// <summary>
        /// Called before DataView is removed.
        /// </summary>
        public event EventHandler OnDataViewRemoved;
        /// <summary>
        /// Called after DataView is added.
        /// </summary>
        public event EventHandler OnDataViewAdded;
        /// <summary>
        /// Parent DataView.
        /// </summary>
        public DataView DataView
        {
            get
            {
                return _DataView;
            }
            set
            {
                if (_DataView != value)
                {
                    if ((_DataView == null) && (value != null))
                    {
                        _DataView = value;
                        DoOnDataViewAdded();
                    }
                    else if ((_DataView != null) && (_DataView != value))
                    {
                        DoOnDataViewRemoved();
                        _DataView = value;
                    }
                }
                else
                {
                    _DataView = value;
                }
            }
        }

        /// <summary>
        /// Called before DataView is removed.
        /// </summary>
        public virtual void DoOnDataViewRemoved()
        {
            OnDataViewRemoved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called after DataView is added.
        /// </summary>
        public virtual void DoOnDataViewAdded()
        {
            OnDataViewAdded?.Invoke(this, EventArgs.Empty);
        }

        public DataViewRowCell this[int X]
        {
            get
            {
                return Cells[X];
            }
            set
            {
                Cells[X] = value;
            }
        }
        public int Count
        {
            get
            {
                return Cells.Count;
            }
        }

        public DataViewRow()
        {
            Style.DrawBackground = true;
            Style.DrawMouseOver = true;
            // DataView items don't use Anchoring.
            UseAnchoring = false;
        }

        /// <summary>
        /// Create a new DataViewRow with empty cells that match the Width of the Header Cells.
        /// </summary>
        /// <param name="DataView">DataView to add to, and get Headers from, and get CellHeight from.</param>
        public DataViewRow(DataView DataView) : this()
        {
            DataView.AddRow(this);

            for (int X = 0; X < DataView.Header.Count; X++)
            {
                AddCell(X, new DataViewRowCell());
            }
        }

        public override void Draw()
        {
            if (IsVisible)
            {
                DrawUnderlays();

                GUI.BeginGroup(Bounds);

                foreach (Panel Child in _NonCellChildren)
                {
                    Child.Draw();
                }

                foreach (DataViewRowCell Cell in Cells)
                {
                    Cell.Draw();
                }

                GUI.EndGroup();

                // These use Bound with non-zero Ys.
                DrawOverlays();
            }
        }

        #region "Cell Manipulation"

        public virtual int IndexOf(DataViewRowCell Cell)
        {
            return Cells.IndexOf(Cell);
        }

        public void SetCellSizeAndPosition(int HeaderX, ref DataViewRowCell Cell)
        {
            Panel Header = DataView.Header[HeaderX];

            Cell.X = Header.X;
            // Y is always 0 because it's inside a GUI.BeginGroup.
            Cell.Y = 0f;
            Cell.Width = Header.Width;
            Cell.Height = DataView.CellHeight;
        }

        /// <summary>
        /// Replaces the Current Cell at the specified Index with the given Cell.
        /// </summary>
        /// <param name="HeaderX">Index of the Cell to replace, also the Index of the Header.</param>
        /// <param name="Cell">Cell to Replace with.</param>
        public virtual void ReplaceCell(int HeaderX, DataViewRowCell Cell)
        {
            RemoveCellAt(HeaderX);
            InsertCell(HeaderX, Cell);
        }

        public virtual void AddCell(int HeaderX, DataViewRowCell Cell)
        {
            Cell.DataView = DataView;
            Cell.DataViewRow = this;

            SetCellSizeAndPosition(HeaderX, ref Cell);
            Cell.UseAnchoring = false;

            Cell.Index = HeaderX;

            Cells.Add(Cell);
            Children.Add(Cell);
            Register(Cell);

            DoOnCellsChanged();
        }

        public virtual void RemoveCellAt(int X)
        {
            RemoveCell(Cells[X]);
        }

        public virtual void RemoveCell(DataViewRowCell Cell)
        {
            Cell.DataView = null;
            Cell.DataViewRow = null;

            Cells.Remove(Cell);
            Children.Remove(Cell);
            UnRegister(Cell);

            DoOnCellsChanged();
        }

        public virtual void InsertCell(int X, DataViewRowCell Cell)
        {
            Cell.DataView = DataView;
            Cell.DataViewRow = this;

            SetCellSizeAndPosition(X, ref Cell);
            Cell.UseAnchoring = false;

            Cell.Index = X;

            Cells.Insert(X, Cell);
            Children.Add(Cell);
            Register(Cell);

            DoOnCellsChanged();
        }

        public virtual void DoOnCellsChanged()
        {
            OnCellsChanged?.Invoke(this, EventArgs.Empty);
        }

        public override void InsertChild(int Index, Panel Child)
        {
            base.InsertChild(Index, Child);

            _NonCellChildren.Add(Child);
        }

        public override void AddChild(Panel Child)
        {
            base.AddChild(Child);

            _NonCellChildren.Add(Child);
        }

        public override void RemoveChild(Panel Child)
        {
            base.RemoveChild(Child);

            _NonCellChildren.Remove(Child);
        }

        /// <summary>
        /// Removes this Row from its DataView parent.
        /// </summary>
        public virtual void RemoveFromRowParent()
        {
            if (DataView != null)
            {
                DataView.RemoveRow(this);
            }
        }

        #endregion "Cell Manipulation"
    }
}
