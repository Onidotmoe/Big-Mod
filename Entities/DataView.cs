using System.Threading.Tasks;
using Verse;

namespace BigMod.Entities
{
    // TODO: Somesort of grouping without having each row having children, like listview in winform or wpf
    /// <summary>
    /// <para>A 2D Datatable whereas a DataView is 1D.</para>
    /// <para>Uses Cells instead of Panels.</para>
    /// </summary>
    public class DataView : Panel
    {
        public event EventHandler OnCellsChanged;
        public event EventHandler OnScrollPositionChanged;
        public IList<DataViewRowCell> Cells
        {
            get
            {
                return Rows.SelectMany((F) => F.Cells).ToList().AsReadOnly();
            }
        }
        public List<Panel> _NonCellChildren = new List<Panel>();
        public bool ShowScrollbarHorizontal;
        public bool ShowScrollbarVertical;
        /// <summary>
        /// Cells in a 2 dimensional YX array.
        /// </summary>
        /// <remarks>Cells do not use Anchoring.</remarks>
        public List<DataViewRow> Rows = new List<DataViewRow>();
        private Vector2 _ScrollPosition;
        public Rect ViewPortBounds_Visible;
        /// <summary>
        /// Header where all the Titles of the Columns and their sorting buttons are stored.
        /// </summary>
        /// <remarks>
        /// <para>Cells do not use Anchoring.</para>
        /// <para>Headers are rendered separately from the other Cells.</para>
        /// </remarks>
        public List<Panel> Header = new List<Panel>();
        /// <summary>
        /// Width of the Horizontal Scrollbar and Height of the Vertical Scrollbar.
        /// </summary>
        public float ScrollbarSize = 16f;
        /// <summary>
        /// Items that are not visible through the <see cref="ViewPortBounds_Visible"/> rectangle will not be updated or drawn.
        /// </summary>
        public bool VirtualizeItems = true;
        public bool ShowGridLines = true;
        public float GridLineThickness = 2f;
        public Color GridLineColor = Globals.GetColor("DataView.GridLineColor");

        public Vector2 ScrollPosition
        {
            get
            {
                return _ScrollPosition;
            }
            set
            {
                if (_ScrollPosition != value)
                {
                    _ScrollPosition = value;
                    Update_ViewPortBounds_Visible();
                    DoOnScrollPositionChanged();
                }
            }
        }
        /// <summary>
        /// Should GUI Scrolling be used for <see cref="ScrollPosition"/> with MouseWheel?
        /// </summary>
        public bool IgnoreGUIScroll;
        public bool VisibleOnMouseOverOnly_Scrollbars;

        private void Update_ViewPortBounds_Visible()
        {
            Vector2 ScrollPositionSize = (_ViewPortBounds.size - ScrollPosition);

            ViewPortBounds_Visible = new Rect(ScrollPosition, new Vector2(Mathf.Min(Width, (ScrollPositionSize.x + Width)), Mathf.Min(Height, (ScrollPosition.y + Height))));

            UpdateVirtualization();
        }

        private Rect _ViewPortBounds;

        /// <summary>
        /// Used for Clipping Items when they overflow the DataView's Bounds.
        /// </summary>
        public Rect ViewPortBounds
        {
            get
            {
                return _ViewPortBounds;
            }
            set
            {
                if (_ViewPortBounds != value)
                {
                    // ViewPort can never be smaller than the DataView
                    _ViewPortBounds.size = Vector2.Max(Bounds.size, value.size);
                    _ViewPortBounds.position = value.position;
                    _ViewPortBounds.size = new Vector2((_ViewPortBounds.size.x - ScrollbarSize), (_ViewPortBounds.size.y - ScrollbarSize));

                    Update_ViewPortBounds_Visible();
                }
            }
        }
        /// <summary>
        /// Height of Cells. Width is determined by the Column Header of each Cell.
        /// </summary>
        public float CellHeight = 25f;
        /// <summary>
        /// Margin between Rows.
        /// </summary>
        public float CellMarginY = 1f;
        public float HeaderHeight = 50f;
        /// <summary>
        /// Margin between Columns.
        /// </summary>
        public float HeaderMarginX = 0f;
        /// <summary>
        /// Alternates the background color of Rows using <see cref="ColorEven"/> and <see cref=" ColorOdd"/>.
        /// </summary>
        public bool AlternateColors;
        /// <summary>
        /// Every Even Row will have this color applied to help differentiate between Rows.
        /// </summary
        public Color ColorEven = Globals.GetColor("DataView.ColorEven");
        /// <summary>
        /// Every odd Row will have this color applied to help differentiate between Rows.
        /// </summary>
        public Color ColorOdd = Globals.GetColor("DataView.ColorOdd");

