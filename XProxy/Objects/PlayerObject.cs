using XProxy.Objects.Components;

namespace XProxy.Objects
{
    public class PlayerObject : SpawnableObject
    {
        public string UserId
        {
            get => PlayerAuthComponent.UserId;
            set => PlayerAuthComponent.UserId = value;
        }

        public string Nickname
        {
            get => NicknameComponent.Nickname;
            set => NicknameComponent.Nickname = value;
        }

        public PlayerRolesComponent PlayerRolesComponent { get; }
        public PlayerAuthComponent PlayerAuthComponent { get; }
        public NicknameComponent NicknameComponent { get; }

        public PlayerObject(bool isLocalPlayer, bool isOwner, uint networkId) : base(isLocalPlayer, isOwner, networkId, 3816198336, 0)
        {
            Behaviours = new BehaviourInfo[24];

            PlayerRolesComponent = new PlayerRolesComponent(this);
            Behaviours[0] = PlayerRolesComponent;

            PlayerAuthComponent = new PlayerAuthComponent(this);
            Behaviours[7] = PlayerAuthComponent;

            NicknameComponent = new NicknameComponent(this);
            Behaviours[3] = NicknameComponent;
        }
    }
}
