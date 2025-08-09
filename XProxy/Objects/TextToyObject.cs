using XProxy.Objects.Components;

namespace XProxy.Objects
{
    public class TextToyObject : SpawnableObject
    {
        public string Text
        {
            get => TextToy.Text;
            set => TextToy.Text = value;
        }

        public TextToyComponent TextToy { get; private set; }

        public TextToyObject(string text, uint networkId) : base(false, false, networkId, 162530276, 0)
        {
            TextToy = new TextToyComponent(this);
            Behaviours = new[]
            {
                TextToy,
            };
        }
    }
}