        public DataViewRow this[int Y]
        {
            get
            {
                return Rows[Y];
            }
            set
            {
                Rows[Y] = value;
            }
        }

        public DataView(float Width, float Height, bool ShowScrollbarVertical = false, bool ShowScrollbarHorizontal = false)
        {
            Size = new Vector2(Width, Height);

            this.ShowScrollbarVertical = ShowScrollbarVertical;
            this.ShowScrollbarHorizontal = ShowScrollbarHorizontal;
        }

        public override void Draw()
        {
            if (IsVisible)
            {
                DrawUnderlays();

                foreach (Panel Child in _NonCellChildren)
                {
                    Child.Draw();
                }

                float Old_fixedHeight = GUI.skin.verticalScrollbar.fixedHeight;
                float Old_fixedWidth = GUI.skin.verticalScrollbar.fixedWidth;

                if (VisibleOnMouseOverOnly_Scrollbars && !IsMouseOver)
                {
                    GUI.skin.verticalScrollbar.fixedHeight = 0;
                    GUI.skin.verticalScrollbar.fixedWidth = 0;
                }

                ScrollPosition = GUI.BeginScrollView(Bounds, ScrollPosition, ViewPortBounds, ShowScrollbarHorizontal, ShowScrollbarVertical);

                if (VisibleOnMouseOverOnly_Scrollbars)
                {
                    GUI.skin.verticalScrollbar.fixedHeight = Old_fixedHeight;
                    GUI.skin.verticalScrollbar.fixedWidth = Old_fixedWidth;
                }

                GUI.BeginGroup(new Rect(0, 0, ViewPortBounds.width, HeaderHeight));

                // Headers can exceed the Entity's Bounds and therefor need to be inside the ScrollView, also would be offset relative to the ScrollView's items when it move horizontally.
                foreach (Panel Cell in Header)
                {
                    Cell.Draw();
                }

                GUI.EndGroup();

                for (int i = 0; i < Rows.Count; i++)
                {
                    DataViewRow Row = Rows[i];

                    if (AlternateColors)
                    {
                        Row.Style.BackgroundColor = (((i % 2) == 0) ? ColorEven : ColorOdd);
                    }

                    Row.Draw();
                }

                DrawGridLines();

                GUI.EndScrollView(!IgnoreGUIScroll);
                DrawBorder();
                DrawMouseOver();
                DrawDisabledOverlay();
            }
        }

        public void DrawGridLines()
        {
            if (ShowGridLines)
            {
                // TODO: ViewPortBounds.height isn't enough??
                for (int X = 0; X < Header.Count; X++)
                {
                    GUI.DrawTexture(new Rect((Header[X].Right - (GridLineThickness / 2)), 0f, GridLineThickness, ViewPortBounds.height), BaseContent.WhiteTex, ScaleMode.StretchToFill, true, 1f, GridLineColor, 0f, 0f);
                }

                // Draw one underneath the Header
                GUI.DrawTexture(new Rect(0f, HeaderHeight - (GridLineThickness / 2), ViewPortBounds.width, GridLineThickness), BaseContent.WhiteTex, ScaleMode.StretchToFill, true, 1f, GridLineColor, 0f, 0f);

                for (int Y = 0; Y < Rows.Count; Y++)
                {
                    GUI.DrawTexture(new Rect(0f, (HeaderHeight + ((Y + 1) * CellHeight) + ((Y + 1) * CellMarginY) - (GridLineThickness / 2)), ViewPortBounds.width, GridLineThickness), BaseContent.WhiteTex, ScaleMode.StretchToFill, true, 1f, GridLineColor, 0f, 0f);
                }
            }
        }

        public virtual void Header_OnMouseEnter(object Sender, MouseEventArgs EventArgs)
        {
            ToggleColumnHightlight(Header.IndexOf((Panel)Sender), true);
        }

        public virtual void Header_OnMouseLeave(object Sender, MouseEventArgs EventArgs)
        {
            ToggleColumnHightlight(Header.IndexOf((Panel)Sender), false);
        }

        public void ToggleColumnHightlight(int X, bool Toggle)
        {
            foreach (DataViewRow Row in Rows)
            {
                Row[X].ForceDrawMouseOver = Toggle;
            }
        }

