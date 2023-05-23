namespace BigMod.Entities
{
    public class DataViewRowCell : Panel
    {
        /// <summary>
        /// X index inside its DataViewRow.
        /// </summary>
        public int Index;

        public DataViewRowCell()
        {
            // DataView items don't use Anchoring.
            UseAnchoring = false;

            Style.DrawMouseOver = true;
        }

        /// <summary>
        /// Creates a new DataViewRowCell with the given Index.
        /// </summary>
        /// <param name="Index">Index inside the DataViewRow.</param>
        public DataViewRowCell(int Index) : this()
        {
            this.Index = Index;
        }

        public virtual void RemoveFromCellParent()
        {
            if (DataViewRow != null)
            {
                DataViewRow.RemoveCell(this);
            }
            if (DataView != null)
            {
                DataView.RemoveCell(this);
            }
        }

        public override Vector2 GetAbsolutePosition()
        {
            return ((ParentWindow?.Position ?? Vector2.zero) + Position + (DataViewRow?.Position ?? Vector2.zero));
        }

        #region "Events & Callbacks"

        /// <summary>
        /// Called before DataViewRow is removed.
        /// </summary>
        public event EventHandler OnDataViewRowRemoved;
        /// <summary>
        /// Called after DataViewRow is added.
        /// </summary>
        public event EventHandler OnDataViewRowAdded;
        private DataViewRow _DataViewRow;
        public DataViewRow DataViewRow
        {
            get
            {
                return _DataViewRow;
            }
            set
            {
                if (_DataViewRow != value)
                {
                    if ((_DataViewRow == null) && (value != null))
                    {
                        _DataViewRow = value;
                        DoOnDataViewRowAdded();
                    }
                    else if ((_DataViewRow != null) && (_DataViewRow != value))
                    {
                        DoOnDataViewRowRemoved();
                        _DataViewRow = value;
                    }
                }
                else
                {
                    _DataViewRow = value;
                }
            }
        }

        /// <summary>
        /// Called before DataViewRow is removed.
        /// </summary>
        public virtual void DoOnDataViewRowRemoved()
        {
            OnDataViewRowRemoved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called after DataViewRow is added.
        /// </summary>
        public virtual void DoOnDataViewRowAdded()
        {
            OnDataViewRowAdded?.Invoke(this, EventArgs.Empty);
        }

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

        #endregion "Events & Callbacks"
    }
}
