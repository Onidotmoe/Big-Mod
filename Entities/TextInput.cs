namespace BigMod.Entities
{
    public enum TextInputStyle
    {
        /// <summary>
        /// Text with Style defined in <see cref="WindowManager.GUISkin"/>.
        /// </summary>
        Text,
        /// <summary>
        /// Only allows Integer number input.
        /// </summary>
        Numeric,
        /// <summary>
        /// Only allows Float number input.
        /// </summary>
        Numeric_Float
    }

    public class TextInput : Panel
    {
        /// <summary>
        /// Used to detect when the user has stopped typing.
        /// </summary>
        private System.Timers.Timer TextTimer = new System.Timers.Timer(WindowManager.TextInputDelay);
        /// <summary>
        /// Called when <see cref="Text"/> has changed.
        /// </summary>
        public virtual event EventHandler OnTextChanged;
        /// <summary>
        /// Called when the user has not stopped typing and threshold defined in <see cref="WindowManager.TextInputDelay"/> has passed.
        /// </summary>
        public virtual event EventHandler OnTypingFinished;
        private string _Text = string.Empty;
        private TextInputStyle _RenderStyle = TextInputStyle.Text;
        public TextInputStyle RenderStyle
        {
            get
            {
                return _RenderStyle;
            }
            set
            {
                if (_RenderStyle != value)
                {
                    _RenderStyle = value;

                    switch (_RenderStyle)
                    {
                        case TextInputStyle.Text:
                            Style.TextAnchor = TextAnchor.MiddleLeft;
                            break;

                        case TextInputStyle.Numeric:
                            Style.TextAnchor = TextAnchor.MiddleRight;

                            if (string.IsNullOrWhiteSpace(_Text))
                            {
                                _Text = "0";
                            }

                            break;

                        case TextInputStyle.Numeric_Float:
                            Style.TextAnchor = TextAnchor.MiddleRight;

                            if (string.IsNullOrWhiteSpace(_Text))
                            {
                                _Text = "0.0";
                            }

                            break;
                    }
                }
            }
        }
        /// <summary>
        /// Minimum value when <see cref="RenderStyle"/> is <see cref="TextInputStyle.Numeric"/> or <see cref="TextInputStyle.Numeric_Float"/>.
        /// </summary>
        public float Min;
        /// <summary>
        /// Maximum value when <see cref="RenderStyle"/> is <see cref="TextInputStyle.Numeric"/> or <see cref="TextInputStyle.Numeric_Float"/>.
        /// </summary>
        public float Max;
        private int _Numeric;
        public int Numeric
        {
            get
            {
                return _Numeric;
            }
            set
            {
                if (_Numeric != value)
                {
                    _Numeric = (int)Mathf.Clamp(value, Min, Max);
                    _Text = _Numeric.ToString();
                    DoOnTextChanged(this, EventArgs.Empty);
                }
            }
        }
        private float _Numeric_Float;
        public float Numeric_Float
        {
            get
            {
                return _Numeric_Float;
            }
            set
            {
                if (_Numeric_Float != value)
                {
                    _Numeric_Float = Mathf.Clamp(value, Min, Max);
                    _Text = _Numeric_Float.ToString();
                    DoOnTextChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Text inside the Input Entity.
        /// </summary>
        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                if (_Text != value)
                {
                    // Text is not allowed to be null when using GUI.TextField. Widgets.TextField checks for null beforehand too.
                    _Text = (value ?? string.Empty);

                    switch (RenderStyle)
                    {
                        case TextInputStyle.Text:
                            {
                                break;
                            }
                        case TextInputStyle.Numeric:
                            {
                                // We're using reflection to get access to this private static method.
                                Globals.Widgets_ResolveParseNow<int>(_Text, ref _Numeric, ref _Text, Min, Max, false);
                                _Text = _Numeric.ToString();
                                break;
                            }
                        case TextInputStyle.Numeric_Float:
                            {
                                Globals.Widgets_ResolveParseNow<float>(_Text, ref _Numeric_Float, ref _Text, Min, Max, false);
                                _Text = _Numeric_Float.ToString();
                                break;
                            }
                    }

                    DoOnTextChanged(this, EventArgs.Empty);
                }
            }
        }

        public TextInput()
        {
            TextTimer.Elapsed += TextTimerDone;

            Style.DrawBackground = true;
            Style.TextAnchor = TextAnchor.MiddleLeft;
            Style.TextOffset = new Vector2(2f, 0f);
        }

        public TextInput(TextInputStyle Style) : this()
        {
            RenderStyle = Style;
        }

        public TextInput(TextInputStyle Style, float Min, float Max) : this(Style)
        {
            this.Min = Min;
            this.Max = Max;
        }

        public override void Draw()
        {
            if (IsVisible)
            {
                base.Draw();

                // Verse.Text.CurTextFieldStyle is private and can't be easily changed.
                GUI.skin.textField.font = Style.Font;
                GUI.skin.textField.alignment = Style.TextAnchor;
                GUI.skin.textField.wordWrap = Style.WordWrap;
                GUI.skin.textField.contentOffset = Style.TextOffset;

                GUI.color = GetActiveTextColor();

                // Leave the switch incase we want to add more options later.
                switch (RenderStyle)
                {
                    case TextInputStyle.Text:
                        Text = GUI.TextField(Bounds, Text);
                        break;

                    case TextInputStyle.Numeric:
                    case TextInputStyle.Numeric_Float:
                        // Widgets.TextFieldNumeric<int>(Bounds, ref _Numeric, ref Buffer, Min, Max) doesn't allow us to easily use our own style.
                        Text = GUI.TextField(Bounds, Text);
                        break;
                }

                GUI.color = Color.white;
            }
        }

        public virtual void DoOnTextChanged(object Sender, EventArgs EventArgs)
        {
            TextTimer.Stop();
            TextTimer.Start();

            OnTextChanged?.Invoke(Sender, EventArgs);
        }

        private void TextTimerDone(object Sender, System.Timers.ElapsedEventArgs EventArgs)
        {
            TextTimer.Stop();
            DoOnTypingFinished(this, EventArgs);
        }

        // TODO: when ListView is filtered, this entity becomes unfocused.
        // TODO: Pressing Enter while this is focused should force this event:
        public virtual void DoOnTypingFinished(object Sender, EventArgs EventArgs)
        {
            OnTypingFinished?.Invoke(Sender, EventArgs);
        }

        public override void DoOnMouseWheel(object Sender, MouseEventArgs EventArgs)
        {
            base.DoOnMouseWheel(Sender, EventArgs);

            if ((RenderStyle == TextInputStyle.Numeric) || (RenderStyle == TextInputStyle.Numeric_Float))
            {
                if (RenderStyle == TextInputStyle.Numeric)
                {
                    Numeric += (int)EventArgs.Delta;
                }
                else if (RenderStyle == TextInputStyle.Numeric_Float)
                {
                    Numeric_Float += EventArgs.Delta;
                }
            }
        }

        /// <summary>
        /// Clears the current Text.
        /// </summary>
        public virtual void Clear()
        {
            Text = string.Empty;
        }
    }
}