        #region "Cell Manipulation"

        public void SetRowSizeAndPosition(ref DataViewRow Row)
        {
            // X is always 0 because Rows start from the TopLeft.
            Row.X = 0f;
            Row.Y = (HeaderHeight + (Row.Index * CellHeight) + (Row.Index * CellMarginY));
            Row.Width = (Header.Any() ? Header.Last().Right : Row.MinSize.x);
            Row.Height = CellHeight;
        }

        /// <summary>
        /// Updates the Visibility of Items relative to their position in the visible ViewPort.
        /// </summary>
        public void UpdateVirtualization()
        {
            if (VirtualizeItems)
            {
                // Parallel.For goes from Inclusive to Exclusive.
                Parallel.For(0, Header.Count, X =>
                {
                    Header[X].IsVisible = ViewPortBounds_Visible.Overlaps(Header[X].Bounds);
                });

                Parallel.For(0, Rows.Count, Y =>
                {
                    for (int X = 0; X < Rows[Y].Count; X++)
                    {
                        Panel Cell = Rows[Y][X];
                        Cell.IsVisible = ViewPortBounds_Visible.Overlaps(new Rect(Cell.X, (HeaderHeight + (Y * CellHeight) + (Y * CellMarginY)), Cell.Width, Cell.Height));
                    }
                });
            }
        }

        public DataViewRow GetRow(int Y)
        {
            return Rows[Y];
        }

        public List<DataViewRowCell> GetColumn(int X)
        {
            return Rows.Select((F) => F.Cells[X]).ToList();
        }

        public Panel GetCell(int X, int Y)
        {
            return Rows[Y].Cells[X];
        }

        public void AddRow(DataViewRow Row)
        {
            // Index is 0 based, Count is not, this will always give the Last Index we want to appened to.
            Row.Index = Rows.Count;

            SetRowSizeAndPosition(ref Row);

            Row.DataView = this;
            Rows.Add(Row);
            Children.Add(Row);
            Register(Row);

            DoOnCellsChanged();
        }

        public void AddColumn(Panel Column)
        {
            Column.UseAnchoring = false;
            // Y is always 0 because it's inside a GUI.BeginGroup.
            Column.Position = new Vector2((Header.Any() ? (Header.Last().Right + HeaderMarginX) : 0f), 0f);

            Header.Add(Column);
            Children.Add(Column);
            Register(Column);
            Column.OnMouseEnter += Header_OnMouseEnter;
            Column.OnMouseLeave += Header_OnMouseLeave;

            int Header_Index = (Header.Count - 1);

            // Add Empty cells to all Rows if they don't have enough.
            for (int Y = 0; Y < Rows.Count; Y++)
            {
                if (Rows[Y].Count < Header.Count)
                {
                    Rows[Y].AddCell(Header_Index, new DataViewRowCell());
                }
            }

            DoOnCellsChanged();
        }

        public void RemoveColumn(Panel Column)
        {
            int X = Header.IndexOf(Column);
            Header.Remove(Column);
            Children.Remove(Column);
            UnRegister(Column);
            Column.OnMouseEnter -= Header_OnMouseEnter;
            Column.OnMouseLeave -= Header_OnMouseLeave;

            // Remove cells associated with this Column.
            for (int Y = 0; Y < Rows.Count; Y++)
            {
                if (Rows[Y].Count < Header.Count)
                {
                    Rows[Y].RemoveCellAt(X);
                }
            }

            DoOnCellsChanged();
        }

        public void RemoveColumnAt(int X)
        {
            RemoveColumn(Header[X]);
        }

        public void RemoveRow(DataViewRow Row)
        {
            Row.DataView = null;
            Rows.Remove(Row);
            Children.Remove(Row);
            UnRegister(Row);

            UpdatePositions();

            DoOnCellsChanged();
        }

        public void RemoveCell(DataViewRowCell Cell)
        {
            Vector2 Position = IndexOf(Cell);

            RemoveCellAt((int)Position.x, (int)Position.y);
        }

        public void RemoveCellAt(int X, int Y)
        {
            Rows[Y].RemoveCellAt(X);

            DoOnCellsChanged();
        }

