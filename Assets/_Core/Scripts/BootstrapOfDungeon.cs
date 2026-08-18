using TimeControll;
using Zenject;

public class BootstrapOfDungeon : MonoInstaller
{
    public override void InstallBindings() => InitializeAndBindServices(Container);

    private void InitializeAndBindServices(DiContainer container)
    {
        container.BindInterfacesAndSelfTo<TimeController>().FromNew().AsSingle().NonLazy();

        container.BindInterfacesAndSelfTo<DungeonGameDirector>().FromNew().AsSingle().NonLazy();
    }
}
