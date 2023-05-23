using Verse;

namespace BigMod.Entities.Windows.Orders
{
    public class ListViewItem_Order : ListViewItem
    {
        public Orders Orders;
        public Gizmo Gizmo;
        public KeyCode Key;
        public List<Gizmo> Grouping = new List<Gizmo>();

        /// <summary>
        /// Gizmo proxy Item.
        /// </summary>
        /// <param name="Orders">Orders Host Window.</param>
        /// <param name="Gizmo">Gizmo to convert to a Item.</param>
        public ListViewItem_Order(Orders Orders, Gizmo Gizmo)
        {
            this.Orders = Orders;
            this.Gizmo = Gizmo;

            Grouping.Add(Gizmo);

            Size = new Vector2(Gizmo.GetWidth(120f), Mathf.Max(Gizmo.Height, Orders.ItemMinSize.y));

            IsEnabled = !Gizmo.disabled;
            Command Command = (Gizmo as Command);

            Style.DrawMouseOver = false;
            Header.Style.DrawMouseOver = false;
            Header.Label.RenderStyle = LabelStyle.GUI;
            Header.Label.Style.FontType = GameFont.Tiny;
            Header.Label.Style.TextAnchor = TextAnchor.UpperLeft;
            Header.Label.Style.TextOffset = new Vector2(4f, 4f);

            if ((Command != null) && (Command.hotKey != null) && (Command.hotKey.MainKey != KeyCode.None))
            {
                Key = Command.hotKey.MainKey;
                Text = Command.hotKey.MainKey.ToStringReadable();
            }
        }

        public override void Update()
        {
            base.Update();

            if (IsVisible)
            {
                if ((Key != KeyCode.None) && Input.GetKeyDown(Key))
                {
                    Gizmo.ProcessInput(Event.current);
                    Orders.SelectionChanged = true;
                }
            }
        }

        public override void Draw()
        {
            GizmoResult GizmoResult = Gizmo.GizmoOnGUI(Position, Width, default);

            if (GizmoResult.InteractEvent != null)
            {
                Grouping.ForEach((F) => F.ProcessInput(Event.current));
                Event.current.Use();
                Orders.SelectionChanged = true;
            }

            GenUI.AbsorbClicksInRect(Bounds);

            base.Draw();
        }

        public virtual bool TryMerge(Gizmo Gizmo)
        {
            if (this.Gizmo.GroupsWith(Gizmo))
            {
                Grouping.Add(Gizmo);
                this.Gizmo.MergeWith(Gizmo);

                return true;
            }

            return false;
        }
    }
}
