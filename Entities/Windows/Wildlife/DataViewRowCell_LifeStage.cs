using Verse;

namespace BigMod.Entities.Windows.Wildlife
{
    public class DataViewRowCell_LifeStage : DataViewRowCell_Data
    {
        public Pawn Pawn;
        public Image Image;
        public DevelopmentalStage Stage
        {
            get
            {
                return Pawn.DevelopmentalStage;
            }
        }

        public DataViewRowCell_LifeStage(Pawn Pawn)
        {
            this.Pawn = Pawn;

            Image = new Image();
            Image.InheritParentSize = true;
            Image.ScaleMode = ScaleMode.ScaleToFit;
            Image.Style.Color = Color.white;
            AddChild(Image);
        }

        public override void Pull()
        {
            Image.IsVisible = (Stage != DevelopmentalStage.None);

            if (Image.IsVisible)
            {
                Image.SetTexture(Globals.TryGetTexturePathFromAlias(Pawn.DevelopmentalStage.ToString()));
                ToolTipText = Globals.PawnColumnWorker_LifeStage_GetIconTip(Pawn);
            }
            else
            {
                ToolTipText = string.Empty;
            }
        }

        public override int CompareTo(DataViewRowCell_Data Other)
        {
            return Pawn.DevelopmentalStage.CompareTo(((DataViewRowCell_LifeStage)Other).Pawn.DevelopmentalStage);
        }
    }
}
