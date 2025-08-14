using UnityEngine;
using XProxy.Objects.Components;

namespace XProxy.Objects;

public class TextToyObject : SpawnableObject
{
    public string Text
    {
        get => TextToy.Text;
        set => TextToy.Text = value;
    }

    public Vector2 DisplaySize
    {
        get => TextToy.DisplaySize;
        set => TextToy.DisplaySize = value;
    }

    public TextToyComponent TextToy { get; private set; }

    public TextToyObject(World world, string text) : base(world, null, 162530276)
    {
        WithPayload = true;

        TextToy = new TextToyComponent(this);

        Behaviours = new[]
        {
            TextToy,
        };

        Text = text;
    }
}