        public void UpdatePositions()
        {
            for (int X = 0; X < Header.Count; X++)
            {
                // Y is always 0 because it's inside a GUI.BeginGroup.
                Header[X].Position = new Vector2(((X > 0) ? (Header[X - 1].Right + HeaderMarginX) : 0f), 0f);
            }

            Parallel.For(0, Rows.Count, Y =>
            {
                Rows[Y].Index = Y;
                Rows[Y].Position = new Vector2(0f, (HeaderHeight + (Y * CellHeight) + (Y * CellMarginY)));

                for (int X = 0; X < Rows[Y].Count; X++)
                {
                    // Y is always 0 because it's inside a GUI.BeginGroup.
                    Rows[Y][X].Position = new Vector2(Header[X].X, 0f);
                }
            });
        }

        public void UpdateSizes()
        {
            Parallel.For(0, Rows.Count, Y =>
            {
                for (int X = 0; X < Rows[Y].Count; X++)
                {
                    Rows[Y][X].Size = new Vector2(Header[X].Width, CellHeight);
                }
            });
        }

        public Vector2 IndexOf(Panel Cell)
        {
            for (int Y = 0; Y < Rows.Count; Y++)
            {
                for (int X = 0; X < Rows[Y].Count; X++)
                {
                    if (Rows[Y][X] == Cell)
                    {
                        return new Vector2(X, Y);
                    }
                }
            }

            return default;
        }

        public int IndexOf(DataViewRow Row)
        {
            return Rows.IndexOf(Row);
        }

        public void RemoveRowAt(int Y)
        {
            RemoveRow(Rows[Y]);
        }

        public void InsertRow(int Y, DataViewRow Row)
        {
            AddRow(Row);

            // Don't bother changing its position if it's out of bounds.
            // Index is Zero based, Count is not.
            if ((Y >= 0) && (Y <= (Rows.Count - 1)))
            {
                Rows.Remove(Row);
                Rows.Insert(Y, Row);

                // Will handle setting the right Y Index on Rows for us.
                UpdatePositions();
            }
        }

        public virtual void DoOnCellsChanged()
        {
            UpdateViewPortBounds();

            OnCellsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Removes all Rows.
        /// </summary>
        public void Clear()
        {
            for (int Y = (Rows.Count - 1); Y >= 0; Y--)
            {
                RemoveRowAt(Y);
            }
        }

        /// <summary>
        /// Insertion of non-Item children isn't respected in a DataView.
        /// </summary>
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

        public Panel GetHeaderByID(string ID)
        {
            return Header.FirstOrDefault((F) => (F.ID == ID));
        }

        public int GetHeaderIndexByID(string ID)
        {
            return Header.FindIndex((F) => (F.ID == ID));
        }

        #endregion "Cell Manipulation"

        #region "Positioning & Sizing"

        public override void DoOnSizeChanged(object Sender, EventArgs EventArgs)
        {
            base.DoOnSizeChanged(Sender, EventArgs);

            UpdateViewPortBounds();
        }
        public void DoOnScrollPositionChanged()
        {
            OnScrollPositionChanged?.Invoke(this, EventArgs.Empty);
        }
        public void UpdateViewPortBounds()
        {
            ViewPortBounds = new Rect(ViewPortBounds.position.x, ViewPortBounds.position.y, (Header.Any() ? Header.Max((F) => F.Right) : 0f), (HeaderHeight + (Rows.Count - 1) * CellHeight) + ((Rows.Count - 1) * CellMarginY));
        }

        public override bool GetMouseOver(Vector2 MousePosition, Vector2 ChildOffset = default)
        {
            // Modified from Panel.GetMouseOver
            if (!IgnoreMouse && Bounds.Contains(MousePosition))
            {
                IsMouseOver = true;

                if (CanCascadeMouseInput())
                {
                    MousePosition += ChildOffset;

                    foreach (Panel Child in _NonCellChildren)
                    {
                        Child.GetMouseOver(MousePosition);
                    }

                    // Only Cells are affected by ScrollPosition, DataView position has to be subtracted
                    MousePosition += ScrollPosition - Position;

                    foreach (Panel Cell in Header)
                    {
                        Cell.GetMouseOver(MousePosition);
                    }

                    // We check if the Mouse is in the Correct Y axis and then Check Row Cells from there.
                    foreach (DataViewRow Row in Rows)
                    {
                        // Cell's Children's Y is always 0 because it's in a GUI.BeginGroup.
                        // Y has to be subtracted to compensate for this.
                        Row.GetMouseOver(MousePosition, new Vector2(0f, -(HeaderHeight + (Row.Index * CellHeight) + (Row.Index * CellMarginY))));
                    }
                }
            }
            else
            {
                IsMouseOver = false;
            }

            return IsMouseOver;
        }

        #endregion "Positioning & Sizing"
    }
}
